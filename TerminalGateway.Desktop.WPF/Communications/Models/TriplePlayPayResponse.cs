using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TriplePlayPayResponse<T>
    {
        [JsonPropertyName("id")]
        public string Id {  get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("message")]
        public T Message { get; set; }
    }
}
