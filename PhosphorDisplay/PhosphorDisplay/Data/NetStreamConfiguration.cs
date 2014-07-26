using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhosphorDisplay.Data
{
    public class NetStreamConfiguration
    {
        public int OversampleRatio { get; set; }
        public bool UseFastAdc { get; set; }

        public int AfeGain { get; set; }
        public int AdcSpeed { get; set; }
    }
}
