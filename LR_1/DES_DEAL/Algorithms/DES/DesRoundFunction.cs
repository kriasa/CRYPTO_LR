using System;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Core.Utilities;
using DES_DEAL.Engine.CipherModes;

namespace DES_DEAL.Algorithms.DES
{
    public sealed class DesRoundFunction : IRoundFunction
    {
        private const int BITS_PER_BYTE = 8;
        private const int SBoxCount = 8;
        private const int SBoxInputBits = 6;
        private const int SBoxOutputBits = 4;
        private const int FIRST_BIT_SHIFT = 7;
        private const int LAST_BIT_SHIFT = BITS_PER_BYTE - SBoxInputBits;
        private const int COLUMN_SHIFT = LAST_BIT_SHIFT + 1;
        private const int COLUMN_MASK = 0x0F;
        private const int BIT_MASK = 0x01;
        private const int NIBBLES_PER_BYTE = 2;

        private static readonly int[][] SixBitGroupTables = BuildSixBitGroupTables();

        public byte[] Transform(byte[] block, byte[] roundKey)
        {
            ArgumentNullException.ThrowIfNull(block);
            ArgumentNullException.ThrowIfNull(roundKey);

            byte[] expanded = BitPermutator.Permute(block, DesConstants.Expansion, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            byte[] xored = CipherModeHelpers.Xor(expanded, roundKey);

            byte[] substituted = SubstituteWithSBoxes(xored);

            return BitPermutator.Permute(substituted, DesConstants.Permutation, BitOrder.MsbToLsb, BitIndexBase.OneBased);
        }

        private static byte[] SubstituteWithSBoxes(byte[] xored)
        {
            int outputByteCount = DesConstants.HalfBlockSizeBytes;
            var result = new byte[outputByteCount];

            for (int boxIndex = 0; boxIndex < SBoxCount; boxIndex++)
            {
                byte[] chunk = BitPermutator.Permute(xored, SixBitGroupTables[boxIndex], BitOrder.MsbToLsb, BitIndexBase.OneBased);
                int groupValue = chunk[0];

                int firstBit = (groupValue >> FIRST_BIT_SHIFT) & BIT_MASK;
                int lastBit = (groupValue >> LAST_BIT_SHIFT) & BIT_MASK;
                int row = (firstBit << 1) | lastBit;

                int column = (groupValue >> COLUMN_SHIFT) & COLUMN_MASK;

                int nibble = DesConstants.SBoxes[boxIndex, row, column];

                int byteIndex = boxIndex / NIBBLES_PER_BYTE;
                bool isUpperNibble = (boxIndex % NIBBLES_PER_BYTE == 0);

                result[byteIndex] |= (byte)(isUpperNibble ? (nibble << SBoxOutputBits) : nibble);
            }

            return result;
        }

        private static int[][] BuildSixBitGroupTables()
        {
            var tables = new int[SBoxCount][];
            for (int boxIndex = 0; boxIndex < SBoxCount; boxIndex++)
            {
                var table = new int[SBoxInputBits];
                for (int bit = 0; bit < SBoxInputBits; bit++)
                    table[bit] = boxIndex * SBoxInputBits + bit + 1;
                tables[boxIndex] = table;
            }
            return tables;
        }
    }
}