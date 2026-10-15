using System;
using DES_DEAL.Core.Enums;

namespace DES_DEAL.Core.Utilities
{
    public static class BitPermutator{
        private const int BITS_PER_BYTE = 8;
        private const int ROUNDING_OFFSET_FOR_BYTES = BITS_PER_BYTE - 1;
        public static byte[] Permute( byte[]? input, int[]? permutationTable, BitOrder bitOrder, BitIndexBase indexBase)
        {
            ValideArgs(input, permutationTable);

            int inputTotalBits = input!.Length * BITS_PER_BYTE;
            int outputTotalBits = permutationTable!.Length;
            int outputByteCount = (outputTotalBits + ROUNDING_OFFSET_FOR_BYTES) / BITS_PER_BYTE;
            var output = new byte[outputByteCount];
            int indexOffset = (indexBase == BitIndexBase.OneBased) ? 1 : 0;

            for (int outputBitIndex = 0; outputBitIndex < outputTotalBits; ++outputBitIndex)
            {
                int rawSourceIndex = permutationTable[outputBitIndex];
                int sourceBitIndex = rawSourceIndex - indexOffset;
                if (sourceBitIndex < 0 || sourceBitIndex >= inputTotalBits) 
                    throw new  ArgumentOutOfRangeException(nameof(permutationTable), rawSourceIndex, "The table references a non-existent element");
                var bitValue = GetBit(input, sourceBitIndex, bitOrder);
                SetBit(output, outputBitIndex, bitValue, bitOrder);
             }

            return output;
        }
        private static bool GetBit(byte[] data, int bitIndex, BitOrder bitOrder)
        {
            (int byteIndex, int shift) = ResolvePosition(bitIndex, data.Length, bitOrder);
            return ((data[byteIndex] >> shift) & 1) == 1;
        }
        private static void SetBit(byte[] data, int bitIndex, bool value, BitOrder bitOrder)
        {
            (int byteIndex, int shift) = ResolvePosition(bitIndex, data.Length, bitOrder);
            if (value)
            {
                data[byteIndex] |= (byte)(1 << shift);
            }
            else
            {
                data[byteIndex] &= (byte) ~ (1 << shift);
            }
        }
        private static (int byteIndex, int shift) ResolvePosition(int bitIndex, int totalByteCount, BitOrder bitOrder)
        {
            int byteIndex;
            int shift;

            if (bitOrder == BitOrder.MsbToLsb)
            {
                byteIndex = bitIndex / BITS_PER_BYTE;
                int bitInByte = bitIndex % BITS_PER_BYTE;
                shift = ROUNDING_OFFSET_FOR_BYTES - bitInByte;
            }
            else
            {
                int byteOffsetFromEnd = bitIndex / BITS_PER_BYTE;
                int bitInByteFromLsb = bitIndex % BITS_PER_BYTE;
                byteIndex = totalByteCount - 1 - byteOffsetFromEnd;
                shift = bitInByteFromLsb;
            }

            return (byteIndex, shift);
        }
        private static void ValideArgs(byte[]? input, int[]? permutationTable)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(permutationTable);
            if (permutationTable.Length == 0) throw new ArgumentException("P-table cannot be empty", nameof(permutationTable));
        }
    }
}