using System.Numerics;
using CryptoLab.NumberTheory.Abstractions;
using CryptoLab.NumberTheory.Symbols;
using Xunit;

namespace CryptoLab.NumberTheory.Tests.Symbols;

public sealed class LegendreSymbolCalculatorTests
{
    private readonly LegendreSymbolCalculator _sut = new(new JacobiSymbolCalculator());

    [Fact]
    public void Constructor_NullJacobi_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LegendreSymbolCalculator(null!));
    }

    [Theory]
    [InlineData(2, 7, 1)]
    [InlineData(3, 7, -1)]
    [InlineData(7, 7, 0)]
    [InlineData(-1, 13, 1)]
    [InlineData(-1, 7, -1)]
    [InlineData(10, 13, 1)]
    [InlineData(5, 13, -1)]
    [InlineData(1, 3, 1)]
    [InlineData(2, 3, -1)]
    public void Legendre_KnownValues_ReturnsExpected(long a, long p, int expected)
    {
        Assert.Equal(expected, _sut.Legendre(a, p));
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(14, 7)]
    [InlineData(-21, 7)]
    public void Legendre_NumeratorDivisibleByP_ReturnsZero(long a, long p)
    {
        Assert.Equal(0, _sut.Legendre(a, p));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-7)]
    [InlineData(4)]
    [InlineData(100)]
    public void Legendre_EvenOrTooSmallModulus_Throws(long p)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Legendre(1, p));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(101)]
    [InlineData(1009)]
    public void Legendre_PrimeModulus_MatchesEulerCriterion(long p)
    {
        for (long a = -3; a <= p + 3; a++)
        {
            BigInteger power = BigInteger.ModPow(((a % p) + p) % p, (p - 1) / 2, p);
            int expected = power.IsZero ? 0 : power.IsOne ? 1 : -1;

            Assert.Equal(expected, _sut.Legendre(a, p));
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(101)]
    [InlineData(1009)]
    public void Legendre_PrimeModulus_HasEqualNumberOfResiduesAndNonResidues(long p)
    {
        int residues = 0, nonResidues = 0;

        for (long a = 1; a < p; a++)
        {
            int symbol = _sut.Legendre(a, p);
            Assert.NotEqual(0, symbol);

            if (symbol == 1) residues++;
            else nonResidues++;
        }

        Assert.Equal((int)((p - 1) / 2), residues);
        Assert.Equal((int)((p - 1) / 2), nonResidues);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(103)]
    public void Legendre_ResultIsConsistentWithSquares(long p)
    {
        // Квадраты по модулю p имеют символ 1, если не делятся на p.
        for (long x = 1; x < p; x++)
        {
            Assert.Equal(1, _sut.Legendre(x * x, p));
        }
    }

    [Theory]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(101)]
    public void Legendre_IsMultiplicativeInNumerator(long p)
    {
        for (long a = 1; a < p; a++)
        {
            for (long b = 1; b < p; b++)
            {
                Assert.Equal(_sut.Legendre(a, p) * _sut.Legendre(b, p), _sut.Legendre(a * b, p));
            }
        }
    }

    [Fact]
    public void Legendre_DelegatesComputationToJacobi()
    {
        var jacobi = new RecordingJacobi(result: -1);
        var sut = new LegendreSymbolCalculator(jacobi);

        int result = sut.Legendre(5, 11);

        Assert.Equal(-1, result);
        Assert.Equal(1, jacobi.CallCount);
        Assert.Equal(new BigInteger(5), jacobi.LastA);
        Assert.Equal(new BigInteger(11), jacobi.LastN);
    }

    [Fact]
    public void Legendre_InvalidModulus_DoesNotReachJacobi()
    {
        var jacobi = new RecordingJacobi(result: 1);
        var sut = new LegendreSymbolCalculator(jacobi);

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Legendre(5, 4));
        Assert.Equal(0, jacobi.CallCount);
    }

    private sealed class RecordingJacobi : IJacobiSymbolCalculator
    {
        private readonly int _result;

        public RecordingJacobi(int result) => _result = result;

        public int CallCount { get; private set; }
        public BigInteger LastA { get; private set; }
        public BigInteger LastN { get; private set; }

        public int Jacobi(BigInteger a, BigInteger n)
        {
            CallCount++;
            LastA = a;
            LastN = n;
            return _result;
        }
    }
}