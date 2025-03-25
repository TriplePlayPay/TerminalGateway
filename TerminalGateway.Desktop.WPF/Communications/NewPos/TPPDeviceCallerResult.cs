using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    /// <summary>
    /// Result of a TPP device call
    /// </summary>
    public class TPPDeviceCallerResult
    {
        public bool Success { get; set; }
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public bool WasCancelled { get; set; }
        public bool TimedOut { get; set; }
    }
}
