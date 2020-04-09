using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.DLS {
    
    /// <summary>
    /// Instrument.
    /// </summary>
    public class Instrument {

        /// <summary>
        /// Instrument name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Bank Id.
        /// </summary>
        public uint BankId;

        /// <summary>
        /// Instrument Id.
        /// </summary>
        public uint InstrumentId;

        /// <summary>
        /// Regions.
        /// </summary>
        public List<Region> Regions = new List<Region>();

    }

}
