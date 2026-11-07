using Xunit;
using Rijndael.MathService.GaloisField;
using System;

namespace Rijndael.Tests.MathService
{
    public class GFArithmeticServiceTests
    {
        private readonly IGFArithmeticService _service;
        private const byte AesModule = 0x1B;

        public GFArithmeticServiceTests()
        {
            _service = new GFArithmeticService();
        }

        [Theory]
        [InlineData(0x57, 0x83, 0xD4)]
        [InlineData(0x00, 0x57, 0x57)]
        [InlineData(0xAA, 0xAA, 0x00)]
        public void Add_ShouldReturnCorrectResult(byte a, byte b, byte expected)
        {
            byte actual = _service.Add(a, b);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0x57, 0x01, 0x57)]
        [InlineData(0x57, 0x00, 0x00)]
        [InlineData(0x57, 0x02, 0xAE)]
        [InlineData(0x57, 0x13, 0xFE)]
        [InlineData(0x57, 0x83, 0xC1)]
        public void Multiply_ShouldReturnCorrectResult(byte a, byte b, byte expected)
        {
            byte actual = _service.Multiply(a, b, AesModule);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0x01, 0x01)]
        [InlineData(0x53, 0xCA)]
        [InlineData(0x9A, 0x9F)]
        public void GetInverse_ShouldReturnCorrectResult(byte a, byte expected)
        {
            byte actual = _service.GetInverse(a, AesModule);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInverse_OfZero_ShouldThrowOrReturnZero()
        {
            byte actual = _service.GetInverse(0x00, AesModule);
            Assert.Equal(0x00, actual);
        }
    }
}