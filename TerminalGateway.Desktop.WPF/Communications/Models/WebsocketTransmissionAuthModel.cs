using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketTransmissionAuthModel
    {
        public string Type { get; set; }
        public string ApiKey { get; set; }
        public string LaneId { get; set; }
    }
}
