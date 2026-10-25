using System.Numerics;
using CryptoLab.NumberTheory.Gcd;
using CryptoLab.NumberTheory.Models;
using Xunit;

namespace CryptoLab.NumberTheory.Tests.Gcd;

public sealed class ExtendedEuclideanGcdTests
{
    private readonly ExtendedEuclideanGcd _sut = new();

    private static void AssertBezout(BigInteger a, BigInteger b, BezoutResult result)
    {
        Assert.Equal(result.Gcd, a * result.X + b * result.Y);
    }

    [Fact]
    public void ExtendedGcd_ClassicExample_ReturnsKnownCoefficients()
    {
        BezoutResult result = _sut.ExtendedGcd(240, 46);

        Assert.Equal(new BigInteger(2), result.Gcd);
        Assert.Equal(new BigInteger(-9), result.X);
        Assert.Equal(new BigInteger(47), result.Y);
    }

    [Theory]
    [InlineData(240, 46, 2)]
    [InlineData(35, 15, 5)]
    [InlineData(101, 13, 1)]
    [InlineData(17, 31, 1)]
    [InlineData(48, 18, 6)]
    [InlineData(7, 7, 7)]
    [InlineData(1, 1, 1)]
    public void ExtendedGcd_PositiveInput_ReturnsGcdAndSatisfiesBezout(long a, long b, long expectedGcd)
    {
        BezoutResult result = _sut.ExtendedGcd(a, b);

        Assert.Equal(new BigInteger(expectedGcd), result.Gcd);
        AssertBezout(a, b, result);
    }

    [Theory]
    [InlineData(-12, 18, 6)]
    [InlineData(12, -18, 6)]
    [InlineData(-12, -18, 6)]
    [InlineData(-7, 13, 1)]
    public void ExtendedGcd_NegativeInput_ReturnsNonNegativeGcdAndSatisfiesBezout(long a, long b, long expectedGcd)
    {
        BezoutResult result = _sut.ExtendedGcd(a, b);

        Assert.True(result.Gcd.Sign >= 0);
        Assert.Equal(new BigInteger(expectedGcd), result.Gcd);
        AssertBezout(a, b, result);
    }

    [Theory]
    [InlineData(5, 0, 5)]
    [InlineData(0, 5, 5)]
    [InlineData(-5, 0, 5)]
    [InlineData(0, -5, 5)]
    public void ExtendedGcd_OneArgumentZero_ReturnsAbsoluteOfOther(long a, long b, long expectedGcd)
    {
        BezoutResult result = _sut.ExtendedGcd(a, b);

        Assert.Equal(new BigInteger(expectedGcd), result.Gcd);
        AssertBezout(a, b, result);
    }

    [Fact]
    public void ExtendedGcd_BothZero_ReturnsZeroGcd()
    {
        BezoutResult result = _sut.ExtendedGcd(0, 0);

        Assert.Equal(BigInteger.Zero, result.Gcd);
        AssertBezout(0, 0, result);
    }

    [Fact]
    public void ExtendedGcd_LargeNumbers_SatisfiesBezoutAndMatchesEuclid()
    {
        BigInteger a = BigInteger.Parse("123456789012345678901234567890");
        BigInteger b = BigInteger.Parse("987654321098765432109876543210");

        BezoutResult result = _sut.ExtendedGcd(a, b);

        Assert.Equal(BigInteger.Parse("9000000000900000000090"), result.Gcd);
        AssertBezout(a, b, result);
    }

    [Fact]
    public void ExtendedGcd_CoprimeInput_GivesModularInverse()
    {
        // Если gcd(a, m) = 1, то x — обратный к a по модулю m.
        BigInteger a = 17, m = 3120; // классический пример RSA: e = 17, φ = 3120
        BezoutResult result = _sut.ExtendedGcd(a, m);

        BigInteger inverse = ((result.X % m) + m) % m;

        Assert.Equal(BigInteger.One, result.Gcd);
        Assert.Equal(new BigInteger(2753), inverse);
        Assert.Equal(BigInteger.One, a * inverse % m);
    }

    [Fact]
    public void ExtendedGcd_AgreesWithEuclideanAndSatisfiesBezoutOnRandomInput()
    {
        var euclid = new EuclideanGcd();
        var random = new Random(4242);

        for (int i = 0; i < 2000; i++)
        {
            BigInteger a = random.NextInt64(-1_000_000_000, 1_000_000_000);
            BigInteger b = random.NextInt64(-1_000_000_000, 1_000_000_000);

            BezoutResult result = _sut.ExtendedGcd(a, b);

            Assert.Equal(euclid.Gcd(a, b), result.Gcd);
            AssertBezout(a, b, result);
        }
    }
}