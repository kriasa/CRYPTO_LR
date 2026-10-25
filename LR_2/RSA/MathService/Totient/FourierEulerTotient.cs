using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Totient
{
    public sealed class FourierEulerTotient : IEulerTotientCalculator
    {
        private readonly IGcdCalculator _gcd;
        public FourierEulerTotient(IGcdCalculator gcd) => _gcd = gcd ?? throw new ArgumentNullException(nameof(gcd));
        public BigInteger Phi(BigInteger n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n > 0");
            double sum = 0.0;

            for (var k = BigInteger.One; k <= n; ++k)
            {
                var gcd = _gcd.Gcd(k, n);
                var angle = (2 * Math.PI * (double)k) / (double)n;
                var cos = Math.Cos(angle);
                sum += (double)gcd * cos;
            }

            return (BigInteger)Math.Round(sum);
        }
    }
}