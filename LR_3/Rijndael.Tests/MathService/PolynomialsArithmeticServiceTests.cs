using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Rijndael.MathService;
using Rijndael.MathService.Polynomial;
using Rijndael.Tests.Helpers;
using Xunit;

namespace Rijndael.Tests.MathService
{
    public class PolynomialsArithmeticServiceTests
    {
        private readonly IPolynomialsArithmeticService _service =
            new PolynomialsArithmeticService();

        public static IEnumerable<object[]> IrreducibleBytes =>
            Gf2Poly.Irreducible8.Select(b => new object[] { b });

        [Theory]
        [MemberData(nameof(IrreducibleBytes))]
        public void IsIrreducible_ReturnsTrue_ForEveryKnownIrreducible(byte polynomial)
        {
            Assert.True(_service.IsIrreducible(polynomial));
        }

        [Theory]
        [InlineData(0x00)] // x^8
        [InlineData(0x01)] // x^8+1 = (x+1)^8
        [InlineData(0x1A)] // нет свободного члена: делится на x
        [InlineData(0x1F)] // чётное число членов: делится на x+1
        [InlineData(0x05)] // x^8+x^2+1 = (x^4+x+1)^2
        [InlineData(0x41)] // (x^4+x^3+1)^2
        [InlineData(0x55)] // (x^4+x^3+x^2+x+1)^2
        [InlineData(0xBB)] // (x^4+x+1)(x^4+x^3+1)
        [InlineData(0xD1)] // (x^4+x+1)(x^4+x^3+x^2+x+1)
        [InlineData(0x17)] // (x^4+x^3+1)(x^4+x^3+x^2+x+1)
        [InlineData(0x47)] // (x^3+x+1)(x^5+x^2+1)
        [InlineData(0x99)] // приводимый полином
        [InlineData(0xC9)] // (x^2+x+1)(x^6+x+1)
        [InlineData(0xFF)] // приводимый полином
        public void IsIrreducible_ReturnsFalse_ForReducible(byte polynomial)
        {
            Assert.False(_service.IsIrreducible(polynomial));
        }

        [Fact]
        public void IsIrreducible_AgreesWithBruteForceOracle_ForAll256Values()
        {
            for (int b = 0; b < 256; b++)
            {
                bool expected = Gf2Poly.IsIrreducible(0x100 | b);

                Assert.Equal(
                    expected,
                    _service.IsIrreducible((byte)b));

            }
        }

        [Fact]
        public void IrreducibilityPolynomials8_ReturnsExactly30()
        {
            Assert.Equal(30, _service.IrreducibilityPolynomials8().Count);
        }

        [Fact]
        public void IrreducibilityPolynomials8_ReturnsKnownSet_WithoutDuplicates()
        {
            var actual = _service.IrreducibilityPolynomials8()
                .OrderBy(x => x)
                .ToArray();

            Assert.Equal(
                Gf2Poly.Irreducible8.OrderBy(x => x).ToArray(),
                actual);

            Assert.Equal(actual.Length, actual.Distinct().Count());
        }

        [Fact]
        public void IrreducibilityPolynomials8_ContainsAesPolynomial_AndOnlyOddOnes()
        {
            var all = _service.IrreducibilityPolynomials8();

            Assert.Contains((byte)0x1B, all);
            Assert.All(all, p => Assert.Equal(1, p & 1));
        }

        [Fact]
        public void IrreducibilityPolynomials8_ReturnsSameResult_OnRepeatedCalls()
        {
            Assert.Equal(
                _service.IrreducibilityPolynomials8(),
                _service.IrreducibilityPolynomials8());
        }

        private static BigInteger[] Repeat(BigInteger value, int count)
            => Enumerable.Repeat(value, count).ToArray();

        private void AssertFactors(
            BigInteger polynomial,
            params BigInteger[] expected)
        {
            var actual = _service.FactorizePolynomials(polynomial);

            Assert.Equal(
                expected.OrderBy(x => x).ToArray(),
                actual.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void Factorize_X()
            => AssertFactors(2, 2);

        [Fact]
        public void Factorize_XPlusOne()
            => AssertFactors(3, 3);

        [Fact]
        public void Factorize_XSquared()
            => AssertFactors(4, 2, 2);

        [Fact]
        public void Factorize_XPlusOneSquared()
            => AssertFactors(5, 3, 3);

        [Fact]
        public void Factorize_XSquaredPlusXPlusOne_IsIrreducible()
            => AssertFactors(7, 7);

        [Fact]
        public void Factorize_X8()
            => AssertFactors(0x100, Repeat(2, 8));

        [Fact]
        public void Factorize_X8PlusOne()
            => AssertFactors(0x101, Repeat(3, 8));

        [Fact]
        public void Factorize_Ones8()
            => AssertFactors(0x1FF, 0x07, 0x49);

        [Fact]
        public void Factorize_SquareOfQuartic()
            => AssertFactors(0x105, 0x13, 0x13);

        [Fact]
        public void Factorize_TwoDifferentQuartics_1()
            => AssertFactors(0x1BB, 0x13, 0x19);

        [Fact]
        public void Factorize_TwoDifferentQuartics_2()
            => AssertFactors(0x1D1, 0x13, 0x1F);

        [Fact]
        public void Factorize_TwoDifferentQuartics_3()
            => AssertFactors(0x117, 0x19, 0x1F);

        [Fact]
        public void Factorize_Cubic_Times_Quintic()
            => AssertFactors(0x147, 0x0B, 0x25);

        [Fact]
        public void Factorize_Quadratic_Times_Sextic()
            => AssertFactors(0x1C9, 0x07, 0x43);

        [Fact]
        public void Factorize_Irreducible_ReturnsItself_ForAll30()
        {
            foreach (byte b in Gf2Poly.Irreducible8)
                AssertFactors(0x100 | b, 0x100 | b);
        }

        [Fact]
        public void Factorize_IrreducibleOfDegree16_ReturnsItself()
            => AssertFactors(0x1002D, 0x1002D);

        [Fact]
        public void Factorize_ThreeFactors_DifferentDegrees()
            => AssertFactors(0x7155D, 0x07, 0x11B, 0x11D);

        [Fact]
        public void Factorize_Degree24_TwoIrreducibles()
            => AssertFactors(0x11B2ECF, 0x11B, 0x1002D);

        [Fact]
        public void Factorize_BeyondUlong_XPow70()
            => AssertFactors(BigInteger.One << 70, Repeat(2, 70));

        [Fact]
        public void Factorize_BeyondUlong_XPow64PlusOne()
            => AssertFactors(
                (BigInteger.One << 64) | BigInteger.One,
                Repeat(3, 64));

        [Fact]
        public void Factorize_ProductEqualsInput_AndFactorsIrreducible_ForAllUpToDegree11()
        {
            for (int p = 2; p < 4096; p++)
            {
                var factors = _service.FactorizePolynomials(p);

                Assert.True(
                    Gf2Poly.Product(factors) == p,
                    $"product mismatch for {p}");

                foreach (var f in factors)
                {
                    Assert.True(
                        Gf2Poly.IsIrreducible(f),
                        $"factor {f} of {p} is reducible");
                }
            }
        }
    }
}