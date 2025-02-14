using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TerminalDeleteResponseModel
    {
        [JsonPropertyName("deleted_id")]
        public string DeletedId { get; set; }
    }
}
