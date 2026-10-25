using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Totient
{
    public sealed class CarmichaelCalculator : ICarmichaelCalculator
    {
        private readonly IFactorizer _factorizer;
        private readonly IGcdCalculator _gcd;
        public CarmichaelCalculator(IFactorizer factorizer, IGcdCalculator gcd){
            _factorizer = factorizer ?? throw new ArgumentNullException(nameof(factorizer));
            _gcd = gcd ?? throw new ArgumentNullException(nameof(gcd));
        }
        public BigInteger Lambda(BigInteger n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n > 0");

            var lambda = BigInteger.One;

            foreach (var factor in _factorizer.Factorize(n))
            {
                var (p, k) = factor;
                lambda = Lcm(lambda, (Phi(p, k)));
            }
            return lambda;
        }
        private static BigInteger Phi(BigInteger p, int k)=> p==2 ? 
            k switch
            {
                1 => 1,
                2 => 2,
                _ => Pow(p, k-2)
            }
            : Pow (p, k - 1) * (p - 1);
        private static BigInteger Pow(BigInteger bas, BigInteger exp)
        {

            var rez = BigInteger.One;

            while (exp > 0)
            {
                if ((exp & 1) == 1) rez *= bas;
                bas *= bas;
                exp >>= 1;
            }
            return rez;
        }
        private BigInteger Lcm (BigInteger a, BigInteger b) => a / _gcd.Gcd(a, b)*b;
    }
}