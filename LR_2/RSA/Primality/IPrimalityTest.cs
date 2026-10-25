using System.Numerics;

namespace RSA.Primality
{
    public interface IPrimalityTest
    {
        bool IsPrime(BigInteger n, double minProbability);
    }
}