using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace TerminalGateway.Desktop.WPF.Communications.Rest
{

    public class ApiKeyHttpClientFactory : IHttpClient
    {
        private readonly HttpClient httpClient;

        public ApiKeyHttpClientFactory() => httpClient = new HttpClient();

        public void Configure(IConfiguration configuration) => httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + configuration["apiKey"]);

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream, CancellationToken cancellationToken)
        {
            using var content = new StreamContent(contentStream);
            content.Headers.Add("Content-Type", "application/json");

            var response = await httpClient
                .PostAsync(requestUri, content, cancellationToken)
                .ConfigureAwait(false);

            return response;
        }

        public void Dispose() => httpClient?.Dispose();
    }
}
