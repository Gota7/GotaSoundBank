using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.DLS {
    
    /// <summary>
    /// Region.
    /// </summary>
    public class Region {

        /// <summary>
        /// Low note.
        /// </summary>
        public ushort NoteLow = 0;

        /// <summary>
        /// High note.
        /// </summary>
        public ushort NoteHigh = 127;

        /// <summary>
        /// Low velocity.
        /// </summary>
        public ushort VelocityLow = 0;

        /// <summary>
        /// High velocity.
        /// </summary>
        public ushort VelocityHigh = 127;

        /// <summary>
        /// If the same note is played when already playing, it will not stop the first one if this is enabled.
        /// </summary>
        public bool DoublePlayback = true;

        /// <summary>
        /// Key group. 0 if disabled, 1-15 if used.
        /// </summary>
        public byte KeyGroup;

        /// <summary>
        /// Layer.
        /// </summary>
        public ushort Layer;

        /// <summary>
        /// Root note.
        /// </summary>
        public byte RootNote = 60;

        /// <summary>
        /// Tuning.
        /// </summary>
        public short Tuning;

        /// <summary>
        /// Gain.
        /// </summary>
        public int Gain;

        /// <summary>
        /// No truncation.
        /// </summary>
        public bool NoTruncation = true;

        /// <summary>
        /// No compression.
        /// </summary>
        public bool NoCompression;

        /// <summary>
        /// Loops.
        /// </summary>
        public bool Loops;

        /// <summary>
        /// If the loop is to loop and be released instead of looping forward only.
        /// </summary>
        public bool LoopAndRelease;

        /// <summary>
        /// Loop start.
        /// </summary>
        public uint LoopStart;

        /// <summary>
        /// Loop length.
        /// </summary>
        public uint LoopLength;

        /// <summary>
        /// Master link.
        /// </summary>
        public bool PhaseMaster;

        /// <summary>
        /// Ignore steering in articulation.
        /// </summary>
        public bool MultiChannel;

        /// <summary>
        /// Phase group.
        /// </summary>
        public ushort PhaseGroup;

        /// <summary>
        /// Channel flags.
        /// </summary>
        public uint ChannelFlags;

        /// <summary>
        /// Wave Id.
        /// </summary>
        public uint WaveId;

        /// <summary>
        /// Articulators.
        /// </summary>
        public List<Articulator> Articulators = new List<Articulator>();

    }

}
