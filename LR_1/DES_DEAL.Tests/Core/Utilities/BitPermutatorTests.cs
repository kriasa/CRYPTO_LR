using System;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Utilities;
using Xunit;

namespace DES_DEAL.Tests.Core.Utilities
{
    /// <summary>
    /// Тесты для <see cref="BitPermutator"/> (Задание 1).
    /// </summary>
    public class BitPermutatorTests
    {
        // 0xB0 = 1011_0000
        private static readonly byte[] SampleInput = { 0xB0 };

        [Fact]
        public void Permute_IdentityTable_MsbToLsb_OneBased_ReturnsSameBytes()
        {
            int[] identityTable = { 1, 2, 3, 4, 5, 6, 7, 8 };

            byte[] result = BitPermutator.Permute(
                SampleInput, identityTable, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            Assert.Equal(SampleInput, result);
        }

        [Fact]
        public void Permute_IdentityTable_LsbToMsb_ZeroBased_ReturnsSameBytes()
        {
            int[] identityTable = { 0, 1, 2, 3, 4, 5, 6, 7 };

            byte[] result = BitPermutator.Permute(
                SampleInput, identityTable, BitOrder.LsbToMsb, BitIndexBase.ZeroBased);

            Assert.Equal(SampleInput, result);
        }

        [Fact]
        public void Permute_ReverseTable_MsbToLsb_ZeroBased_ReversesBitsInByte()
        {
            // Разворот порядка бит внутри байта: 1011_0000 -> 0000_1101
            int[] reverseTable = { 7, 6, 5, 4, 3, 2, 1, 0 };

            byte[] result = BitPermutator.Permute(
                SampleInput, reverseTable, BitOrder.MsbToLsb, BitIndexBase.ZeroBased);

            Assert.Equal(new byte[] { 0x0D }, result);
        }

        [Fact]
        public void Permute_ReverseTable_LsbToMsb_ZeroBased_ReversesBitsInByte()
        {
            // Разворот бит внутри байта не зависит от направления нумерации - результат тот же
            int[] reverseTable = { 7, 6, 5, 4, 3, 2, 1, 0 };

            byte[] result = BitPermutator.Permute(
                SampleInput, reverseTable, BitOrder.LsbToMsb, BitIndexBase.ZeroBased);

            Assert.Equal(new byte[] { 0x0D }, result);
        }

        [Fact]
        public void Permute_ExpandingTable_WithRepeatedBits_ProducesCorrectLongerResult()
        {
            // Расширяющая перестановка (по аналогии с E-блоком DES): 8 бит -> 12 бит,
            // некоторые номера бит повторяются.
            int[] expandingTable = { 8, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 1 };

            byte[] result = BitPermutator.Permute(
                SampleInput, expandingTable, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            Assert.Equal(new byte[] { 0x5A, 0x10 }, result);
        }

        [Fact]
        public void Permute_CompressingTable_ProducesShorterResult()
        {
            // Сжимающая перестановка: 8 бит -> 4 бита (берём только чётные номера бит)
            int[] compressingTable = { 2, 4, 6, 8 };

            byte[] result = BitPermutator.Permute(
                SampleInput, compressingTable, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            Assert.Equal(new byte[] { 0x40 }, result);
        }

        [Fact]
        public void Permute_TableLengthNotMultipleOf8_PadsTailWithZeros()
        {
            // 4 выходных бита должны уложиться в 1 байт, старшие 4 бита результата - "хвост" из нулей
            int[] table = { 1, 1, 1, 1 };

            byte[] result = BitPermutator.Permute(
                SampleInput, table, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            Assert.Single(result);
            // bit1 == 1 (MSB входного 0xB0), значит первые 4 бита результата = 1111, остальные 4 - нули
            Assert.Equal(0b1111_0000, result[0]);
        }

        [Fact]
        public void Permute_MultiByteInput_MsbToLsb_CrossesByteBoundaryCorrectly()
        {
            // MsbToLsb: нумерация идёт от НАЧАЛА массива. bit8 (zero-based) - это последний
            // (младший) бит первого байта, bit9 - первый (старший) бит второго байта.
            byte[] input = { 0b0000_0001, 0b1100_0000 };
            int[] table = { 8, 9 };

            byte[] result = BitPermutator.Permute(
                input, table, BitOrder.MsbToLsb, BitIndexBase.ZeroBased);

            Assert.Equal(new byte[] { 0b1100_0000 }, result);
        }

        [Fact]
        public void Permute_MultiByteInput_LsbToMsb_NumbersFromLastByteBackwards()
        {
            // LsbToMsb: нумерация идёт от КОНЦА массива. Первый (старший) байт значения
            // должен получить номера [8..15] (а не [0..7]), последний байт - [0..7].
            // Извлекаем весь первый байт таблицей {8..15} - должны получить его как есть.
            byte[] input = { 0xB0, 0x0F }; // byte0 = 0xB0 (старший), byte1 = 0x0F (младший)
            int[] tableForFirstByte = { 8, 9, 10, 11, 12, 13, 14, 15 };
            int[] tableForLastByte = { 0, 1, 2, 3, 4, 5, 6, 7 };

            byte[] firstByteResult = BitPermutator.Permute(
                input, tableForFirstByte, BitOrder.LsbToMsb, BitIndexBase.ZeroBased);
            byte[] lastByteResult = BitPermutator.Permute(
                input, tableForLastByte, BitOrder.LsbToMsb, BitIndexBase.ZeroBased);

            Assert.Equal(new byte[] { 0xB0 }, firstByteResult);
            Assert.Equal(new byte[] { 0x0F }, lastByteResult);
        }

        [Fact]
        public void Permute_SingleBit_LsbToMsb_BitZeroIsLsbOfLastByte()
        {
            // Бит 0 в LsbToMsb - младший бит ПОСЛЕДНЕГО байта, а не первого.
            byte[] input = { 0x00, 0x01 }; // младший бит последнего байта = 1

            byte[] result = BitPermutator.Permute(
                input, new[] { 0 }, BitOrder.LsbToMsb, BitIndexBase.ZeroBased);

            Assert.Equal(1, result[0] & 1);
        }

        [Fact]
        public void Permute_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                BitPermutator.Permute(null, new[] { 1 }, BitOrder.MsbToLsb, BitIndexBase.OneBased));
        }

        [Fact]
        public void Permute_NullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                BitPermutator.Permute(SampleInput, null, BitOrder.MsbToLsb, BitIndexBase.OneBased));
        }

        [Fact]
        public void Permute_EmptyTable_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                BitPermutator.Permute(SampleInput, Array.Empty<int>(), BitOrder.MsbToLsb, BitIndexBase.OneBased));
        }

        [Fact]
        public void Permute_OneBasedIndexZero_IsOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // При OneBased минимальный допустимый номер бита - 1, поэтому 0 недопустим
            int[] table = { 0 };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BitPermutator.Permute(SampleInput, table, BitOrder.MsbToLsb, BitIndexBase.OneBased));
        }

        [Fact]
        public void Permute_ZeroBasedIndexEqualToBitLength_IsOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Для 1-байтового входа при ZeroBased допустимы номера 0..7, номер 8 - уже за пределами
            int[] table = { 8 };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BitPermutator.Permute(SampleInput, table, BitOrder.MsbToLsb, BitIndexBase.ZeroBased));
        }

        [Fact]
        public void Permute_OneBasedIndexEqualToBitLengthPlusOne_IsOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Для 1-байтового входа при OneBased допустимы номера 1..8, номер 9 - уже за пределами
            int[] table = { 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BitPermutator.Permute(SampleInput, table, BitOrder.MsbToLsb, BitIndexBase.OneBased));
        }
    }
}