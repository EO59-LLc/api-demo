using System;
using System.Threading.Tasks;

namespace EO59.Api.Downloader.Services
{
    /// <summary>
    /// This is minimalistic implementation of WebClient to download
    /// string from the URL
    /// </summary>
    internal static class ApiRequestClient
    {
        /// <summary>
        /// Run URL query and return result as string.
        /// </summary>
        /// <param name="url">Url of the request, example: https://server/path/param </param>
        /// <returns>Result of the request as string.</returns>
        public static async Task<string> GetString(string url)
        {
            using var webClient = new System.Net.WebClient();
            webClient.Headers.Set("Accept","application/json");
            return await webClient.DownloadStringTaskAsync(new Uri(url));
        }
    }
}