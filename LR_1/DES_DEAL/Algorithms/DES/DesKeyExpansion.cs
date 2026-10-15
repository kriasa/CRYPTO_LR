using System;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Core.Utilities;

namespace DES_DEAL.Algorithms.DES
{
    public sealed class DesKeyExpansion : IKeyExpansion
    {
        private const int BITS_PER_BYTE = 8;
        private const int HALF_KEY_BITS = 28;
        private const int KEY_BYTES = 7;
        private const uint MASK_28_BITS = 0x0FFFFFFF;
        private const byte BYTE_MASK = 0xFF;

        public byte[][] GenerateRoundKeys(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);

            int expectedKeyLength = DesConstants.BlockSizeBytes;
            if (key.Length != expectedKeyLength) throw new ArgumentException($" len(key) = {expectedKeyLength} byte", nameof(key));

            byte[] permutedKey = BitPermutator.Permute(key, DesConstants.Pc1, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            (uint c, uint d) = Split(permutedKey);

            var roundKeys = new byte[DesConstants.RoundCount][];

            for (int round = 0; round < DesConstants.RoundCount; ++round)
            {
                int shift = DesConstants.KeyShifts[round];

                c = RotateLeft(c, shift);
                d = RotateLeft(d, shift);

                byte[] combined56 = Combine(c, d);

                roundKeys[round] = BitPermutator.Permute(combined56, DesConstants.Pc2, BitOrder.MsbToLsb, BitIndexBase.OneBased);
            }

            return roundKeys;
        }
        private static uint RotateLeft(uint value, int shift)
        {
            return ((value << shift) | (value >> (HALF_KEY_BITS - shift))) & MASK_28_BITS;
        }
        private static (uint c, uint d) Split(byte[] key)
        {
            ulong full = 0;
            for (int i = 0; i < KEY_BYTES; i++)
            {
                full = (full << BITS_PER_BYTE) | key[i];
            }

            uint c = (uint)((full >> HALF_KEY_BITS) & MASK_28_BITS);
            uint d = (uint)(full & MASK_28_BITS);

            return (c, d);
        }

        private static byte[] Combine(uint c, uint d)
        {
            ulong full = ((ulong)c << HALF_KEY_BITS) | (d & MASK_28_BITS);
            var result = new byte[KEY_BYTES];

            for (int i = KEY_BYTES - 1; i >= 0; i--)
            {
                result[i] = (byte)(full & BYTE_MASK);
                full >>= BITS_PER_BYTE;
            }

            return result;
        }
    }
}