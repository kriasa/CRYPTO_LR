using System;
using System.Linq;
using DES_DEAL.Algorithms.DEAL;
using Xunit;

namespace DES_DEAL.Tests.Algorithms.DEAL
{
    public class DealKeyExpansionTests
    {
        private static readonly byte[] Key128 = BuildKey(16);
        private static readonly byte[] Key192 = BuildKey(24);
        private static readonly byte[] Key256 = BuildKey(32);

        [Theory]
        [InlineData(16, 6)]
        [InlineData(24, 6)]
        [InlineData(32, 8)]
        public void GenerateRoundKeys_ReturnsExpectedRoundCountForKeyLength(int keyLengthBytes, int expectedRoundCount)
        {
            var expansion = new DealKeyExpansion();
            byte[] key = BuildKey(keyLengthBytes);

            byte[][] roundKeys = expansion.GenerateRoundKeys(key);

            Assert.Equal(expectedRoundCount, roundKeys.Length);
        }

        [Theory]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(32)]
        public void GenerateRoundKeys_EachRoundKeyIsEightBytesLong(int keyLengthBytes)
        {
            var expansion = new DealKeyExpansion();
            byte[] key = BuildKey(keyLengthBytes);

            byte[][] roundKeys = expansion.GenerateRoundKeys(key);

            Assert.All(roundKeys, k => Assert.Equal(8, k.Length));
        }

        [Fact]
        public void GenerateRoundKeys_IsDeterministic()
        {
            var expansion = new DealKeyExpansion();

            byte[][] first = expansion.GenerateRoundKeys(Key128);
            byte[][] second = expansion.GenerateRoundKeys(Key128);

            for (int i = 0; i < first.Length; i++)
            {
                Assert.Equal(first[i], second[i]);
            }
        }

        [Fact]
        public void GenerateRoundKeys_AllRoundKeysAreDistinct()
        {
            var expansion = new DealKeyExpansion();

            byte[][] roundKeys = expansion.GenerateRoundKeys(Key256);

            int distinctCount = roundKeys.Select(Convert.ToBase64String).Distinct().Count();

            Assert.Equal(roundKeys.Length, distinctCount);
        }

        [Fact]
        public void GenerateRoundKeys_DifferentMasterKeys_ProduceDifferentRoundKeys()
        {
            var expansion = new DealKeyExpansion();
            byte[] otherKey = BuildKey(16, seed: 0xFF);

            byte[][] roundKeysA = expansion.GenerateRoundKeys(Key128);
            byte[][] roundKeysB = expansion.GenerateRoundKeys(otherKey);

            Assert.NotEqual(roundKeysA[0], roundKeysB[0]);
        }

        [Fact]
        public void GenerateRoundKeys_DifferentKeyLengths_ProduceDifferentRoundKeyOnceCycleWraps()
        {
            var expansion = new DealKeyExpansion();
            byte[] key192WithSamePrefix = new byte[24];
            Array.Copy(Key128, key192WithSamePrefix, 16);

            byte[][] roundKeys128 = expansion.GenerateRoundKeys(Key128);
            byte[][] roundKeys192 = expansion.GenerateRoundKeys(key192WithSamePrefix);

            Assert.NotEqual(roundKeys128[2], roundKeys192[2]);
        }

        [Fact]
        public void GenerateRoundKeys_DifferentKeyLengthsWithSamePrefix_FirstTwoRoundKeysMatch()
        {
            var expansion = new DealKeyExpansion();
            byte[] key192WithSamePrefix = new byte[24];
            Array.Copy(Key128, key192WithSamePrefix, 16);

            byte[][] roundKeys128 = expansion.GenerateRoundKeys(Key128);
            byte[][] roundKeys192 = expansion.GenerateRoundKeys(key192WithSamePrefix);

            Assert.Equal(roundKeys128[0], roundKeys192[0]);
            Assert.Equal(roundKeys128[1], roundKeys192[1]);
        }

        [Fact]
        public void GenerateRoundKeys_NullKey_ThrowsArgumentNullException()
        {
            var expansion = new DealKeyExpansion();

            // Добавили ! для подавления warning
            Assert.Throws<ArgumentNullException>(() => expansion.GenerateRoundKeys(null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(15)]
        [InlineData(20)]
        [InlineData(33)]
        public void GenerateRoundKeys_UnsupportedKeyLength_ThrowsArgumentException(int length)
        {
            var expansion = new DealKeyExpansion();
            byte[] badKey = new byte[length];

            Assert.Throws<ArgumentException>(() => expansion.GenerateRoundKeys(badKey));
        }

        private static byte[] BuildKey(int length, byte seed = 0x01)
        {
            var key = new byte[length];
            for (int i = 0; i < length; i++)
            {
                key[i] = (byte)(seed + i);
            }
            return key;
        }
    }
}