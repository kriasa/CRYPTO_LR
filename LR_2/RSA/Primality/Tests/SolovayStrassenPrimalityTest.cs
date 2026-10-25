using System.Numerics;
using RSA.MathService.Enums;
using RSA.MathService.Abstractions;

namespace RSA.Primality.Test
{
    public sealed class SolovayStrassenPrimalityTest : BasePrimalityTest
    {
        public SolovayStrassenPrimalityTest(IMathTheoryService mathservice) : base(mathservice) { }
        protected override bool PrimalityTest(BigInteger a, BigInteger n)
        {
            var test1 = _mathService.Gcd(a, n, GcdMethod.Binary);
            var testLeft = _mathService.ModPow(a, (n - 1) / 2, n);
            BigInteger jacobi = _mathService.JacobiSymbol(a, n);
            jacobi = jacobi == -1 ? n-1 : jacobi;

            return (test1 == 1 && testLeft == jacobi);
        }
    }
}