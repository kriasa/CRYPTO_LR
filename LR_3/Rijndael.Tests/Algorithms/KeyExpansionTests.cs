
using Rijndael.Algorithms;
using Rijndael.Algorithms.Substitution;
using Rijndael.Algorithms.Transforms;
using Rijndael.MathService;
using Xunit;

namespace Rijndael.Tests.Algorithms
{
    public sealed class KeyExpansionTests
    {
        private const byte AesModule = 0x1B;

        private static KeyExpansion CreateExpansion(
            RijndaelBlockSize blockSize,
            RijndaelKeySize keySize)
        {
            var service = new MathGFService();
            var sBox = new SBoxProvider(AesModule, service);

            return new KeyExpansion(
                blockSize,
                keySize,
                AesModule,
                service,
                sBox);
        }

        private static byte[] Hex(string value)
        {
            return Convert.FromHexString(value);
        }

        [Fact]
        public void GenerateRoundKeys_Aes128_ShouldReturnCorrectKeys()
        {
            var key = Hex("000102030405060708090A0B0C0D0E0F");

            var expected = new[]
            {
                "000102030405060708090A0B0C0D0E0F",
                "D6AA74FDD2AF72FADAA678F1D6AB76FE",
                "B692CF0B643DBDF1BE9BC5006830B3FE",
                "B6FF744ED2C2C9BF6C590CBF0469BF41",
                "47F7F7BC95353E03F96C32BCFD058DFD",
                "3CAAA3E8A99F9DEB50F3AF57ADF622AA",
                "5E390F7DF7A69296A7553DC10AA31F6B",
                "14F9701AE35FE28C440ADF4D4EA9C026",
                "47438735A41C65B9E016BAF4AEBF7AD2",
                "549932D1F08557681093ED9CBE2C974E",
                "13111D7FE3944A17F307A78B4D2B30C5"
            };

            var expansion = CreateExpansion(
                RijndaelBlockSize.Bits128,
                RijndaelKeySize.Bits128);

            var actual = expansion.GenerateRoundKeys(key);

            Assert.Equal(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(Hex(expected[i]), actual[i]);
            }
        }

        [Fact]
        public void GenerateRoundKeys_Aes192_ShouldReturnCorrectInitialKeys()
        {
            var key = Hex(
                "000102030405060708090A0B0C0D0E0F1011121314151617");

            var expected = new[]
            {
                "000102030405060708090A0B0C0D0E0F",
                "10111213141516175846F2F95C43F4FE",
                "544AFEF55847F0FA4856E2E95C43F4FE"
            };

            var expansion = CreateExpansion(
                RijndaelBlockSize.Bits128,
                RijndaelKeySize.Bits192);

            var actual = expansion.GenerateRoundKeys(key);

            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(Hex(expected[i]), actual[i]);
            }
        }

        [Fact]
        public void GenerateRoundKeys_Aes256_ShouldReturnCorrectInitialKeys()
        {
            var key = Hex(
                "000102030405060708090A0B0C0D0E0F" +
                "101112131415161718191A1B1C1D1E1F");

            var expected = new[]
            {
                "000102030405060708090A0B0C0D0E0F",
                "101112131415161718191A1B1C1D1E1F",
                "A573C29FA176C498A97FCE93A572C09C",
                "1651A8CD0244BEDA1A5DA4C10640BADE",
                "AE87DFF00FF11B68A68ED5FB03FC1567"
            };

            var expansion = CreateExpansion(
                RijndaelBlockSize.Bits128,
                RijndaelKeySize.Bits256);

            var actual = expansion.GenerateRoundKeys(key);

            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(Hex(expected[i]), actual[i]);
            }
        }

        [Theory]
        [InlineData(RijndaelBlockSize.Bits128, RijndaelKeySize.Bits128, 11)]
        [InlineData(RijndaelBlockSize.Bits128, RijndaelKeySize.Bits192, 13)]
        [InlineData(RijndaelBlockSize.Bits128, RijndaelKeySize.Bits256, 15)]

        [InlineData(RijndaelBlockSize.Bits192, RijndaelKeySize.Bits128, 13)]
        [InlineData(RijndaelBlockSize.Bits192, RijndaelKeySize.Bits192, 13)]
        [InlineData(RijndaelBlockSize.Bits192, RijndaelKeySize.Bits256, 15)]

        [InlineData(RijndaelBlockSize.Bits256, RijndaelKeySize.Bits128, 15)]
        [InlineData(RijndaelBlockSize.Bits256, RijndaelKeySize.Bits192, 15)]
        [InlineData(RijndaelBlockSize.Bits256, RijndaelKeySize.Bits256, 15)]
        public void GenerateRoundKeys_AllConfigurations_ShouldReturnCorrectDimensions(
            RijndaelBlockSize blockSize,
            RijndaelKeySize keySize,
            int expectedRoundKeysCount)
        {
            int keyLength =
                RijndaelSpecification.GetKeyColumnsCount(keySize)
                * RijndaelSpecification.Nb_rows;

            int blockLength =
                RijndaelSpecification.GetColumnsCount(blockSize)
                * RijndaelSpecification.Nb_rows;

            var key = new byte[keyLength];

            var expansion = CreateExpansion(blockSize, keySize);
            var actual = expansion.GenerateRoundKeys(key);

            Assert.Equal(expectedRoundKeysCount, actual.Length);

            foreach (var roundKey in actual)
            {
                Assert.Equal(blockLength, roundKey.Length);
            }
        }

        [Fact]
        public void GenerateRoundKeys_NullKey_ShouldThrowArgumentNullException()
        {
            var expansion = CreateExpansion(
                RijndaelBlockSize.Bits128,
                RijndaelKeySize.Bits128);

            Assert.Throws<ArgumentNullException>(
                () => expansion.GenerateRoundKeys(null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(15)]
        [InlineData(17)]
        [InlineData(24)]
        [InlineData(32)]
        public void GenerateRoundKeys_InvalidKeyLength_ShouldThrowArgumentException(
            int keyLength)
        {
            var expansion = CreateExpansion(
                RijndaelBlockSize.Bits128,
                RijndaelKeySize.Bits128);

            var key = new byte[keyLength];

            Assert.Throws<ArgumentException>(
                () => expansion.GenerateRoundKeys(key));
        }
    }
}
