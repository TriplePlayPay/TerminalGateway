using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Models
{
    public class WebsocketTransmissionModel<T>
    {
        public string Type { get; set; }
        public T Message { get; set; }
    }
}
