using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGateway.Desktop.WPF.Communications.Validators
{
    public class ApiKeyValidator
    {
        public bool IsValidUuidV4(string uuidString)
        {
            if (Guid.TryParse(uuidString, out Guid guid))
            {
                byte[] bytes = guid.ToByteArray();
                // Version number is in the 7th byte
                int version = (bytes[7] >> 4);
                // Variant is stored in the 8th byte; should be 0b10xx xxxx for UUID version 4
                int variant = (bytes[8] >> 6) & 0x03;

                return version == 4 && (variant == 2);
            }
            return false;
        }
    }
}
