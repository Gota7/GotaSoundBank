using GotaSoundBank.DLS;
using GotaSoundIO.IO;
using GotaSoundIO.IO.RIFF;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// A sound font. My implementation does not support PCM24 so quality will be lost if PCM24 is used.
    /// </summary>
    public class SoundFont : IOFile {

        /// <summary>
        /// Presets.
        /// </summary>
        public List<Preset> Presets = new List<Preset>();

        /// <summary>
        /// Instruments.
        /// </summary>
        public List<Instrument> Instruments = new List<Instrument>();

        /// <summary>
        /// Samples.
        /// </summary>
        public List<SampleItem> Samples = new List<SampleItem>();

        /// <summary>
        /// Sound engine.
        /// </summary>
        public string SoundEngine = "EMU8000";

        /// <summary>
        /// Bank name.
        /// </summary>
        public string BankName = "General MIDI";

        /// <summary>
        /// ROM name.
        /// </summary>
        public string RomName = "";

        /// <summary>
        /// ROM version. Major and minor.
        /// </summary>
        public Tuple<ushort, ushort> RomVersion = null;   

        /// <summary>
        /// Creation date.
        /// </summary>
        public string CreationDate = "";

        /// <summary>
        /// Who designed the sound.
        /// </summary>
        public string SoundDesigner = "";

        /// <summary>
        /// Product.
        /// </summary>
        public string Product = "";

        /// <summary>
        /// Copyright.
        /// </summary>
        public string Copyright = "";

        /// <summary>
        /// Comment.
        /// </summary>
        public string Comment = "";

        /// <summary>
        /// Tool used to last modify SF2.
        /// </summary>
        public string Tools = "";

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public SoundFont() {}

        /// <summary>
        /// Read from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public SoundFont(string filePath) : base(filePath) {}

        /// <summary>
        /// Create a sound font from a downloadable sounds file.
        /// </summary>
        /// <param name="dls">A DLS file.</param>
        public SoundFont(DownloadableSounds dls) {

            //Get samples.
            Dictionary<int, int> waveLink;
            dls.AssignLoops();
            CreateSampleTable(dls.Waves, out waveLink);

            //Get instruments.
            foreach (var i in dls.Instruments) {
                Instrument inst = new Instrument();
                inst.Name = i.Name;
                foreach (var r in i.Regions) {

                    //New zone.
                    Zone z = new Zone();

                    //Key range.
                    if (r.NoteHigh != 127 || r.NoteLow != 0) {
                        z.Generators.Add(new Generator() { Gen = SF2Generators.KeyRange, Amount = new SF2GeneratorAmount() { LowByte = (byte)r.NoteLow, HighByte = (byte)r.NoteHigh } });
                    }

                    //Velocity range.
                    if (r.VelocityHigh != 127 || r.VelocityLow != 0) {
                        z.Generators.Add(new Generator() { Gen = SF2Generators.VelRange, Amount = new SF2GeneratorAmount() { LowByte = (byte)r.VelocityLow, HighByte = (byte)r.VelocityHigh } });
                    }

                    //Root key.
                    z.Generators.Add(new Generator() { Gen = SF2Generators.OverridingRootKey, Amount = new SF2GeneratorAmount() { UAmount = r.RootNote } });

                    //Pitch correction.
                    Samples[waveLink[(int)r.WaveId]].PitchCorrection = (sbyte)(r.Tuning / 65536);

                    //Sample Id.
                    z.Generators.Add(new Generator() { Gen = SF2Generators.SampleID, Amount = new SF2GeneratorAmount() { UAmount = (ushort)waveLink[(int)r.WaveId] } });
                    if (dls.Waves[(int)r.WaveId].Loops) { z.Generators.Add(new Generator() { Gen = SF2Generators.SampleModes, Amount = new SF2GeneratorAmount() { Amount = 1 } }); }

                    //Articulators.
                    foreach (var a in r.Articulators) {
                        foreach (var c in a.Connections) {

                            //Generator.
                            SF2Generators gen = (SF2Generators)100;
                            SF2GeneratorAmount amount = new SF2GeneratorAmount();

                            //Switch connection type.
                            switch (c.DestinationConnection) {
                                case DestinationConnection.Chorus:
                                    gen = SF2Generators.ChorusEffectsSend;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1AttackTime:
                                    gen = SF2Generators.AttackVolEnv;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1DecayTime:
                                    gen = SF2Generators.DecayVolEnv;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1DelayTime:
                                    gen = SF2Generators.DelayVolEnv;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1HoldTime:
                                    gen = SF2Generators.HoldVolEnv;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1ReleaseTime:
                                    gen = SF2Generators.ReleaseVolEnv;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.EG1SustainLevel:
                                    gen = SF2Generators.SustainVolEnv;
                                    amount.Amount = (short)((1 - c.Scale / 65536d / 1000) * 1000);
                                    break;
                                case DestinationConnection.KeyNumber:
                                    gen = SF2Generators.Keynum;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.Pan:
                                    gen = SF2Generators.Pan;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.LFOFrequency:
                                    gen = SF2Generators.FreqModLFO;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                                case DestinationConnection.LFOStartDelayTime:
                                    gen = SF2Generators.DelayModLFO;
                                    amount.Amount = (short)(c.Scale / 65536);
                                    break;
                            }

                            //Add generator.
                            if ((int)gen != 100) {
                                z.Generators.Add(new Generator() { Gen = gen, Amount = amount });
                            }

                            //Modulator used.
                            if (c.TransformConnection != TransformConnection.None) { 
                                //Nah, I'm lazy.
                            }
                        
                        }
                    }
                    
                    //Add zone.
                    inst.Zones.Add(z);

                }
                Instruments.Add(inst);

            }

            //Set presets.
            ushort instNum = 0;
            foreach (var i in dls.Instruments) {
                Presets.Add(new Preset() { Bank = (ushort)i.BankId, Name = i.Name, PresetNumber = (ushort)i.InstrumentId, Zones = new List<Zone>() { new Zone() { Generators = new List<Generator>() { new Generator() { Gen = SF2Generators.Instrument, Amount = new SF2GeneratorAmount() { UAmount = instNum++ } } } } } });
            }

        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r2">The reader.</param>
        public override void Read(FileReader r2) {

            //Use a RIFF reader.
            using (RiffReader r = new RiffReader(r2.BaseStream)) {

                //Get INFO data.
                var info = (ListChunk)r.GetChunk("INFO");

                //Sound engine.
                r.OpenChunk(info.GetChunk("isng"));
                SoundEngine = r.ReadNullTerminated();

                //Bank name.
                r.OpenChunk(info.GetChunk("INAM"));
                BankName = r.ReadNullTerminated();

                //ROM name.
                if (info.GetChunk("irom") != null) {
                    r.OpenChunk(info.GetChunk("irom"));
                    RomName = r.ReadNullTerminated();
                }

                //ROM version.
                if (info.GetChunk("iver") != null) {
                    r.OpenChunk(info.GetChunk("iver"));
                    RomVersion = new Tuple<ushort, ushort>(r.ReadUInt16(), r.ReadUInt16());
                }

                //Creation date.
                if (info.GetChunk("ICRD") != null) {
                    r.OpenChunk(info.GetChunk("ICRD"));
                    CreationDate = r.ReadNullTerminated();
                }

                //Sound designer.
                if (info.GetChunk("IENG") != null) {
                    r.OpenChunk(info.GetChunk("IENG"));
                    SoundDesigner = r.ReadNullTerminated();
                }

                //Product.
                if (info.GetChunk("IPRD") != null) {
                    r.OpenChunk(info.GetChunk("IPRD"));
                    Product = r.ReadNullTerminated();
                }

                //Copyright.
                if (info.GetChunk("ICOP") != null) {
                    r.OpenChunk(info.GetChunk("ICOP"));
                    Copyright = r.ReadNullTerminated();
                }

                //Comment.
                if (info.GetChunk("ICMT") != null) {
                    r.OpenChunk(info.GetChunk("ICMT"));
                    Comment = r.ReadNullTerminated();
                }

                //Tools.
                if (info.GetChunk("ISFT") != null) {
                    r.OpenChunk(info.GetChunk("ISFT"));
                    Tools = r.ReadNullTerminated();
                }

                //Get wave table position.
                long waveTablePos = ((ListChunk)r.GetChunk("sdta")).GetChunk("smpl").Pos;

                //The hydra.
                Presets = new List<Preset>();
                Instruments = new List<Instrument>();
                Samples = new List<SampleItem>();
                var hydra = (ListChunk)r.GetChunk("pdta");

                //Get presets.
                r.OpenChunk(hydra.GetChunk("phdr"));
                uint numPresets = hydra.GetChunk("phdr").Size / 38 - 1;
                for (uint i = 0; i < numPresets; i++) {
                    Presets.Add(r.Read<Preset>());
                }

                //Get preset bags.
                List<Tuple<ushort, ushort>> presetGenModIndices = new List<Tuple<ushort, ushort>>();
                List<Zone> presetZones = new List<Zone>();
                r.OpenChunk(hydra.GetChunk("pbag"));
                uint numPbags = hydra.GetChunk("pbag").Size / 4 - 1;
                for (uint i = 0; i < numPbags; i++) {
                    presetGenModIndices.Add(new Tuple<ushort, ushort>(r.ReadUInt16(), r.ReadUInt16()));
                    presetZones.Add(new Zone());
                }

                //Get preset modulators.
                List<Modulator> pMods = new List<Modulator>();
                r.OpenChunk(hydra.GetChunk("pmod"));
                uint numPmods = hydra.GetChunk("pmod").Size / 10 - 1;
                for (uint i = 0; i < numPmods; i++) {
                    pMods.Add(r.Read<Modulator>());
                }

                //Get preset generators.
                List<Generator> pGens = new List<Generator>();
                r.OpenChunk(hydra.GetChunk("pgen"));
                uint numPgens = hydra.GetChunk("pgen").Size / 4 - 1;
                for (uint i = 0; i < numPgens; i++) {
                    pGens.Add(r.Read<Generator>());
                }

                //Get true generators and modulators.
                for (int i = 0; i < presetGenModIndices.Count; i++) {

                    //Index.
                    int startGen = presetGenModIndices[i].Item1;
                    int startMod = presetGenModIndices[i].Item2;
                    int numGen = pGens.Count - startGen;
                    int numMod = pMods.Count - startMod;
                    if (i + 1 <= presetGenModIndices.Count - 1) {
                        numGen = presetGenModIndices[i + 1].Item1 - startGen;
                        numMod = presetGenModIndices[i + 1].Item2 - startMod;
                    }

                    //Get stuff.
                    for (int j = startGen; j < startGen + numGen; j++) {
                        presetZones[i].Generators.Add(pGens[j]);
                    }
                    for (int j = startMod; j < startMod + numMod; j++) {
                        presetZones[i].Modulators.Add(pMods[j]);
                    }

                }

                //Add the zones to the presets.
                for (int i = 0; i < Presets.Count; i++) {

                    //Index.
                    int startZone = Presets[i].ReadingBagIndex;
                    int numZones = presetGenModIndices.Count - startZone;
                    if (i + 1 <= Presets.Count - 1) {
                        numZones = Presets[i + 1].ReadingBagIndex - startZone;
                    }

                    //Get stuff.
                    for (int j = startZone; j < startZone + numZones; j++) {
                        if (Presets[i].Zones.Count == 0 && presetZones[j].Generators.Where(x => x.Gen == SF2Generators.Instrument).Where(x => x.Gen == SF2Generators.Instrument).Count() < 1) {
                            Presets[i].GlobalZone = presetZones[j];
                        } else {
                            Presets[i].Zones.Add(presetZones[j]);
                        }
                    }

                }

                //Get instruments.
                r.OpenChunk(hydra.GetChunk("inst"));
                uint numInstruments = hydra.GetChunk("inst").Size / 22 - 1;
                for (uint i = 0; i < numInstruments; i++) {
                    Instruments.Add(r.Read<Instrument>());
                }

                //Get instrument bags.
                List<Tuple<ushort, ushort>> instrumentGenModIndices = new List<Tuple<ushort, ushort>>();
                List<Zone> instrumentZones = new List<Zone>();
                r.OpenChunk(hydra.GetChunk("ibag"));
                uint numIbags = hydra.GetChunk("ibag").Size / 4 - 1;
                for (uint i = 0; i < numIbags; i++) {
                    instrumentGenModIndices.Add(new Tuple<ushort, ushort>(r.ReadUInt16(), r.ReadUInt16()));
                    instrumentZones.Add(new Zone());
                }

                //Get instrument modulators.
                List<Modulator> iMods = new List<Modulator>();
                r.OpenChunk(hydra.GetChunk("imod"));
                uint numImods = hydra.GetChunk("imod").Size / 10 - 1;
                for (uint i = 0; i < numImods; i++) {
                    iMods.Add(r.Read<Modulator>());
                }

                //Get instrument generators.
                List<Generator> iGens = new List<Generator>();
                r.OpenChunk(hydra.GetChunk("igen"));
                uint numIgens = hydra.GetChunk("igen").Size / 4 - 1;
                for (uint i = 0; i < numIgens; i++) {
                    iGens.Add(r.Read<Generator>());
                }

                //Get true generators and modulators.
                for (int i = 0; i < instrumentGenModIndices.Count; i++) {

                    //Index.
                    int startGen = instrumentGenModIndices[i].Item1;
                    int startMod = instrumentGenModIndices[i].Item2;
                    int numGen = iGens.Count - startGen;
                    int numMod = iMods.Count - startMod;
                    if (i + 1 <= instrumentGenModIndices.Count - 1) {
                        numGen = instrumentGenModIndices[i + 1].Item1 - startGen;
                        numMod = instrumentGenModIndices[i + 1].Item2 - startMod;
                    }

                    //Get stuff.
                    for (int j = startGen; j < startGen + numGen; j++) {
                        instrumentZones[i].Generators.Add(iGens[j]);
                    }
                    for (int j = startMod; j < startMod + numMod; j++) {
                        instrumentZones[i].Modulators.Add(iMods[j]);
                    }

                }

                //Add the zones to the instruments.
                for (int i = 0; i < Instruments.Count; i++) {

                    //Index.
                    int startZone = Instruments[i].ReadingBagIndex;
                    int numZones = instrumentGenModIndices.Count - startZone;
                    if (i + 1 <= Instruments.Count - 1) {
                        numZones = Instruments[i + 1].ReadingBagIndex - startZone;
                    }

                    //Get stuff.
                    for (int j = startZone; j < startZone + numZones; j++) {
                        if (Instruments[i].Zones.Count == 0 && instrumentZones[j].Generators.Where(x => x.Gen == SF2Generators.SampleID).Count() < 1) {
                            Instruments[i].GlobalZone = instrumentZones[j];
                        }
                        else {
                            Instruments[i].Zones.Add(instrumentZones[j]);
                        }
                    }

                }

                //Get samples.
                r.OpenChunk(hydra.GetChunk("shdr"));
                uint numSamples = hydra.GetChunk("shdr").Size / 46 - 1;
                r.CurrentOffset = waveTablePos;
                for (uint i = 0; i < numSamples; i++) {
                    Samples.Add(r.Read<SampleItem>());
                }

            }

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w2">The writer.</param>
        public override void Write(FileWriter w2) {

            //Use a RIFF writer.
            using (RiffWriter w = new RiffWriter(w2.BaseStream)) {

                //Start file.
                w.InitFile("sfbk");

                //Start INFO.
                w.StartListChunk("INFO");

                //Version.
                w.StartChunk("ifil");
                w.Write((ushort)2);
                w.Write((ushort)1);
                w.EndChunk();

                //Sound engine.
                w.StartChunk("isng");
                w.WriteNullTerminated(SoundEngine);
                w.Align(2);
                w.EndChunk();

                //Bank name.
                w.StartChunk("INAM");
                w.WriteNullTerminated(BankName);
                w.Align(2);
                w.EndChunk();

                //ROM name.
                if (!RomName.Equals("")) {
                    w.StartChunk("irom");
                    w.WriteNullTerminated(RomName);
                    w.Align(2);
                    w.EndChunk();
                }

                //ROM version.
                if (RomVersion != null) {
                    w.StartChunk("iver");
                    w.Write(RomVersion.Item1);
                    w.Write(RomVersion.Item2);
                    w.EndChunk();
                }

                //Creation date.
                if (!CreationDate.Equals("")) {
                    w.StartChunk("ICRD");
                    w.WriteNullTerminated(CreationDate);
                    w.Align(2);
                    w.EndChunk();
                }

                //Sound designer.
                if (!SoundDesigner.Equals("")) {
                    w.StartChunk("IENG");
                    w.WriteNullTerminated(SoundDesigner);
                    w.Align(2);
                    w.EndChunk();
                }

                //Product.
                if (!Product.Equals("")) {
                    w.StartChunk("IPRD");
                    w.WriteNullTerminated(Product);
                    w.Align(2);
                    w.EndChunk();
                }

                //Copyright.
                if (!Copyright.Equals("")) {
                    w.StartChunk("ICOP");
                    w.WriteNullTerminated(Copyright);
                    w.Align(2);
                    w.EndChunk();
                }

                //Comment.
                if (!Comment.Equals("")) {
                    w.StartChunk("ICMT");
                    w.WriteNullTerminated(Comment);
                    w.Align(2);
                    w.EndChunk();
                }

                //Tools.
                if (!Tools.Equals("")) {
                    w.StartChunk("ISFT");
                    w.WriteNullTerminated(Tools);
                    w.Align(2);
                    w.EndChunk();
                }

                //End INFO.
                w.EndChunk();

                //Sample block.
                w.StartListChunk("sdta");
                w.StartChunk("smpl");
                long waveTableStart = w.Position;
                Dictionary<SampleItem, long> samplePositions = new Dictionary<SampleItem, long>();
                foreach (var s in Samples) {
                    samplePositions.Add(s, w.Position);
                    w.Write(new short[s.Wave.Audio.NumSamples]);
                    w.Write(new short[46]);
                }
                w.EndChunk();
                w.EndChunk();

                //The hydra.
                w.StartListChunk("pdta");

                //Presets.
                w.StartChunk("phdr");
                ushort currBagIndex = 0;
                List<Zone> zones = new List<Zone>();
                foreach (var p in Presets) {
                    p.ReadingBagIndex = currBagIndex;
                    currBagIndex += (ushort)p.NumZones;
                    w.Write(p);
                    zones.AddRange(p.GetAllZones());
                }
                w.Write(new Preset() { Name = "EOP", Bank = 255, PresetNumber = 255, ReadingBagIndex = currBagIndex });
                w.EndChunk();

                //Preset bags.     
                w.StartChunk("pbag");
                ushort currGenIndex = 0;
                ushort currModIndex = 0;
                foreach (var z in zones) {
                    w.Write(currGenIndex);
                    w.Write(currModIndex);
                    currGenIndex += (ushort)z.Generators.Count;
                    currModIndex += (ushort)z.Modulators.Count;
                }
                w.Write(currGenIndex);
                w.Write(currModIndex);
                w.EndChunk();

                //Preset modulators.
                w.StartChunk("pmod");
                foreach (var z in zones) {
                    foreach (var v in z.Modulators) {
                        w.Write(v);
                    }
                }
                w.Write(new Modulator());
                w.EndChunk();

                //Preset generators.
                w.StartChunk("pgen");
                foreach (var z in zones) {
                    foreach (var v in z.Generators) {
                        w.Write(v);
                    }
                }
                w.Write(new Generator());
                w.EndChunk();

                //Instruments.
                w.StartChunk("inst");
                currBagIndex = 0;
                zones = new List<Zone>();
                foreach (var p in Instruments) {
                    p.ReadingBagIndex = currBagIndex;
                    currBagIndex += (ushort)p.NumZones;
                    w.Write(p);
                    zones.AddRange(p.GetAllZones());
                }
                w.Write(new Instrument() { Name = "EOI", ReadingBagIndex = currBagIndex });
                w.EndChunk();

                //Instrument bags.     
                w.StartChunk("ibag");
                currGenIndex = 0;
                currModIndex = 0;
                foreach (var z in zones) {
                    w.Write(currGenIndex);
                    w.Write(currModIndex);
                    currGenIndex += (ushort)z.Generators.Count;
                    currModIndex += (ushort)z.Modulators.Count;
                }
                w.Write(currGenIndex);
                w.Write(currModIndex);
                w.EndChunk();

                //Instrument modulators.
                w.StartChunk("imod");
                foreach (var z in zones) {
                    foreach (var v in z.Modulators) {
                        w.Write(v);
                    }
                }
                w.Write(new Modulator());
                w.EndChunk();

                //Instrument generators.
                w.StartChunk("igen");
                foreach (var z in zones) {
                    foreach (var v in z.Generators) {
                        w.Write(v);
                    }
                }
                w.Write(new Generator());
                w.EndChunk();

                //Samples.
                w.StartChunk("shdr");
                foreach (var s in Samples) {
                    w.CurrentOffset = samplePositions[s];
                    w.StructureOffsets.Push(waveTableStart);
                    w.Write(s);
                }
                w.Write("EOS".ToCharArray());
                w.Write(new byte[0x2B]);
                w.EndChunk();

                //End the hydra.
                w.EndChunk();

                //Close file.
                w.CloseFile();

            }

        }

        /// <summary>
        /// Create a sample table from some waves and get a wave's new index.
        /// </summary>
        /// <param name="waves"></param>
        /// <param name="newIndices"></param>
        public void CreateSampleTable(List<RiffWave> waves, out Dictionary<int, int> newIndices) {

            //New indices.
            newIndices = new Dictionary<int, int>();
            int currInd = 0;
            ushort link = 1;

            //Fix loops.
            foreach (var w in waves) {
                if (w.LoopEnd != 0) {
                    w.Loops = true;
                }
            }

            //For each wave.
            Samples = new List<SampleItem>();
            for (int i = 0; i < waves.Count; i++) {

                //Switch the number of channels.
                switch (waves[i].Audio.Channels.Count()) {

                    //Mono.
                    case 1:
                        Samples.Add(new SampleItem() { LinkType = SF2LinkTypes.Mono, Name = "Sample " + i, Wave = waves[i] });
                        break;

                    //Stereo.
                    case 2:
                        RiffWave left = new RiffWave();
                        RiffWave right = new RiffWave();
                        left.FromOtherStreamFile(waves[i]);
                        right.FromOtherStreamFile(waves[i]);
                        left.Audio.Channels.RemoveAt(1);
                        right.Audio.Channels.RemoveAt(0);
                        Samples.Add(new SampleItem() { LinkType = SF2LinkTypes.Left, Name = "Sample " + i + " L", Link = link, Wave = left });
                        Samples.Add(new SampleItem() { LinkType = SF2LinkTypes.Right, Name = "Sample " + i + " R", Link = link++, Wave = right });
                        break;

                    //Link.
                    default:
                        int chanNum = 0;
                        foreach (var w in waves) {
                            RiffWave lnk = new RiffWave();
                            lnk.FromOtherStreamFile(w);
                            lnk.Audio.Channels = new List<List<GotaSoundIO.Sound.Encoding.IAudioEncoding>>() { lnk.Audio.Channels[chanNum++] };
                            Samples.Add(new SampleItem() { LinkType = SF2LinkTypes.Left, Name = "Sample " + i + " Link " + chanNum, Link = link, Wave = lnk });
                        }
                        link++;
                        break;

                }

                //Increase index.
                newIndices.Add(i, currInd);
                currInd += (ushort)waves[i].Audio.Channels.Count();

            }

        }

    }

}
