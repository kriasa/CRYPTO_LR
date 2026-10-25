using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface IJacobiSymbolCalculator
    {
        int JacobiSymbol(BigInteger a, BigInteger p);
    }
}