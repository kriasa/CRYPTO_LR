using System.Numerics;
using System.Reflection.Emit;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Gcd
{
    public sealed class EuclideanGcd : IGcdCalculator
    {
        public BigInteger Gcd(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);

            while (!b.IsZero)
            {
                (a, b) = (b, a % b);
            }
            return a;
        }
    }
}