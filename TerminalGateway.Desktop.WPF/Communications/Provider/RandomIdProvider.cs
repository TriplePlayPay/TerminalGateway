using GlobalPayments.Api.Terminals.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Provider
{
    public class RandomIdProvider : IRequestIdProvider
    {
        private Random random;

        public RandomIdProvider()
        {
            random = new Random(DateTime.Now.Millisecond);
        }

        public int GetRequestId()
        {
            return random.Next(100000, 999999);
        }
    }
}
