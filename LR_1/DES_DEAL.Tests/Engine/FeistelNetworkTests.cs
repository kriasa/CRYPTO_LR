using System;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine;
using Xunit;

namespace DES_DEAL.Tests.Engine
{
    public class FeistelNetworkTests
    {
        // Простейший генератор ключей для теста: просто возвращает массив из 2 раундовых ключей
        private class DummyKeyExpansion : IKeyExpansion
        {
            public byte[][] GenerateRoundKeys(byte[] masterKey)
            {
                return new byte[][]
                {
                    new byte[] { 0x01, 0x02 },
                    new byte[] { 0x03, 0x04 }
                };
            }
        }

        // Простейшая раундовая функция: делает XOR правой половинки с ключом
        private class DummyRoundFunction : IRoundFunction
        {
            public byte[] Transform(byte[] block, byte[] roundKey)
            {
                var result = new byte[block.Length];
                for (int i = 0; i < block.Length; i++)
                {
                    result[i] = (byte)(block[i] ^ roundKey[i % roundKey.Length]);
                }
                return result;
            }
        }

        [Fact]
        public void FeistelNetwork_EncryptAndDecrypt_RestoresOriginalBlock()
        {
            // Arrange
            IKeyExpansion keyExp = new DummyKeyExpansion();
            IRoundFunction roundFunc = new DummyRoundFunction();

            var feistel = new FeistelNetwork(keyExp, roundFunc);

            byte[] masterKey = { 0xFF, 0xFF };
            byte[] originalBlock = { 0xAA, 0xBB, 0xCC, 0xDD }; // 4 байта (по 2 байта на L и R)

            feistel.SetKey(masterKey);

            // Act
            byte[] encrypted = feistel.Encrypt(originalBlock);
            byte[] decrypted = feistel.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(originalBlock, encrypted); // Шифротекст не должен равняться открытому тексту
            Assert.Equal(originalBlock, decrypted);    // После дешифровки должен получиться оригинал
        }

        [Fact]
        public void FeistelNetwork_NullOrInvalidBlock_ThrowsException()
        {
            var feistel = new FeistelNetwork(new DummyKeyExpansion(), new DummyRoundFunction());
            feistel.SetKey(new byte[] { 0x01 });

            // Нечетная длина блока не должна допускаться (нельзя поделить пополам)
            Assert.Throws<ArgumentException>(() => feistel.Encrypt(new byte[] { 0x01, 0x02, 0x03 }));
        }
    }
}