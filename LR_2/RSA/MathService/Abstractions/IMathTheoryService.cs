using System.Numerics;
using RSA.MathService.Enums;

namespace RSA.MathService.Abstractions
{
    public interface IMathTheoryService
    {
        int JacobiSymbol (BigInteger a, BigInteger n);
        int LegendreSymbol(BigInteger a, BigInteger p);
        (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b);
        BigInteger ModPow(BigInteger a, BigInteger pow, BigInteger mod);
        BigInteger Lambda(BigInteger n);
        BigInteger Gcd(BigInteger a, BigInteger b, GcdMethod method);
        BigInteger Phi(BigInteger n,EulerMethod method);
    }
}