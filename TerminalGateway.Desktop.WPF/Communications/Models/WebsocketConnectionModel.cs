using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketConnectionModel
    {
        public string ApiKey { get; set; }
        public string IpAddress { get; set; }
        public string LaneId { get; set; }
    }
}
