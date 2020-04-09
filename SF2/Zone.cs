using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {
    
    /// <summary>
    /// An instrument or preset zone.
    /// </summary>
    public class Zone {

        /// <summary>
        /// Generators.
        /// </summary>
        public List<Generator> Generators = new List<Generator>();

        /// <summary>
        /// Modulators.
        /// </summary>
        public List<Modulator> Modulators = new List<Modulator>();

    }

}
