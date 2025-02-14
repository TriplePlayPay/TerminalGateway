using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class PingResult
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }
    }
}
