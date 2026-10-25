using System.Numerics;

namespace RSA.MathService.Totient
{
    public sealed class TrialDivisionFactorizer : IFactorizer
    {
        public IReadOnlyList<(BigInteger Prime, int Pow)> Factorize(BigInteger n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n > 0");
            
            var factors = new List<(BigInteger Prime, int Pow)>();
            var remainder = n;

            ExtractFac(ref remainder, 2, factors);

            for (BigInteger i = 3; i * i <= remainder; i+=2)
            {
                ExtractFac(ref remainder, i, factors);
            }

            if (remainder > 1) factors.Add((remainder, 1));
            return factors;
        }
        private static void ExtractFac(ref BigInteger n, BigInteger prime, List<(BigInteger Prime, int Pow)> factors)
        {
            int count = 0;
            while ((n % prime).IsZero)
            {
                count++;
                n /= prime;
            }
            if (count > 0) factors.Add((prime, count));
        }
    }
}