using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface IGcdCalculator
    {
        BigInteger Gcd(BigInteger a, BigInteger b);
    }
}