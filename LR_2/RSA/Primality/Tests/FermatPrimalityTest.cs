using System.Numerics;
using RSA.MathService.Enums;
using RSA.MathService.Abstractions;

namespace RSA.Primality.Test
{
    public sealed class FermatPrimalityTest : BasePrimalityTest
    {
        public FermatPrimalityTest(IMathTheoryService mathservice) : base (mathservice){}
        protected override bool PrimalityTest(BigInteger a, BigInteger n)
        {
            var test1 = _mathService.Gcd(a, n, GcdMethod.Binary);
            var test2 = _mathService.ModPow(a, n-1, n);
            return (test1 == 1 && test2 == 1);
        }
    }
}