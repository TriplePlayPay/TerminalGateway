using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TerminalResponseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("lane_id")]
        public string LaneId { get; set; }

        [JsonPropertyName("terminal_type")]
        public string TerminalType { get; set; }

        [JsonPropertyName("terminal_ip_address")]
        public string IpAddress { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("tpn")]
        public string? TerminalPinNumber { get; set; }

        [JsonPropertyName("auth_key")]
        public string? AuthKey { get; set; }

        [JsonPropertyName("sn")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("activation_token")]
        public string? ActivationToken { get; set; }

        [JsonPropertyName("register_id")]
        public string? RegisterId { get; set; }
    }
}
