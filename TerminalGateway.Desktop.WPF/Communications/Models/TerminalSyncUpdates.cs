using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class TerminalSyncUpdates
    {
        public bool SuccessfullySyncedToDatabase { get; set; }
        public bool SuccessfullySyncedToApi { get; set; }
    }
}
