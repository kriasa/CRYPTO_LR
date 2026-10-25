using System.Numerics;
using CryptoLab.NumberTheory.Abstractions;
using CryptoLab.NumberTheory.Gcd;
using Xunit;

namespace CryptoLab.NumberTheory.Tests.Gcd;

public sealed class BinaryGcdTests : GcdCalculatorContractTests
{
    protected override IGcdCalculator CreateCalculator() => new BinaryGcd();

    [Theory]
    [InlineData(8, 12, 4)]
    [InlineData(64, 48, 16)]
    [InlineData(6, 10, 2)]
    [InlineData(1 << 20, 1 << 10, 1 << 10)]
    public void Gcd_BothEven_PreservesCommonPowerOfTwo(long a, long b, long expected)
    {
        Assert.Equal(new BigInteger(expected), CreateCalculator().Gcd(a, b));
    }

    [Theory]
    [InlineData(32, 9, 1)]
    [InlineData(9, 32, 1)]
    [InlineData(40, 15, 5)]
    public void Gcd_OneEvenOneOdd_IgnoresTwosOfEvenArgument(long a, long b, long expected)
    {
        Assert.Equal(new BigInteger(expected), CreateCalculator().Gcd(a, b));
    }

    [Fact]
    public void Gcd_AgreesWithEuclideanOnRandomInput()
    {
        var binary = CreateCalculator();
        var euclid = new EuclideanGcd();
        var random = new Random(31337);

        for (int i = 0; i < 2000; i++)
        {
            BigInteger a = random.NextInt64(-1_000_000_000, 1_000_000_000);
            BigInteger b = random.NextInt64(-1_000_000_000, 1_000_000_000);

            Assert.Equal(euclid.Gcd(a, b), binary.Gcd(a, b));
        }
    }
}