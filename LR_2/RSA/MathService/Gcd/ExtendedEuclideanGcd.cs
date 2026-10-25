using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Gcd
{
    public sealed class ExtendedEuclideanGcd : IExtendedGcdCalculator
    {
        public (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            if (a.IsZero && b.IsZero) return (0,0,0);

            int signA = a.Sign;
            int signB = b.Sign;

            var(gcd, x, y) = RecursionGcd(BigInteger.Abs(a), BigInteger.Abs(b));
            return (gcd, x * signA, y * signB);
        }
        private static (BigInteger gcd, BigInteger x, BigInteger y) RecursionGcd(BigInteger a, BigInteger b)
        {
            if (a.IsZero)
            {
                return (b, 0, 1);
            }
            var (gcd, x1, y1) = RecursionGcd(b % a, a);
            var (x, y) = (y1 - (b / a) * x1, x1);
            return (gcd, x, y);
        }
    } 
}