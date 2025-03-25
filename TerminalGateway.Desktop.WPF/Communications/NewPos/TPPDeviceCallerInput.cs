using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    public class TPPDeviceCallerInput
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string MerchantKey { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public int TimeoutSeconds { get; set; } = 240;
    }
}
