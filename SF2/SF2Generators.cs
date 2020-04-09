using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// Generators.
    /// </summary>
    public enum SF2Generators : ushort {
        StartAddrsOffset = 0,
        EndAddrsOffset = 1,
        StartloopAddrsOffset = 2,
        EndloopAddrsOffset = 3,
        StartAddrsCoarseOffset = 4,
        ModLfoToPitch = 5,
        VibLfoToPitch = 6,
        ModEnvToPitch = 7,
        InitialFilterFc = 8,
        InitialFilterQ = 9,
        ModLfoToFilterFc = 10, // 0x000A
        ModEnvToFilterFc = 11, // 0x000B
        EndAddrsCoarseOffset = 12, // 0x000C
        ModLfoToVolume = 13, // 0x000D
        ChorusEffectsSend = 15, // 0x000F
        ReverbEffectsSend = 16, // 0x0010
        Pan = 17, // 0x0011
        DelayModLFO = 21, // 0x0015
        FreqModLFO = 22, // 0x0016
        DelayVibLFO = 23, // 0x0017
        FreqVibLFO = 24, // 0x0018
        DelayModEnv = 25, // 0x0019
        AttackModEnv = 26, // 0x001A
        HoldModEnv = 27, // 0x001B
        DecayModEnv = 28, // 0x001C
        SustainModEnv = 29, // 0x001D
        ReleaseModEnv = 30, // 0x001E
        KeynumToModEnvHold = 31, // 0x001F
        KeynumToModEnvDecay = 32, // 0x0020
        DelayVolEnv = 33, // 0x0021
        AttackVolEnv = 34, // 0x0022
        HoldVolEnv = 35, // 0x0023
        DecayVolEnv = 36, // 0x0024
        SustainVolEnv = 37, // 0x0025
        ReleaseVolEnv = 38, // 0x0026
        KeynumToVolEnvHold = 39, // 0x0027
        KeynumToVolEnvDecay = 40, // 0x0028
        Instrument = 41, // 0x0029
        KeyRange = 43, // 0x002B
        VelRange = 44, // 0x002C
        StartloopAddrsCoarseOffset = 45, // 0x002D
        Keynum = 46, // 0x002E
        Velocity = 47, // 0x002F
        InitialAttenuation = 48, // 0x0030
        EndloopAddrsCoarseOffset = 50, // 0x0032
        CoarseTune = 51, // 0x0033
        FineTune = 52, // 0x0034
        SampleID = 53, // 0x0035
        SampleModes = 54, // 0x0036
        ScaleTuning = 56, // 0x0038
        ExclusiveClass = 57, // 0x0039
        OverridingRootKey = 58, // 0x003A
        EndOper = 60 // 0x003C
    }

}
