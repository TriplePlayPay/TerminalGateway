using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalGateway.Desktop.WPF.Communications.Constants;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Rest;

namespace TerminalGateway.Desktop.WPF.Communications.ApiKey
{
    public class ApiKeyManager
    {
        public static async Task<bool> IsApiKeyValid(string apiKey)
        {
            RestConfiguration restConfiguration = new RestConfiguration()
            {
                ApiKey = apiKey,
                ApiUrl = ConstantValues.ApiUrl
            };
            RestCaller restCaller = new RestCaller(restConfiguration);
            return await restCaller.IsApiKeyValid();
        }
    }
}
