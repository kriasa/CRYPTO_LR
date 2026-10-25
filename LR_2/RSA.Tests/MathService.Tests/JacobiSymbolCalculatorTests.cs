using System.Numerics;
using CryptoLab.NumberTheory.Symbols;
using Xunit;

namespace CryptoLab.NumberTheory.Tests.Symbols;

public sealed class JacobiSymbolCalculatorTests
{
    private readonly JacobiSymbolCalculator _sut = new();

    [Theory]
    [InlineData(2, 15, 1)]
    [InlineData(7, 15, -1)]
    [InlineData(5, 21, 1)]
    [InlineData(1001, 9907, -1)]
    [InlineData(-3, 35, -1)]
    [InlineData(6, 9, 0)]
    [InlineData(1, 9, 1)]
    [InlineData(2, 3, -1)]
    [InlineData(2, 5, -1)]
    [InlineData(2, 7, 1)]
    [InlineData(2, 17, 1)]
    public void Jacobi_KnownValues_ReturnsExpected(long a, long n, int expected)
    {
        Assert.Equal(expected, _sut.Jacobi(a, n));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-5)]
    [InlineData(123456)]
    public void Jacobi_ModulusOne_AlwaysReturnsOne(long a)
    {
        Assert.Equal(1, _sut.Jacobi(a, 1));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(9)]
    [InlineData(15)]
    [InlineData(1001)]
    public void Jacobi_ZeroNumerator_ReturnsZero(long n)
    {
        Assert.Equal(0, _sut.Jacobi(0, n));
    }

    [Theory]
    [InlineData(3, 9)]
    [InlineData(10, 15)]
    [InlineData(21, 35)]
    [InlineData(14, 7)]
    public void Jacobi_NonCoprimeArguments_ReturnsZero(long a, long n)
    {
        Assert.Equal(0, _sut.Jacobi(a, n));
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, -3)]
    [InlineData(5, 2)]
    [InlineData(5, 10)]
    [InlineData(0, 4)]
    public void Jacobi_EvenOrNonPositiveModulus_Throws(long a, long n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Jacobi(a, n));
    }

    [Fact]
    public void Jacobi_IsPeriodicInNumeratorModuloN()
    {
        var random = new Random(99);

        for (int i = 0; i < 500; i++)
        {
            BigInteger n = random.NextInt64(1, 100_000) * 2 + 1;
            BigInteger a = random.NextInt64(-1_000_000, 1_000_000);

            Assert.Equal(_sut.Jacobi(a, n), _sut.Jacobi(a + n, n));
            Assert.Equal(_sut.Jacobi(a, n), _sut.Jacobi(a - 3 * n, n));
        }
    }

    [Fact]
    public void Jacobi_IsMultiplicativeInNumerator()
    {
        var random = new Random(100);

        for (int i = 0; i < 500; i++)
        {
            BigInteger n = random.NextInt64(1, 100_000) * 2 + 1;
            BigInteger a = random.NextInt64(-10_000, 10_000);
            BigInteger b = random.NextInt64(-10_000, 10_000);

            Assert.Equal(_sut.Jacobi(a, n) * _sut.Jacobi(b, n), _sut.Jacobi(a * b, n));
        }
    }

    [Fact]
    public void Jacobi_IsMultiplicativeInDenominator()
    {
        var random = new Random(101);

        for (int i = 0; i < 500; i++)
        {
            BigInteger m = random.NextInt64(1, 5_000) * 2 + 1;
            BigInteger n = random.NextInt64(1, 5_000) * 2 + 1;
            BigInteger a = random.NextInt64(-100_000, 100_000);

            Assert.Equal(_sut.Jacobi(a, m) * _sut.Jacobi(a, n), _sut.Jacobi(a, m * n));
        }
    }

    [Fact]
    public void Jacobi_QuadraticReciprocity_HoldsForCoprimeOddNumbers()
    {
        // (m/n)·(n/m) = (-1)^((m-1)/2 · (n-1)/2)
        var random = new Random(102);
        int checkedPairs = 0;

        while (checkedPairs < 300)
        {
            BigInteger m = random.NextInt64(1, 50_000) * 2 + 1;
            BigInteger n = random.NextInt64(1, 50_000) * 2 + 1;
            if (!BigInteger.GreatestCommonDivisor(m, n).IsOne)
            {
                continue;
            }

            int exponentParity = (int)(((m - 1) / 2 * ((n - 1) / 2)) % 2);
            int expected = exponentParity == 0 ? 1 : -1;

            Assert.Equal(expected, _sut.Jacobi(m, n) * _sut.Jacobi(n, m));
            checkedPairs++;
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(101)]
    [InlineData(9907)]
    public void Jacobi_PrimeModulus_MatchesEulerCriterion(long p)
    {
        for (long a = -5; a <= p + 5; a++)
        {
            BigInteger power = BigInteger.ModPow(((a % p) + p) % p, (p - 1) / 2, p);
            int expected = power.IsZero ? 0 : power.IsOne ? 1 : -1;

            Assert.Equal(expected, _sut.Jacobi(a, p));
        }
    }

    [Fact]
    public void Jacobi_LargeNumbers_ReturnsValueFromSet()
    {
        BigInteger n = BigInteger.Pow(2, 127) - 1; // простое число Мерсенна
        BigInteger a = BigInteger.Parse("123456789012345678901234567890");

        int result = _sut.Jacobi(a, n);
        BigInteger power = BigInteger.ModPow(a % n, (n - 1) / 2, n);
        int expected = power.IsOne ? 1 : -1;

        Assert.Equal(expected, result);
    }
}