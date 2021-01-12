using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// A generator.
    /// </summary>
    public class Generator : IReadable, IWriteable {

        /// <summary>
        /// Generator.
        /// </summary>
        public SF2Generators Gen;

        /// <summary>
        /// Amount.
        /// </summary>
        public SF2GeneratorAmount Amount;

        /// <summary>
        /// Read the generator.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Gen = (SF2Generators)r.ReadUInt16();
            Amount = new SF2GeneratorAmount();
            Amount.Amount = r.ReadInt16();
        }

        /// <summary>
        /// Write the generator.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write((ushort)Gen);
            w.Write(Amount.Amount);
        }

    }

}
