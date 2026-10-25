using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface IModularExponentiation
    {
        BigInteger ModPow(BigInteger bas, BigInteger exp, BigInteger mod);
    }
}