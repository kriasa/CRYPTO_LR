using System.Numerics;
using CryptoLab.NumberTheory.Abstractions;
using Xunit;

namespace CryptoLab.NumberTheory.Tests.Gcd;

/// <summary>
/// Общий контракт для любой реализации <see cref="IGcdCalculator"/>.
/// Наследники только создают тестируемый калькулятор, тесты исполняются для каждого из них.
/// </summary>
public abstract class GcdCalculatorContractTests
{
    protected abstract IGcdCalculator CreateCalculator();

    [Theory]
    [InlineData(48, 18, 6)]
    [InlineData(18, 48, 6)]
    [InlineData(270, 192, 6)]
    [InlineData(17, 31, 1)]
    [InlineData(7, 7, 7)]
    [InlineData(1, 1, 1)]
    [InlineData(1024, 4096, 1024)]
    [InlineData(100, 75, 25)]
    [InlineData(2147483647, 46341, 1)]
    public void Gcd_KnownPositiveValues_ReturnsExpected(long a, long b, long expected)
    {
        Assert.Equal(new BigInteger(expected), CreateCalculator().Gcd(a, b));
    }

    [Theory]
    [InlineData(0, 5, 5)]
    [InlineData(5, 0, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(0, -9, 9)]
    [InlineData(-9, 0, 9)]
    public void Gcd_WithZero_ReturnsAbsoluteOfOtherArgument(long a, long b, long expected)
    {
        Assert.Equal(new BigInteger(expected), CreateCalculator().Gcd(a, b));
    }

    [Theory]
    [InlineData(-12, 18, 6)]
    [InlineData(12, -18, 6)]
    [InlineData(-12, -18, 6)]
    [InlineData(-7, 13, 1)]
    public void Gcd_NegativeArguments_ReturnsNonNegativeResult(long a, long b, long expected)
    {
        BigInteger result = CreateCalculator().Gcd(a, b);

        Assert.True(result.Sign >= 0);
        Assert.Equal(new BigInteger(expected), result);
    }

    [Fact]
    public void Gcd_LargeNumbers_ReturnsExpected()
    {
        BigInteger a = BigInteger.Parse("123456789012345678901234567890");
        BigInteger b = BigInteger.Parse("987654321098765432109876543210");
        BigInteger expected = BigInteger.Parse("9000000000900000000090");

        Assert.Equal(expected, CreateCalculator().Gcd(a, b));
    }

    [Fact]
    public void Gcd_LargePowersOfTwoAndThree_ReturnsCommonPart()
    {
        BigInteger a = BigInteger.Pow(2, 200) * BigInteger.Pow(3, 5);
        BigInteger b = BigInteger.Pow(2, 150) * BigInteger.Pow(3, 9) * 7;
        BigInteger expected = BigInteger.Pow(2, 150) * BigInteger.Pow(3, 5);

        Assert.Equal(expected, CreateCalculator().Gcd(a, b));
    }

    [Fact]
    public void Gcd_ConsecutiveFibonacciNumbers_AreCoprime()
    {
        BigInteger prev = 1, curr = 1;
        for (int i = 0; i < 300; i++)
        {
            Assert.Equal(BigInteger.One, CreateCalculator().Gcd(prev, curr));
            (prev, curr) = (curr, prev + curr);
        }
    }

    [Fact]
    public void Gcd_IsCommutative()
    {
        var calculator = CreateCalculator();
        var random = new Random(12345);

        for (int i = 0; i < 500; i++)
        {
            BigInteger a = random.NextInt64(-1_000_000, 1_000_000);
            BigInteger b = random.NextInt64(-1_000_000, 1_000_000);

            Assert.Equal(calculator.Gcd(a, b), calculator.Gcd(b, a));
        }
    }

    [Fact]
    public void Gcd_ScalesLinearly()
    {
        // gcd(k·a, k·b) = k · gcd(a, b) для k > 0
        var calculator = CreateCalculator();
        var random = new Random(777);

        for (int i = 0; i < 500; i++)
        {
            BigInteger a = random.NextInt64(1, 100_000);
            BigInteger b = random.NextInt64(1, 100_000);
            BigInteger k = random.NextInt64(1, 1_000);

            Assert.Equal(k * calculator.Gcd(a, b), calculator.Gcd(k * a, k * b));
        }
    }

    [Fact]
    public void Gcd_ResultDividesBothArguments()
    {
        var calculator = CreateCalculator();
        var random = new Random(2024);

        for (int i = 0; i < 500; i++)
        {
            BigInteger a = random.NextInt64(1, 10_000_000);
            BigInteger b = random.NextInt64(1, 10_000_000);

            BigInteger g = calculator.Gcd(a, b);

            Assert.True((a % g).IsZero);
            Assert.True((b % g).IsZero);
        }
    }
}