using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// Link types.
    /// </summary>
    public enum SF2LinkTypes : ushort {
        Mono = 1,
        Right = 2,
        Left = 4,
        Linked = 8
    }

}
