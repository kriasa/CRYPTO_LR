using System.Numerics;

namespace RSA.MathService.Totient
{
    public interface IFactorizer
    {
        IReadOnlyList<(BigInteger Prime, int Pow)> Factorize(BigInteger n);
    }
}