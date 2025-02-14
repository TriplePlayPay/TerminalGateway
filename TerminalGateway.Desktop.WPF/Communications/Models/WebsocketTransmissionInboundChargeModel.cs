using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketTransmissionInboundChargeModel
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("laneId")]
        public string LaneId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("terminal_payment_type")]
        public string TerminalPaymentType { get; set; }
    }
}
