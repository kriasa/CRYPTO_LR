using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface ILegendreSymbolCalculator
    {
        int LegendreSymbol(BigInteger a, BigInteger p);
    }
}