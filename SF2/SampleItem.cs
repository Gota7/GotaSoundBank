using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {
    
    /// <summary>
    /// A sample item.
    /// </summary>
    public class SampleItem : IReadable, IWriteable {

        /// <summary>
        /// Sample name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Original pitch.
        /// </summary>
        public byte OriginalPitch = 60;

        /// <summary>
        /// Pitch correction.
        /// </summary>
        public sbyte PitchCorrection;

        /// <summary>
        /// Sample link.
        /// </summary>
        public ushort Link;

        /// <summary>
        /// If rom type.
        /// </summary>
        public bool IsRomType;

        /// <summary>
        /// Link type.
        /// </summary>
        public SF2LinkTypes LinkType;

        /// <summary>
        /// Mono wave file.
        /// </summary>
        public RiffWave Wave;

        /// <summary>
        /// Read the sample item. Set current offset to start of wave table in advance!
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Name = r.ReadFixedString(20);
            uint startSample = r.ReadUInt32();
            uint endSample = r.ReadUInt32();
            long bak = r.Position;
            r.Position = r.CurrentOffset;
            r.Position += startSample * 2;
            Wave = new RiffWave() { Audio = new AudioData() { Channels = new List<List<GotaSoundIO.Sound.Encoding.IAudioEncoding>>() { new List<GotaSoundIO.Sound.Encoding.IAudioEncoding>() { new PCM16() } } } };
            Wave.Audio.Channels[0][0].ReadRaw(r, (uint)((endSample * 2 + r.CurrentOffset - r.Position) / 2), (uint)(endSample * 2 + r.CurrentOffset - r.Position));
            r.Position = bak;
            Wave.LoopStart = r.ReadUInt32();
            Wave.LoopEnd = r.ReadUInt32();
            if (Wave.LoopEnd != 0) {
                Wave.LoopStart -= startSample;
                Wave.LoopEnd -= startSample;
            }
            Wave.Loops = Wave.LoopEnd > 0;
            Wave.SampleRate = r.ReadUInt32();
            OriginalPitch = r.ReadByte();
            PitchCorrection = r.ReadSByte();
            Link = r.ReadUInt16();
            ushort type = r.ReadUInt16();
            LinkType = (SF2LinkTypes)(type & 0b1111);
            IsRomType = (type & 0b1000000000000000) > 0;
        }

        /// <summary>
        /// Write the sample item. Set the current offset to the start of the wave in advance! Also push into the structure offsets the start of the wave table!
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {

            //Wave table start.
            long waveTableStart = w.StructureOffsets.Pop();

            //Start writing data.
            w.WriteFixedString(Name, 20);
            uint startSample = (uint)((w.CurrentOffset - waveTableStart) / 2);
            w.Write(startSample);
            w.Write((uint)(startSample + Wave.Audio.NumSamples));
            long bak = w.Position;
            w.Position = w.CurrentOffset;
            Wave.Audio.Write(w);
            w.Position = bak;
            w.Write((uint)(Wave.Loops ? Wave.LoopStart + startSample : 0));
            w.Write((uint)(Wave.Loops ? Wave.LoopEnd + startSample : 0));
            w.Write(Wave.SampleRate);
            w.Write(OriginalPitch);
            w.Write(PitchCorrection);
            w.Write(Link);
            ushort val = (ushort)LinkType;
            if (IsRomType) { val |= 0b1000000000000000; }
            w.Write(val);

        }

    }

}
