using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// SF2 modulator.
    /// </summary>
    public enum SF2Modulators : ushort {
        None = 0,
        NoteOnVelocity = 1,
        NoteOnKey = 2,
        PolyPressure = 0xA,
        ChnPressure = 0xD,
        PitchWheel = 0xE,
        PitchWheelSensivity = 0x10,
        Link = 127
    }

}
