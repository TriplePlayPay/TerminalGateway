using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TerminalGateway.Desktop.WPF.Communications.Models;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.Rest
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public RestClient(RestConfiguration restConfiguration)
        {
            _baseUrl = restConfiguration.ApiUrl;
            _apiKey = restConfiguration.ApiKey;
            _httpClient = new HttpClient();
            SetAuthorizationHeader();
        }

        private void SetAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }


        public async Task<string> GetAsync(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + endpoint);
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Log.Error("Message from GetAsync Error :{0} ", e.Message);
                return "";
            }
        }

        public async Task<string> PostAsync(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl + endpoint, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Log.Error("Exception message from PostAsync Error :{0} ", e.Message);
                return "";
            }
        }

        public async Task<string> PatchAsync(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PatchAsync(_baseUrl + endpoint, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Log.Error("Message from PatchAsync Error :{0} ", e.Message);
                return "";
            }
        }

        public async Task<string> DeleteAsync(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(_baseUrl + endpoint);
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Log.Information("DeleteAsync Exception Message :{0} ", e.Message);
                return "";
            }
        }
    }

}
