using GotaSoundIO.Sound;
using GotaSoundIO.IO.RIFF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {
    
    /// <summary>
    /// Sound font.
    /// </summary>
    public class SoundFontOLD {

        /// <summary>
        /// Instruments.
        /// </summary>
        public List<DLS.Instrument> Instruments;

        /// <summary>
        /// Waves.
        /// </summary>
        public List<RiffWave> Waves = new List<RiffWave>();

        /// <summary>
        /// True wave Ids.
        /// </summary>
        public Dictionary<int, int> TrueWaveIds = new Dictionary<int, int>();

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public SoundFontOLD() {}

        /// <summary>
        /// Load a sound font.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public SoundFontOLD(string filePath) {

            //Riff reader.
            using (FileStream src = new FileStream(filePath, FileMode.Open)) {
                using (RiffReader r = new RiffReader(src)) {

                    //Get samples.
                    r.OpenChunk(((ListChunk)r.Chunks.Where(x => x.Magic.Equals("sdta")).FirstOrDefault()).Chunks.Where(x => x.Magic.Equals("smpl")).FirstOrDefault());
                    r.BaseStream.Position -= 4;
                    short[] pcm16 = new short[(int)(r.ReadUInt32() / 2)];
                    for (int i = 0; i < pcm16.Length; i++) {
                        pcm16[i] = r.ReadInt16();
                    }

                    //Open chunk.
                    ListChunk hydra = r.Chunks.Where(x => x.Magic.Equals("pdta")).FirstOrDefault() as ListChunk;

                    //Get waves.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("shdr")).FirstOrDefault());
                    r.BaseStream.Position -= 4;
                    int numWaves = (int)(r.ReadUInt32() / 46 - 1);
                    List<byte> wavPitches = new List<byte>();
                    List<sbyte> pitchCorrections = new List<sbyte>();
                    List<RiffWave> rawWaves = new List<RiffWave>();
                    List<ushort> sampleLinks = new List<ushort>();
                    List<ushort> sampleTypes = new List<ushort>();
                    for (int i = 0; i < numWaves; i++) {

                        //Get data.
                        RiffWave wav = new RiffWave();
                        r.ReadBytes(20);
                        uint start = r.ReadUInt32();
                        uint end = r.ReadUInt32();
                        short[] data = new short[end - start];
                        Array.Copy(pcm16, start, data, 0, end - start);
                        wav.Channels = new List<AudioEncoding>();
                        PCM16 p = new PCM16();
                        p.FromPCM16(data);
                        wav.Channels.Add(p);
                        wav.LoopStart = r.ReadUInt32();
                        wav.LoopEnd = r.ReadUInt32();
                        wav.SampleRate = r.ReadUInt32();
                        wav.Loops = wav.LoopEnd != 0;
                        wavPitches.Add(r.ReadByte());
                        pitchCorrections.Add(r.ReadSByte());
                        sampleLinks.Add(r.ReadUInt16());
                        sampleTypes.Add(r.ReadUInt16());
                        rawWaves.Add(wav);

                    }

                    //Get true waves
                    Waves = new List<RiffWave>();
                    TrueWaveIds = new Dictionary<int, int>();
                    for (int i = 0; i < rawWaves.Count; i++) {

                        //Type.
                        byte type = (byte)(sampleTypes[i] & 0xFF);

                        //Linked and not added.
                        if (type != 1 && !TrueWaveIds.ContainsKey(i)) {

                            //New wave.
                            RiffWave wav = new RiffWave();
                            wav.FromOtherStreamFile(rawWaves[i]);
                            for (int j = i + 1; j < rawWaves.Count; j++) {

                                //Same link Id.
                                if (sampleLinks[i] == sampleLinks[j]) {
                                    TrueWaveIds.Add(j, Waves.Count);
                                    if (type == 4) {
                                        wav.Channels.Insert(0, rawWaves[j].Channels[0]);
                                    } else {
                                        wav.Channels.Add(rawWaves[j].Channels[0]);
                                    }
                                }

                            }

                            //Add link id.
                            Waves.Add(wav);
                            TrueWaveIds.Add(i, Waves.Count - 1);

                        }

                        //Mono sample.
                        else if (!TrueWaveIds.ContainsKey(i)) {
                            Waves.Add(rawWaves[i]);
                            TrueWaveIds.Add(i, Waves.Count - 1);
                        }

                    }

                    //Get bag.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("ibag")).FirstOrDefault());
                    List<Tuple<ushort, ushort>> genModIndices = new List<Tuple<ushort, ushort>>();
                    r.BaseStream.Position -= 4;
                    int numIbags = (int)(r.ReadUInt32() / 4);
                    for (int i = 0; i < numIbags; i++) {
                        genModIndices.Add(new Tuple<ushort, ushort>(r.ReadUInt16(), r.ReadUInt16()));
                    }

                    //Skip mods bc I don't care about it.

                    //Get gens.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("igen")).FirstOrDefault());
                    List<Tuple<SF2Generators, SF2GeneratorAmount>> gens = new List<Tuple<SF2Generators, SF2GeneratorAmount>>();
                    r.BaseStream.Position -= 4;
                    int numGens = (int)(r.ReadUInt32() / 4) - 1;
                    for (int i = 0; i < numGens; i++) {
                        gens.Add(new Tuple<SF2Generators, SF2GeneratorAmount>((SF2Generators)r.ReadUInt16(), new SF2GeneratorAmount() { Amount = r.ReadInt16() }));
                    }

                    //Instruments.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("inst")).FirstOrDefault());
                    r.BaseStream.Position -= 4;
                    int numInsts = (int)(r.ReadUInt32() / 22) - 1;
                    Dictionary<int, ushort> instBagLinks = new Dictionary<int, ushort>();
                    for (int i = 0; i < numInsts; i++) {
                        r.ReadChars(20);
                        instBagLinks.Add(i, r.ReadUInt16());
                    }

                    //Get bag.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("pbag")).FirstOrDefault());
                    List<Tuple<ushort, ushort>> pGenModIndices = new List<Tuple<ushort, ushort>>();
                    r.BaseStream.Position -= 4;
                    int numPbags = (int)(r.ReadUInt32() / 4);
                    for (int i = 0; i < numPbags; i++) {
                        pGenModIndices.Add(new Tuple<ushort, ushort>(r.ReadUInt16(), r.ReadUInt16()));
                    }

                    //Get gens.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("pgen")).FirstOrDefault());
                    List<Tuple<SF2Generators, SF2GeneratorAmount>> pGens = new List<Tuple<SF2Generators, SF2GeneratorAmount>>();
                    r.BaseStream.Position -= 4;
                    int numPgens = (int)(r.ReadUInt32() / 4) - 1;
                    for (int i = 0; i < numPgens; i++) {
                        pGens.Add(new Tuple<SF2Generators, SF2GeneratorAmount>((SF2Generators)r.ReadUInt16(), new SF2GeneratorAmount() { Amount = r.ReadInt16() }));
                    }

                    //Skip mods bc I don't care about it.

                    //Presets.
                    r.OpenChunk(hydra.Chunks.Where(x => x.Magic.Equals("phdr")).FirstOrDefault());
                    r.BaseStream.Position -= 4;
                    int numPresets = (int)(r.ReadUInt32() / 0x26) - 1;
                    Instruments = new List<DLS.Instrument>();
                    for (int i = 0; i < numPresets; i++) {

                        //New instrument.
                        DLS.Instrument inst = new DLS.Instrument();
                        inst.Name = new string(r.ReadChars(20).Where(x => x != 0).ToArray());
                        inst.InstrumentId = r.ReadUInt16();
                        inst.BankId = r.ReadUInt16();
                        ushort bagIndex = r.ReadUInt16();
                        r.ReadUInt32();
                        r.ReadUInt32();
                        r.ReadUInt32();

                        //P-Gens.
                        //foreach (var gen in )

                        //Add instrument.
                        Instruments.Add(inst);

                    }

                }

            }

        }

    }

}
