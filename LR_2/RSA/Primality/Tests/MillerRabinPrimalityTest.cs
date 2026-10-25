using System.Numerics;
using RSA.MathService.Enums;
using RSA.MathService.Abstractions;

namespace RSA.Primality.Test
{
    public sealed class MillerRabinPrimalityTest : BasePrimalityTest
    {
        public MillerRabinPrimalityTest(IMathTheoryService mathservice) : base(mathservice) { }
        protected override double ErrorRate => 0.25;
        protected override bool PrimalityTest(BigInteger a, BigInteger n)
        {
            var test1 = _mathService.Gcd(a, n, GcdMethod.Binary);
            var (D, S) = DecomposeNumber(n);

            var test2 = _mathService.ModPow(a, S , n);
            if (test1 == 1 && (test2 == 1 || test2 == n - 1)) return true;

            for (int r = 0; r < D - 1; ++r)
            {
                test2 = _mathService.ModPow(test2, 2, n);
                if (test2 == n - 1) return true; 
            }
            return false;
        }
        private (int D, BigInteger S) DecomposeNumber(BigInteger n)
        {
            var num = n - 1;
            var count = 0;

            while (num.IsEven)
            {
                count++;
                num >>= 1;
            }

            return (count, num);
        }
    }
}