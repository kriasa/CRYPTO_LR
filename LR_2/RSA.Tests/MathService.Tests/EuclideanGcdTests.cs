using CryptoLab.NumberTheory.Abstractions;
using CryptoLab.NumberTheory.Gcd;

namespace CryptoLab.NumberTheory.Tests.Gcd;

public sealed class EuclideanGcdTests : GcdCalculatorContractTests
{
    protected override IGcdCalculator CreateCalculator() => new EuclideanGcd();
}