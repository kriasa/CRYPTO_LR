using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Totient
{
    public sealed class DefinitionEulerTotient : IEulerTotientCalculator
    {
        private readonly IGcdCalculator _gcd;
        public DefinitionEulerTotient(IGcdCalculator gcd) => _gcd = gcd ?? throw new ArgumentNullException(nameof(gcd));
        public BigInteger Phi(BigInteger n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof (n), "n > 0");

            BigInteger rez = 0;
            for (BigInteger i = 1; i <= n; ++i)
            {
                if (_gcd.Gcd(i, n).IsOne) rez ++;
            } 
            return rez;
        }
    }
}