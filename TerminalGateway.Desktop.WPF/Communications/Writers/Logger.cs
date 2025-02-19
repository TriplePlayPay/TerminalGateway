using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Http.BatchFormatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalGateway.Desktop.WPF.Communications.Constants;
using TerminalGateway.Desktop.WPF.Communications.Database;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Rest;

namespace TerminalGateway.Desktop.WPF.Communications.Writers
{
    public class Logger
    { 
        public static void SetApiKeyAndInitializeLogging()
        {
            DatabaseManager _databaseManager = new DatabaseManager();

            string apiKey = _databaseManager.GetApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Build a user-writable path, e.g. C:\Users\<User>\AppData\Local\TriplePlayPay
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appFolder = Path.Combine(localAppData, "TriplePlayPay");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Combine the final log file path
                string logFilePath = Path.Combine(appFolder, "TriplePlayPayLog.txt");
                string bufferFilePath = Path.Combine(appFolder, "TriplePlayPayBufferLog");

                // Reconfigure with new API key
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                       new KeyValuePair<string, string>("apiKey", apiKey)
                    })
                    .Build();
                var httpClientFactory = new ApiKeyHttpClientFactory();
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(logFilePath)
                    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                        requestUri: "https://tripleplaypay.com/api/logs",
                        bufferBaseFileName: bufferFilePath,
                        batchFormatter: new ArrayBatchFormatter(),
                        textFormatter: new JsonFormatter(renderMessage: true),
                        period: TimeSpan.FromSeconds(5),
                        bufferFileSizeLimitBytes: 1_000_000,
                        restrictedToMinimumLevel: LogEventLevel.Debug,   // or whichever
                        httpClient: httpClientFactory,
                        configuration: configuration
                    )
                    .CreateLogger();

                // Optionally set the static logger
                Serilog.Log.Logger = Log.Logger;

                Log.Information("Serilog initialized with API key.");
            }
        }
    }
}
