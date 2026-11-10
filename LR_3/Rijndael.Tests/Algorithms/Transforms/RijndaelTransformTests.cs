
using System.Linq;
using Rijndael.Algorithms;
using Rijndael.Algorithms.Substitution;
using Rijndael.Algorithms.Transforms;
using Rijndael.MathService;
using Xunit;

namespace Rijndael.Tests.Algorithms.Transforms
{
    public sealed class RijndaelTransformTests
    {
        private const byte AesModule = 0x1B;

        private static RijndaelTransform CreateTransform(
            RijndaelBlockSize blockSize)
        {
            var service = new MathGFService();
            var sBoxProvider = new SBoxProvider(AesModule, service);

            return new RijndaelTransform(
                blockSize,
                AesModule,
                service,
                sBoxProvider);
        }

        private static byte[] CreateState(RijndaelBlockSize blockSize)
        {
            return Enumerable.Range(0, (int)blockSize)
                .Select(i => (byte)i)
                .ToArray();
        }

        [Theory]
        [InlineData(0x00, 0x63)]
        [InlineData(0x01, 0x7C)]
        [InlineData(0x53, 0xED)]
        [InlineData(0x7C, 0x10)]
        [InlineData(0xFF, 0x16)]
        public void SubBytes_ShouldReturnCorrectValues(
            byte input,
            byte expected)
        {
            var transform = CreateTransform(RijndaelBlockSize.Bits128);
            byte[] data = { input };

            transform.SubBytes(data);

            Assert.Equal(expected, data[0]);
        }

        [Theory]
        [InlineData(0x00, 0x52)]
        [InlineData(0x63, 0x00)]
        [InlineData(0xED, 0x53)]
        [InlineData(0x10, 0x7C)]
        [InlineData(0x16, 0xFF)]
        public void InvSubBytes_ShouldReturnCorrectValues(
            byte input,
            byte expected)
        {
            var transform = CreateTransform(RijndaelBlockSize.Bits128);
            byte[] data = { input };

            transform.InvSubBytes(data);

            Assert.Equal(expected, data[0]);
        }

        [Theory]
        [InlineData(RijndaelBlockSize.Bits128)]
        [InlineData(RijndaelBlockSize.Bits192)]
        [InlineData(RijndaelBlockSize.Bits256)]
        public void InvSubBytes_AfterSubBytes_ShouldRestoreOriginalState(
            RijndaelBlockSize blockSize)
        {
            var transform = CreateTransform(blockSize);
            byte[] data = CreateState(blockSize);
            byte[] original = data.ToArray();

            transform.SubBytes(data);
            transform.InvSubBytes(data);

            Assert.Equal(original, data);
        }

        [Fact]
        public void ShiftRows_For128BitBlock_ShouldReturnCorrectState()
        {
            var transform = CreateTransform(RijndaelBlockSize.Bits128);

            byte[] data = Enumerable.Range(0, 16)
                .Select(i => (byte)i)
                .ToArray();

            byte[] expected =
            {
                0, 5, 10, 15,
                4, 9, 14, 3,
                8, 13, 2, 7,
                12, 1, 6, 11
            };

            transform.ShiftRows(data);

            Assert.Equal(expected, data);
        }

        [Theory]
        [InlineData(RijndaelBlockSize.Bits128)]
        [InlineData(RijndaelBlockSize.Bits192)]
        [InlineData(RijndaelBlockSize.Bits256)]
        public void InvShiftRows_AfterShiftRows_ShouldRestoreOriginalState(
            RijndaelBlockSize blockSize)
        {
            var transform = CreateTransform(blockSize);
            byte[] data = CreateState(blockSize);
            byte[] original = data.ToArray();

            transform.ShiftRows(data);
            transform.InvShiftRows(data);

            Assert.Equal(original, data);
        }

        [Fact]
        public void AddRoundKey_ShouldXorStateWithRoundKey()
        {
            var transform = CreateTransform(RijndaelBlockSize.Bits128);

            byte[] data = { 0x57, 0x83, 0x1B, 0xCA };
            byte[] roundKey = { 0xFF, 0x01, 0x1B, 0xCA };

            byte[] expected = { 0xA8, 0x82, 0x00, 0x00 };

            transform.AddRoundKey(data, roundKey);

            Assert.Equal(expected, data);
        }

        [Fact]
        public void AddRoundKey_WithDifferentLengths_ShouldThrowArgumentException()
        {
            var transform = CreateTransform(RijndaelBlockSize.Bits128);

            byte[] data = { 0x01, 0x02 };
            byte[] roundKey = { 0x01 };

            Assert.Throws<ArgumentException>(
                () => transform.AddRoundKey(data, roundKey));
        }

        [Theory]
        [InlineData(RijndaelBlockSize.Bits128)]
        [InlineData(RijndaelBlockSize.Bits192)]
        [InlineData(RijndaelBlockSize.Bits256)]
        public void MixColumns_ShouldReturnCorrectResult(
            RijndaelBlockSize blockSize)
        {
            var transform = CreateTransform(blockSize);

            byte[] data = new byte[(int)blockSize];

            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0xDB;
                data[i + 1] = 0x13;
                data[i + 2] = 0x53;
                data[i + 3] = 0x45;
            }

            byte[] expected = new byte[data.Length];

            for (int i = 0; i < expected.Length; i += 4)
            {
                expected[i] = 0x8E;
                expected[i + 1] = 0x4D;
                expected[i + 2] = 0xA1;
                expected[i + 3] = 0xBC;
            }

            transform.MixColumns(data);

            Assert.Equal(expected, data);
        }

        [Theory]
        [InlineData(RijndaelBlockSize.Bits128)]
        [InlineData(RijndaelBlockSize.Bits192)]
        [InlineData(RijndaelBlockSize.Bits256)]
        public void InvMixColumns_AfterMixColumns_ShouldRestoreOriginalState(
            RijndaelBlockSize blockSize)
        {
            var transform = CreateTransform(blockSize);
            byte[] data = CreateState(blockSize);
            byte[] original = data.ToArray();

            transform.MixColumns(data);
            transform.InvMixColumns(data);

            Assert.Equal(original, data);
        }
    }
}
