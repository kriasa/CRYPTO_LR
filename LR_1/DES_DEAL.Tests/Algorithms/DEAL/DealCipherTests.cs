using System;
using DES_DEAL.Algorithms.DEAL;
using Xunit;

namespace DES_DEAL.Tests.Algorithms.DEAL
{
    public class DealCipherTests
    {
        [Fact]
        public void BlockSizeBytes_IsSixteen()
        {
            // ИСПРАВЛЕНО: BlockSize вместо BlockSizeBytes (название метода сохранено!)
            Assert.Equal(16, new DealCipher().BlockSize);
        }

        [Theory]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(32)]
        public void EncryptThenDecrypt_AllSupportedKeyLengths_ReturnsOriginalBlock(int keyLengthBytes)
        {
            var cipher = new DealCipher();
            cipher.SetKey(BuildKey(keyLengthBytes));

            byte[] block = BuildBlock(16, seed: 0x10);

            byte[] encrypted = cipher.Encrypt(block);
            byte[] decrypted = cipher.Decrypt(encrypted);

            Assert.Equal(block, decrypted);
            Assert.NotEqual(block, encrypted);
        }

        [Fact]
        public void Encrypt_DifferentKeysSameBlock_ProduceDifferentCiphertexts()
        {
            var cipherA = new DealCipher();
            cipherA.SetKey(BuildKey(16, seed: 0x01));

            var cipherB = new DealCipher();
            cipherB.SetKey(BuildKey(16, seed: 0x02));

            byte[] block = BuildBlock(16, seed: 0x10);

            Assert.NotEqual(cipherA.Encrypt(block), cipherB.Encrypt(block));
        }

        [Fact]
        public void Encrypt_SameKeyDifferentKeyLengthVariants_ProduceDifferentCiphertexts()
        {
            var cipher128 = new DealCipher();
            cipher128.SetKey(BuildKey(16));

            var cipher256 = new DealCipher();
            byte[] key256 = new byte[32];
            Array.Copy(BuildKey(16), key256, 16);
            Array.Copy(BuildKey(16, seed: 0x50), 0, key256, 16, 16);
            cipher256.SetKey(key256);

            byte[] block = BuildBlock(16, seed: 0x10);

            Assert.NotEqual(cipher128.Encrypt(block), cipher256.Encrypt(block));
        }

        [Fact]
        public void Encrypt_OneBitFlipInPlaintext_ChangesManyOutputBits()
        {
            var cipher = new DealCipher();
            cipher.SetKey(BuildKey(16));

            byte[] block = BuildBlock(16, seed: 0x10);
            byte[] flipped = (byte[])block.Clone();
            flipped[0] ^= 0x01;

            byte[] resultA = cipher.Encrypt(block);
            byte[] resultB = cipher.Encrypt(flipped);

            int differingBits = CountDifferingBits(resultA, resultB);

            Assert.InRange(differingBits, 32, 96);
        }

        [Fact]
        public void Encrypt_NullBlock_ThrowsArgumentNullException()
        {
            var cipher = new DealCipher();
            cipher.SetKey(BuildKey(16));

            // Добавили ! для подавления warning
            Assert.Throws<ArgumentNullException>(() => cipher.Encrypt(null!));
        }

        [Theory]
        [InlineData(15)]
        [InlineData(17)]
        public void Encrypt_WrongBlockLength_ThrowsArgumentException(int length)
        {
            var cipher = new DealCipher();
            cipher.SetKey(BuildKey(16));

            Assert.Throws<ArgumentException>(() => cipher.Encrypt(new byte[length]));
        }

        [Theory]
        [InlineData(8)]
        [InlineData(20)]
        [InlineData(33)]
        public void SetKey_UnsupportedKeyLength_ThrowsArgumentException(int length)
        {
            var cipher = new DealCipher();

            Assert.Throws<ArgumentException>(() => cipher.SetKey(new byte[length]));
        }

        [Fact]
        public void Encrypt_BeforeSetKey_ThrowsInvalidOperationException()
        {
            var cipher = new DealCipher();

            Assert.Throws<InvalidOperationException>(() => cipher.Encrypt(new byte[16]));
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

        private static byte[] BuildBlock(int length, byte seed)
        {
            var block = new byte[length];
            for (int i = 0; i < length; i++)
            {
                block[i] = (byte)(seed + i * 7);
            }
            return block;
        }

        private static int CountDifferingBits(byte[] a, byte[] b)
        {
            int count = 0;
            for (int i = 0; i < a.Length; i++)
            {
                int diff = a[i] ^ b[i];
                while (diff != 0)
                {
                    count += diff & 1;
                    diff >>= 1;
                }
            }
            return count;
        }
    }
}