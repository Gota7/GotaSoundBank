using GotaSoundIO.Sound;
using GotaSoundIO.IO.RIFF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;
using GotaSoundBank.SF2;

namespace GotaSoundBank.DLS {
    
    /// <summary>
    /// A simplified DLS file. For the sake of simplicity, practically everything but the required chunks are read and written.
    /// </summary>
    public class DownloadableSounds : IOFile {

        /// <summary>
        /// Instruments.
        /// </summary>
        public List<Instrument> Instruments = new List<Instrument>();

        /// <summary>
        /// Waves.
        /// </summary>
        public List<RiffWave> Waves = new List<RiffWave>();

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public DownloadableSounds() {}

        /// <summary>
        /// Read a DLS file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public DownloadableSounds(string filePath) : base(filePath) {}

        /// <summary>
        /// Create a DLS from a sound font.
        /// </summary>
        /// <param name="sf2">The SF2 file.</param>
        public DownloadableSounds(SoundFont sf2) {

            //Get waves.
            List<string> waveMd5s = new List<string>();
            Dictionary<int, int> newWaveIds = new Dictionary<int, int>();

        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r2">The file reader.</param>
        public override void Read(FileReader r2) {

            //Use a RIFF reader.
            using (RiffReader r = new RiffReader(r2.BaseStream)) {

                //Get the number of instruments.
                r.OpenChunk(r.GetChunk("colh"));
                uint numInsts = r.ReadUInt32();

                //Pointer table is skipped since it's just offsets to wave data relative to the first wave identifier.

                //Read wave data.
                Waves = new List<RiffWave>();
                foreach (var c in (r.GetChunk("wvpl") as ListChunk).Chunks) {

                    //Open block.
                    r.OpenChunk(c);

                    //Set position for proper RIFF reading.
                    r.BaseStream.Position -= 8;
                    int len = r.ReadInt32() + 8;
                    r.BaseStream.Position -= 8;
                    RiffWave wav = new RiffWave();
                    wav.Read(r.ReadBytes(len));
                    Waves.Add(wav);

                }

                //Read each instrument.
                foreach (ListChunk c in (r.GetChunk("lins") as ListChunk).Chunks) {

                    //Open block.
                    r.OpenChunk(c);

                    //New instrument.
                    Instrument inst = new Instrument();

                    //Read header.
                    r.OpenChunk(c.GetChunk("insh"));
                    r.ReadUInt32();
                    inst.BankId = r.ReadUInt32();
                    inst.InstrumentId = r.ReadUInt32();

                    //Read regions.
                    foreach (ListChunk g in (c.GetChunk("lrgn") as ListChunk).Chunks) {

                        //New region.
                        Region reg = new Region();

                        //Region header.
                        r.OpenChunk(g.GetChunk("rgnh"));
                        reg.NoteLow = r.ReadUInt16();
                        reg.NoteHigh = r.ReadUInt16();
                        reg.VelocityLow = r.ReadUInt16();
                        reg.VelocityHigh = r.ReadUInt16();
                        reg.DoublePlayback = r.ReadUInt16() > 0;
                        reg.KeyGroup = (byte)r.ReadUInt16();
                        reg.Layer = r.ReadUInt16();

                        //Note information.
                        r.OpenChunk(g.GetChunk("wsmp"));
                        r.ReadUInt32();
                        reg.RootNote = (byte)r.ReadUInt16();
                        reg.Tuning = r.ReadInt16();
                        reg.Gain = r.ReadInt32();
                        uint flags = r.ReadUInt32();
                        reg.NoTruncation = (flags & 0b1) > 0;
                        reg.NoCompression = (flags & 0b10) > 0;
                        reg.Loops = r.ReadUInt32() > 0;
                        if (reg.Loops) {
                            r.ReadUInt32();
                            reg.LoopAndRelease = r.ReadUInt32() > 0;
                            reg.LoopStart = r.ReadUInt32();
                            reg.LoopLength = r.ReadUInt32();
                        }

                        //Wave link.
                        r.OpenChunk(g.GetChunk("wlnk"));
                        uint flg = r.ReadUInt16();
                        reg.PhaseMaster = (flg & 0b1) > 0;
                        reg.MultiChannel = (flg & 0b10) > 0;
                        reg.PhaseGroup = r.ReadUInt16();
                        reg.ChannelFlags = r.ReadUInt32();
                        reg.WaveId = r.ReadUInt32();

                        //Loop.
                        Waves[(int)reg.WaveId].Loops = reg.Loops;
                        if (reg.Loops) {
                            Waves[(int)reg.WaveId].LoopStart = reg.LoopStart;
                            Waves[(int)reg.WaveId].LoopEnd = reg.LoopLength == 0 ? (uint)Waves[(int)reg.WaveId].Channels[0].ToPCM16().Length : reg.LoopStart + reg.LoopLength;
                        }

                        //Articulators.
                        var lar = g.GetChunk("lar2");
                        if (lar == null) {
                            lar = g.GetChunk("lar1");
                        }
                        foreach (Chunk art in (g.GetChunk("lar2") as ListChunk).Chunks) {

                            //Read articulator.
                            Articulator a = new Articulator();
                            r.OpenChunk(art);
                            r.ReadUInt32();
                            uint numCons = r.ReadUInt32();
                            for (uint i = 0; i < numCons; i++) {
                                Connection con = new Connection();
                                con.SourceConnection = (SourceConnection)r.ReadUInt16();
                                con.ControlConnection = r.ReadUInt16();
                                con.DestinationConnection = (DestinationConnection)r.ReadUInt16();
                                con.TransformConnection = (TransformConnection)r.ReadUInt16();
                                con.Scale = r.ReadInt32();
                                a.Connections.Add(con);
                            }
                            reg.Articulators.Add(a);

                        }

                        //Add region.
                        inst.Regions.Add(reg);

                    }

                    //Read name.
                    var info = c.GetChunk("INFO");
                    if (info != null) {
                        var inam = (info as ListChunk).GetChunk("INAM");
                        if (inam != null) {
                            r.OpenChunk(inam);
                            r.BaseStream.Position -= 4;
                            uint siz = r.ReadUInt32();
                            inst.Name = new string(r.ReadChars((int)siz).Where(x => x != 0).ToArray());
                        }
                    }

                    //Add instrument.
                    Instruments.Add(inst);

                }

            }

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w2">The file writer.</param>
        public override void Write(FileWriter w2) {

            //Use a RIFF writer.
            using (RiffWriter w = new RiffWriter(w2.BaseStream)) {

                //Init file.
                w.InitFile("DLS ");

                //Instrument count.
                w.StartChunk("colh");
                w.Write((uint)Instruments.Count);
                w.EndChunk();

                //Instruments.
                w.StartListChunk("lins");
                foreach (var inst in Instruments) {
                    w.StartListChunk("ins ");
                    w.StartChunk("insh");
                    w.Write((uint)inst.Regions.Count);
                    w.Write(inst.BankId);
                    w.Write(inst.InstrumentId);
                    w.EndChunk();
                    w.StartListChunk("lrgn");
                    foreach (Region r in inst.Regions) {
                        w.StartListChunk("rgn2");
                        w.StartChunk("rgnh");
                        w.Write(r.NoteLow);
                        w.Write(r.NoteHigh);
                        w.Write(r.VelocityLow);
                        w.Write(r.VelocityHigh);
                        w.Write((ushort)(r.DoublePlayback ? 1 : 0));
                        w.Write((ushort)r.KeyGroup);
                        w.Write(r.Layer);
                        w.EndChunk();
                        w.StartChunk("wsmp");
                        w.Write((uint)0x14);
                        w.Write((ushort)r.RootNote);
                        w.Write(r.Tuning);
                        w.Write(r.Gain);
                        uint flags = 0;
                        if (r.NoTruncation) { flags |= 0b1; }
                        if (r.NoCompression) { flags |= 0b10; }
                        w.Write(flags);
                        w.Write((uint)(r.Loops ? 1 : 0));
                        if (r.Loops) {
                            w.Write((uint)0x10);
                            w.Write((uint)(r.LoopAndRelease ? 1 : 0));
                            w.Write(r.LoopStart);
                            w.Write(r.LoopLength);
                        }
                        w.EndChunk();
                        w.StartChunk("wlnk");
                        ushort flg = 0;
                        if (r.PhaseMaster) { flg |= 0b1; }
                        if (r.MultiChannel) { flg |= 0b10; }
                        w.Write(flg);
                        w.Write(r.PhaseGroup);
                        w.Write(r.ChannelFlags);
                        w.Write(r.WaveId);
                        w.EndChunk();
                        w.StartListChunk("lar2");
                        foreach (var a in r.Articulators) {
                            w.StartChunk("art2");
                            w.Write((uint)8);
                            w.Write((uint)a.Connections.Count);
                            foreach (var c in a.Connections) {
                                w.Write((ushort)c.SourceConnection);
                                w.Write(c.ControlConnection);
                                w.Write((ushort)c.DestinationConnection);
                                w.Write((ushort)c.TransformConnection);
                                w.Write(c.Scale);
                            }
                            w.EndChunk();
                        }
                        w.EndChunk();
                        w.EndChunk();
                    }
                    w.EndChunk();
                    if (inst.Name != "") {
                        w.StartListChunk("INFO");
                        w.StartChunk("INAM");
                        w.Write(inst.Name.ToCharArray());
                        w.Write("\0".ToCharArray());
                        int len = inst.Name.Length + 1;
                        while (len % 4 != 0) {
                            w.Write((byte)0);
                            len++;
                        }
                        w.EndChunk();
                        w.EndChunk();
                    }
                    w.EndChunk();
                }
                w.EndChunk();

                //Pointer table initializing.
                w.StartChunk("ptbl");
                w.Write((uint)8);
                w.Write((uint)Waves.Count);
                long ptblStart = w.BaseStream.Position;
                w.Write(new byte[Waves.Count * 4]);
                w.EndChunk();

                //Write waves.
                w.StartListChunk("wvpl");
                long waveTableStart = w.BaseStream.Position;
                int waveNum = 0;
                foreach (var wav in Waves) {
                    long wBak = w.BaseStream.Position;
                    w.BaseStream.Position = ptblStart + waveNum++ * 4;
                    w.Write((uint)(wBak - waveTableStart));
                    w.BaseStream.Position = wBak;
                    w.WriteWave(wav);
                }
                w.EndChunk();

                //Write info.
                w.StartListChunk("INFO");
                w.StartChunk("INAM");
                w.Write("Instrument Set".ToCharArray());
                w.EndChunk();
                w.EndChunk();

                //Close file.
                w.CloseFile();

            }

        }

        /// <summary>
        /// Assign wave loops.
        /// </summary>
        public void AssignLoops() {
            foreach (var i in Instruments) {
                foreach (var reg in i.Regions) {

                    //Loop.
                    if (reg.Loops) {
                        Waves[(int)reg.WaveId].Loops = reg.Loops;
                        Waves[(int)reg.WaveId].LoopStart = reg.LoopStart;
                        Waves[(int)reg.WaveId].LoopEnd = reg.LoopLength == 0 ? (uint)Waves[(int)reg.WaveId].Channels[0].ToPCM16().Length : reg.LoopStart + reg.LoopLength;
                    }

                }
            }
        }

    }

}
