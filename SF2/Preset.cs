using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// Instrument, or preset.
    /// </summary>
    public class Preset : IReadable, IWritable {

        /// <summary>
        /// Preset name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Preset.
        /// </summary>
        public ushort PresetNumber;

        /// <summary>
        /// Bank.
        /// </summary>
        public ushort Bank;

        /// <summary>
        /// Bag index to only be used when reading and writing.
        /// </summary>
        public ushort ReadingBagIndex;

        /// <summary>
        /// Library.
        /// </summary>
        public uint Library;

        /// <summary>
        /// Genre.
        /// </summary>
        public uint Genre;

        /// <summary>
        /// Morphology.
        /// </summary>
        public uint Morphology;

        /// <summary>
        /// Number of zones.
        /// </summary>
        public int NumZones => Zones.Count + (GlobalZone != null ? 1 : 0);

        /// <summary>
        /// Global zone.
        /// </summary>
        public Zone GlobalZone = null;

        /// <summary>
        /// Zones.
        /// </summary>
        public List<Zone> Zones = new List<Zone>();

        /// <summary>
        /// Get a list of all the zones.
        /// </summary>
        /// <returns>The zones.</returns>
        public List<Zone> GetAllZones() {
            List<Zone> ret = new List<Zone>();
            if (GlobalZone != null) { ret.Add(GlobalZone); }
            foreach (var z in Zones) {
                ret.Add(z);
            }
            return ret;
        }

        /// <summary>
        /// Read the preset.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Name = r.ReadFixedString(20);
            PresetNumber = r.ReadUInt16();
            Bank = r.ReadUInt16();
            ReadingBagIndex = r.ReadUInt16();
            Library = r.ReadUInt32();
            Genre = r.ReadUInt32();
            Morphology = r.ReadUInt32();
        }

        /// <summary>
        /// Write the preset.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.WriteFixedString(Name, 20);
            w.Write(PresetNumber);
            w.Write(Bank);
            w.Write(ReadingBagIndex);
            w.Write(Library);
            w.Write(Genre);
            w.Write(Morphology);
        }

    }

}
