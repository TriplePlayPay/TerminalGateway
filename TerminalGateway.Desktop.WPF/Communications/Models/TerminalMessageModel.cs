using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TerminalMessageModel
    {
        public string RequestId { get; set; }
        public decimal Amount { get; set; }
        public string TerminalIpAddress { get; set; }
        public string LaneId { get; set; }
        public string Action {  get; set; }
        public string TerminalPaymentType { get; set; }
        public string TerminalType { get; set; }
    }
}
