using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.SF2 {

    /// <summary>
    /// SF2 Generator.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SF2GeneratorAmount {
        [FieldOffset(0)] public byte LowByte;
        [FieldOffset(1)] public byte HighByte;
        [FieldOffset(0)] public short Amount;
        [FieldOffset(0)] public ushort UAmount;
        public override string ToString() => $"BLo = {LowByte}, BHi = {HighByte}, Sh = {Amount}, U = {UAmount}";
    }

}
