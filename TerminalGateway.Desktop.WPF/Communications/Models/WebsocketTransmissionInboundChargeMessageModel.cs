using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketTransmissionInboundChargeMessageModel
    {
        public string LaneId { get; set; }
        public decimal Amount { get; set; }
    }
}
