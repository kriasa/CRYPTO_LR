using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface IExtendedGcdCalculator
    {
        (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b);
    }
}