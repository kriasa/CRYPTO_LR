using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface ICarmichaelCalculator
    {
        BigInteger Lambda(BigInteger n);
    }
}