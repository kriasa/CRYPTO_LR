using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Totient
{
    public sealed class FactorizationEulerTotient : IEulerTotientCalculator
    {
        private readonly IFactorizer _factorsFunc;
        public FactorizationEulerTotient(IFactorizer factorsFunc) => _factorsFunc = factorsFunc ?? throw new ArgumentNullException(nameof(factorsFunc));
        public BigInteger Phi(BigInteger n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n > 0");

            BigInteger rez = n;
            foreach (var factor in _factorsFunc.Factorize(n))
            {
                var p = factor.Prime;
                rez = rez * (p - 1) / p;
            }
            return rez;
        }
    }
}