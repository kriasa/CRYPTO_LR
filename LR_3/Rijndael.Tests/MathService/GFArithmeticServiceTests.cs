using System.Collections.Generic;
using System.Linq;
using Rijndael.MathService.GaloisField;
using Rijndael.Tests.Helpers;
using Xunit;
using Xunit.Sdk;

namespace Rijndael.Tests.MathService
{
    public class GFArithmeticServiceTests
    {
        private readonly IGFArithmeticService _service = new GFArithmeticService();

        private const byte AesModule = 0x1B;

        public static IEnumerable<object[]> AllIrreducibleModules =>
            Gf2Poly.Irreducible8.Select(m => new object[] { m });

        [Theory]
        [InlineData(0x57, 0x83, 0xD4)]
        [InlineData(0x00, 0x57, 0x57)]
        [InlineData(0xAA, 0xAA, 0x00)]
        [InlineData(0xFF, 0xFF, 0x00)]
        [InlineData(0xFF, 0x00, 0xFF)]
        [InlineData(0xFF, 0x01, 0xFE)]
        public void Add_ShouldReturnCorrectResult(byte a, byte b, byte expected)
        {
            Assert.Equal(expected, _service.Add(a, b));
        }

        [Fact]
        public void Add_IsCommutative_AndSelfInverse_ForAllPairs()
        {
            for (int a = 0; a < 256; a++)
            {
                for (int b = 0; b < 256; b++)
                {
                    byte x = (byte)a;
                    byte y = (byte)b;

                    Assert.Equal(_service.Add(x, y), _service.Add(y, x));
                    Assert.Equal(x, _service.Add(_service.Add(x, y), y));
                }
            }
        }

        [Theory]
        [InlineData(0x57, 0x01, 0x57)]
        [InlineData(0x57, 0x00, 0x00)]
        [InlineData(0x57, 0x02, 0xAE)]
        [InlineData(0x57, 0x13, 0xFE)]
        [InlineData(0x57, 0x83, 0xC1)]
        [InlineData(0x00, 0x00, 0x00)]
        [InlineData(0x00, 0xFF, 0x00)]
        [InlineData(0xFF, 0x00, 0x00)]
        [InlineData(0xFF, 0x01, 0xFF)]
        [InlineData(0x01, 0xFF, 0xFF)]
        [InlineData(0x80, 0x02, 0x1B)]
        [InlineData(0xFF, 0xFF, 0x13)]
        public void Multiply_AesModule_ShouldReturnCorrectResult(
            byte a,
            byte b,
            byte expected)
        {
            Assert.Equal(expected, _service.Multiply(a, b, AesModule));
        }

        [Theory]
        [InlineData(0x80, 0x02, 0x1D)]
        [InlineData(0x57, 0x83, 0x31)]
        [InlineData(0xFF, 0xFF, 0xE2)]
        public void Multiply_OtherModule_0x1D_ShouldReturnCorrectResult(
            byte a,
            byte b,
            byte expected)
        {
            Assert.Equal(expected, _service.Multiply(a, b, 0x1D));
        }

        [Fact]
        public void Multiply_WithoutModule_UsesAesModule()
        {
            for (int a = 0; a < 256; a++)
            {
                for (int b = 0; b < 256; b += 7)
                {
                    Assert.Equal(
                        _service.Multiply((byte)a, (byte)b, AesModule),
                        _service.Multiply((byte)a, (byte)b));
                }
            }
        }

        [Fact]
        public void Multiply_ShouldDependOnModule()
        {
            Assert.NotEqual(
                _service.Multiply(0x80, 0x02, 0x1B),
                _service.Multiply(0x80, 0x02, 0x1D));
        }

        [Theory]
        [InlineData(0x1B)]
        [InlineData(0x1D)]
        [InlineData(0xF9)]
        public void Multiply_MatchesIndependentOracle_ForAllPairs(byte module)
        {
            for (int a = 0; a < 256; a++)
            {
                for (int b = 0; b < 256; b++)
                {
                    byte expected = Gf2Poly.MulMod((byte)a, (byte)b, module);
                    byte actual = _service.Multiply((byte)a, (byte)b, module);

                    if (expected != actual)
                    {
                        throw new XunitException(
                            $"module=0x{module:X2}: " +
                            $"0x{a:X2}*0x{b:X2} expected 0x{expected:X2}, " +
                            $"got 0x{actual:X2}");
                    }
                }
            }
        }

        [Fact]
        public void Multiply_IsCommutative_ForAllPairs()
        {
            for (int a = 0; a < 256; a++)
            {
                for (int b = a; b < 256; b++)
                {
                    Assert.Equal(
                        _service.Multiply((byte)a, (byte)b, AesModule),
                        _service.Multiply((byte)b, (byte)a, AesModule));
                }
            }
        }

