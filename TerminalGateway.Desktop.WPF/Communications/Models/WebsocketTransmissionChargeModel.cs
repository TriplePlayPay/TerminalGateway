using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GlobalPayments.Api.Terminals.Abstractions;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketTransmissionChargeModel
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public dynamic Details { get; set; }

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }


        [JsonPropertyName("lane_id")]
        public string LaneId { get; set; }
    }
}
