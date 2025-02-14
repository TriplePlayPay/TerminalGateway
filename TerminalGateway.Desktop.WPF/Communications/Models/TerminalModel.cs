using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TerminalModel
    {
        [JsonPropertyName("lane_id")]
        public string LaneId { get; set; }
       
        [JsonPropertyName("terminal_ip_address")]
        public string IpAddress { get; set; }
    }
}
