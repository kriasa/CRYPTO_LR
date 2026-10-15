using System;
using DES_DEAL.Algorithms.DES;
using Xunit;

namespace DES_DEAL.Tests.Algorithms.DES
{
    public class DesKeyExpansionTests
    {
        [Fact]
        public void ExpandKey_Valid8ByteKey_Returns16RoundKeysOf48Bits()
        {
            // Arrange
            var keyExpansion = new DesKeyExpansion();
            // Стандартный тестовый ключ DES (8 байт / 64 бита)
            byte[] masterKey = { 0x13, 0x34, 0x57, 0x79, 0x9B, 0xBC, 0xDF, 0xF1 };

            // Act
            byte[][] roundKeys = keyExpansion.GenerateRoundKeys(masterKey);

            // Assert
            // У DES ровно 16 раундов -> должно быть 16 раундовых ключей
            Assert.NotNull(roundKeys);
            Assert.Equal(16, roundKeys.Length);

            // Каждый раундовый ключ должен весить ровно 48 бит = 6 байт (по стандарту PC-2)
            foreach (byte[] roundKey in roundKeys)
            {
                Assert.NotNull(roundKey);
                Assert.Equal(6, roundKey.Length);
            }
        }

        [Fact]
        public void ExpandKey_InvalidKeyLength_ThrowsArgumentException()
        {
            var keyExpansion = new DesKeyExpansion();
            byte[] invalidKey = { 0x01, 0x02 }; // Слишком короткий ключ (не 8 байт)

            Assert.Throws<ArgumentException>(() => keyExpansion.GenerateRoundKeys(invalidKey));
        }

        [Fact]
        public void ExpandKey_NullKey_ThrowsArgumentNullException()
        {
            var keyExpansion = new DesKeyExpansion();

            Assert.Throws<ArgumentNullException>(() => keyExpansion.GenerateRoundKeys(null!));
        }
    }
}