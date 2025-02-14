using System;
using System.Net;

namespace TerminalGateway.Desktop.WPF.Communications.Validators
{
    public class IpAddressValidator
    {
        public static bool IsValidIPAddress(string ipString)
        {
            return IPAddress.TryParse(ipString, out IPAddress ip);
        }
    }

}
