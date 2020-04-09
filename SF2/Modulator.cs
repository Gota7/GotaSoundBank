using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {
    
    /// <summary>
    /// Modulator.
    /// </summary>
    public class Modulator : IReadable, IWritable {

        /// <summary>
        /// Source.
        /// </summary>
        public SF2Modulators Source;

        /// <summary>
        /// Destination.
        /// </summary>
        public SF2Generators Destination;

        /// <summary>
        /// Amount.
        /// </summary>
        public short Amount;

        /// <summary>
        /// Amoutn source.
        /// </summary>
        public SF2Modulators AmountSource;

        /// <summary>
        /// Transform type.
        /// </summary>
        public SF2Transforms Transform;

        /// <summary>
        /// Read the modulator.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Source = (SF2Modulators)r.ReadUInt16();
            Destination = (SF2Generators)r.ReadUInt16();
            Amount = r.ReadInt16();
            AmountSource = (SF2Modulators)r.ReadUInt16();
            Transform = (SF2Transforms)r.ReadUInt16();
        }

        /// <summary>
        /// Write the modulator.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write((ushort)Source);
            w.Write((ushort)Destination);
            w.Write(Amount);
            w.Write((ushort)AmountSource);
            w.Write((ushort)Transform);
        }

    }

}
