using System.Numerics;

namespace RSA.MathService.Abstractions
{
    public interface IEulerTotientCalculator
    {
        BigInteger Phi(BigInteger n);
    }
}