        [Fact]
        public void Multiply_OneIsIdentity_ZeroIsAbsorbing_ForAllElements()
        {
            for (int a = 0; a < 256; a++)
            {
                Assert.Equal(a, _service.Multiply((byte)a, 0x01, AesModule));
                Assert.Equal(a, _service.Multiply(0x01, (byte)a, AesModule));

                Assert.Equal(0, _service.Multiply((byte)a, 0x00, AesModule));
                Assert.Equal(0, _service.Multiply(0x00, (byte)a, AesModule));
            }
        }

        [Fact]
        public void Multiply_IsAssociative_AndDistributive_OnRandomTriples()
        {
            var rnd = new System.Random(12345);

            for (int i = 0; i < 20000; i++)
            {
                byte a = (byte)rnd.Next(256);
                byte b = (byte)rnd.Next(256);
                byte c = (byte)rnd.Next(256);

                Assert.Equal(
                    _service.Multiply(
                        _service.Multiply(a, b, AesModule),
                        c,
                        AesModule),
                    _service.Multiply(
                        a,
                        _service.Multiply(b, c, AesModule),
                        AesModule));

                Assert.Equal(
                    _service.Multiply(a, _service.Add(b, c), AesModule),
                    _service.Add(
                        _service.Multiply(a, b, AesModule),
                        _service.Multiply(a, c, AesModule)));
            }
        }

        [Theory]
        [MemberData(nameof(AllIrreducibleModules))]
        public void Multiply_HasNoZeroDivisors_ForEveryIrreducibleModule(byte module)
        {
            for (int a = 1; a < 256; a++)
            {
                for (int b = 1; b < 256; b++)
                {
                    if (_service.Multiply((byte)a, (byte)b, module) == 0)
                    {
                        throw new XunitException(
                            $"module=0x{module:X2}: 0x{a:X2}*0x{b:X2} == 0");
                    }
                }
            }
        }

        [Fact]
        public void Multiply_WithReducibleModule_HasZeroDivisors()
        {
            Assert.Equal(0, _service.Multiply(0x03, 0xFF, 0x01));
        }

        [Theory]
        [InlineData(0x01, 0x01)]
        [InlineData(0x02, 0x8D)]
        [InlineData(0x53, 0xCA)]
        [InlineData(0x9A, 0x9F)]
        [InlineData(0x80, 0x83)]
        [InlineData(0xFF, 0x1C)]
        public void GetInverse_AesModule_ShouldReturnCorrectResult(
            byte a,
            byte expected)
        {
            Assert.Equal(expected, _service.GetInverse(a, AesModule));
        }

        [Theory]
        [InlineData(0x02, 0x8E)]
        [InlineData(0x53, 0x8C)]
        [InlineData(0xFF, 0xFD)]
        public void GetInverse_Module0x1D_ShouldReturnCorrectResult(
            byte a,
            byte expected)
        {
            Assert.Equal(expected, _service.GetInverse(a, 0x1D));
        }

        [Fact]
        public void GetInverse_WithoutModule_UsesAesModule()
        {
            for (int a = 0; a < 256; a++)
            {
                Assert.Equal(
                    _service.GetInverse((byte)a, AesModule),
                    _service.GetInverse((byte)a));
            }
        }

        [Fact]
        public void GetInverse_OfZero_ReturnsZero()
        {
            Assert.Equal(0, _service.GetInverse(0x00, AesModule));
        }

        [Fact]
        public void GetInverse_TimesOriginal_IsOne_ForAllNonZero()
        {
            for (int a = 1; a < 256; a++)
            {
                Assert.Equal(
                    1,
                    _service.Multiply(
                        (byte)a,
                        _service.GetInverse((byte)a, AesModule),
                        AesModule));
            }
        }

        [Theory]
        [MemberData(nameof(AllIrreducibleModules))]
        public void GetInverse_TimesOriginal_IsOne_ForEveryIrreducibleModule(byte module)
        {
            for (int a = 1; a < 256; a++)
            {
                byte inv = _service.GetInverse((byte)a, module);

                if (_service.Multiply((byte)a, inv, module) != 1)
                {
                    throw new XunitException(
                        $"module=0x{module:X2}: " +
                        $"a=0x{a:X2}, inverse=0x{inv:X2}");
                }
            }
        }

        [Fact]
        public void GetInverse_IsInvolution_AndBijection()
        {
            for (int a = 1; a < 256; a++)
            {
                byte inv = _service.GetInverse((byte)a, AesModule);

                Assert.Equal(
                    a,
                    _service.GetInverse(inv, AesModule));
            }

            int distinct = Enumerable.Range(1, 255)
                .Select(a => _service.GetInverse((byte)a, AesModule))
                .Distinct()
                .Count();

            Assert.Equal(255, distinct);
        }
    }
}