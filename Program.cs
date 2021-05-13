using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace EO59.Api.Demo
{
    /// <summary>
    /// Demo program to download data from API and store files on filesystem.
    /// </summary>
    internal class Program
    {
        private static IConfigurationRoot _configuration;

        private static void Main()
        {
            // Configure logging.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();
            // Initialize configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();
            // Create instance of API worker class
            var processor = new ApiReaderService(Log.Logger, _configuration["StorageBasePath"]);
            // Call work method, since it's async use .Wait()
            // for it to complete work before exiting program.
            processor.DoWork(
                _configuration["Subscription:ApiKey"],
                _configuration["Subscription:Name"]
            ).Wait();
        }
    }
}