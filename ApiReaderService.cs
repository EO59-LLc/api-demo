using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EO59.Api.Downloader.Models;
using Newtonsoft.Json;
using Serilog;
using EO59.Api.Downloader.Services;

namespace EO59.Api.Downloader
{
    /// <summary>
    /// This is a demo class to show data retrieval workflow for EO59 API
    /// </summary>
    public class ApiReaderService
    {
        /// <summary>
        ///  Logger instance
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Static definition of API base URL
        /// </summary>
        private const string ApiBase = "https://api.eo59.com/api/";
        /// <summary>
        /// Variable to store file system root directory location
        /// </summary>
        private readonly string _basePath;
        /// <summary>
        /// Size of data page to be downloaded, supported sizes are from 1 to
        /// 10000, currently using recommended default of 5000
        /// </summary>
        private const int DownloadPageSize = 5000;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Pass instance of Serilog logger</param>
        /// <param name="basePath">Pass variable containing file system root where
        /// data will be stored.</param>
        public ApiReaderService(ILogger logger,string basePath)
        {
            _logger = logger;
            _basePath = basePath;
        }
        /// <summary>
        /// Main workflow entry point
        /// </summary>
        /// <param name="subscription">Pass here valid subscription key</param>
        /// <param name="name">Pass here name for subscription, will be used to store cached file.</param>
        /// <returns></returns>
        public async Task DoWork(string subscription, string name )
        {
            _logger.Information("Checking API subscription {Name}",name);
            // Define main place holder for subscription information block from API.
            SubscriptionModel sub;
            try
            {
                // Download subscription information from API
                sub = JsonConvert.DeserializeObject<SubscriptionModel>(
                    await ApiRequestClient.GetString($"{ApiBase}Subscriptions?subscription={subscription}"));
            }
            catch (Exception e)
            {
                _logger.Fatal(e,"Error while reading subscription for {Name}",name);
                return;
            }
            // Check if subscription is missing.
            if (sub == null)
            {
                _logger.Error("Subscription for {Name} is empty, check key value",name);
                return;
            }
            // Check if subscription has any data sets.
            if (sub.DataSets == null || sub.DataSets.Count == 0)
            {
                _logger.Warning("Subscription {Name} has no data sets, exiting",name);
                return;
            }
            // Check cached json to check if subscription has new data.
            if (!await NeedToProcessDataSet(name, sub))
            {
                _logger.Information("No changes in data for {Name}, exiting",name);
                return;
            }
            // Start processing subscription data.
            _logger.Information("Loaded subscription {Name} data, will process {NumOfSets} data sets",
                name, sub.DataSets.Count);
            // Loop through all data sets on the subscription.
            foreach (var dataSet in sub.DataSets)
            {
                var fileName = Path.Combine(_basePath, $"{dataSet.DataSourceKey}.json");
                await DownloadDataSet(subscription, dataSet.DataSourceKey, dataSet.NumberOfPoints, fileName);
            }
            // Replace or save subscription block from API to cached file, this will be used to check if there 
            // are any updates in future requests.
            try
            {
                File.WriteAllText(GetSubscriptionCachedFileName(name),
                    JsonConvert.SerializeObject(sub));
            }
            catch (Exception e)
            {
                _logger.Error(e,"Writing subscription cache file caused exception");
            }
            _logger.Information("Completed all tasks, exiting");
        }
        /// <summary>
        /// Metod to compare cached copy of subscription block to new data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newData"></param>
        /// <returns></returns>
        private async Task<bool> NeedToProcessDataSet(string name, SubscriptionModel newData)
        {
            // build cache file name
            var cacheFileName = GetSubscriptionCachedFileName(name);
            // check if file exists, if not, signal need for processing.
            if (!File.Exists(cacheFileName)) return true;
            // load cached file into memory
            var cached = JsonConvert.DeserializeObject<SubscriptionModel>(
                await File.ReadAllTextAsync(cacheFileName)
            );
            // cached file sanity check
            if (cached != null && cached.DataSets.Count < newData.DataSets.Count) return true;
            // check if any update dates have changed since last download
            return cached != null && (from dataSet in cached.DataSets let toCheck = newData.DataSets
                    .FirstOrDefault(x => x.DataSourceKey == dataSet.DataSourceKey) 
                where toCheck != null where DateTimeOffset.Compare(dataSet.LastUpdate, toCheck.LastUpdate) != 0 
                select dataSet).Any();
        }
        /// <summary>
        /// Convert name into subscription cache file name.
        /// </summary>
        /// <param name="name">Name of subscription</param>
        /// <returns>File system reference to subscription cache file</returns>
        private string GetSubscriptionCachedFileName(string name)
        {
            return Path.Combine(_basePath, $"{name}.json");
        }
        /// <summary>
        /// Download and save data set into json file
        /// </summary>
        /// <param name="subscription">Subscription key</param>
        /// <param name="key">Data set key</param>
        /// <param name="rows">How many rows data set has</param>
        /// <param name="fileName">File name where json will be saved</param>
        /// <returns></returns>
        private async Task DownloadDataSet(string subscription, int key, int rows, string fileName)
        {
            _logger.Information("Downloading data set {Id} to {File}",key,fileName);
            // Set remaining rows count
            var remaining = rows;
            // Set start of download page, we load from 0
            var skip = 0;
            // We will collect json strings into string builder
            var data = new StringBuilder();
            // Since saved json will contain array of points, append start of array.
            data.Append('[');
            // Loop to page through data segments.
            while (remaining > 0)
            {
                _logger.Information("Remaining rows: {Rows}",remaining);
                // build API request url
                var url = $"{ApiBase}Data?subscription={subscription}&key={key}&numberOfItems={DownloadPageSize}&skip={skip}";
                // shift loop variables
                remaining -= DownloadPageSize;
                skip += DownloadPageSize;

                // read json
                var json = await ApiRequestClient.GetString(url);
                
                // Check if response has any data

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.Warning("Reading data at point {Skip} returned empty result, ignoring",skip);
                }
                else
                {
                    // returned json is list, remove list symbols to convert them into block, then append
                    json = json.Remove(json.IndexOf('['), 1);
                    json = json.Remove(json.LastIndexOf(']'), 1);
                    data.AppendLine(json);
                    if (remaining > 0)
                    {
                        data.Append(',');
                    }
                }
            }

            data.Append(']');
            var finalJson = data.ToString();
            // check if json ends in a way that will cause error
            if (finalJson.EndsWith(",]"))
            {
                // remove trailing ,
                finalJson = finalJson.Remove(finalJson.LastIndexOf(','), 1);
            }
            // write json to file
            await File.WriteAllTextAsync(fileName, finalJson);
            _logger.Information("Completed saving {File}",fileName);
        }
    }
}