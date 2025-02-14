using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Http.BatchFormatters;
using System;
using System.Collections.Generic;
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
                // Reconfigure with new API key
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                       new KeyValuePair<string, string>("apiKey", apiKey)
                    })
                    .Build();
                var httpClientFactory = new ApiKeyHttpClientFactory();
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                        requestUri: "https://sandbox.tripleplaypay.com/api/logs",
                        batchFormatter: new ArrayBatchFormatter(),
                        textFormatter: new JsonFormatter(renderMessage: true),
                        period: TimeSpan.FromSeconds(5),
                        bufferFileSizeLimitBytes: 1_000_000,
                        restrictedToMinimumLevel: LogEventLevel.Debug,
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
