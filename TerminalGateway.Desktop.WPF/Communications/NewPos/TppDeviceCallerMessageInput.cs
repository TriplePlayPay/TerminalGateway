using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    public class TppDeviceCallerMessageInput
    {
        public string MerchantKey { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
    }
}
