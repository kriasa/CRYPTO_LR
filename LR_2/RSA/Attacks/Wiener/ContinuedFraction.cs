using System.Numerics;
using System.Transactions;
using RSA.Attacks.Abstractions;
using RSA.Algorithms.Models;

namespace RSA.Attacks.Wiener
{
    public sealed class ContinuedFraction : IContinuedFraction
    {
        public IEnumerable<Fraction> GetConvergents(RsaPublicKey publicKey)
        {
            var coefficientsA = EuclidianDecomposition(publicKey.E, publicKey.N);
            return GetConvergentsPairs(coefficientsA);
        }
        private static List<BigInteger> EuclidianDecomposition(BigInteger e, BigInteger n)
        {
            var resultA = new List<BigInteger>();

            var currentE = e;
            var currentN = n;

            while (!currentN.IsZero)
            {
                resultA.Add(currentE / currentN);
                (currentE, currentN) = (currentN, currentE % currentN);
            }
            return resultA;
        }
        private static IEnumerable<Fraction> GetConvergentsPairs(List<BigInteger> a)
        {
            var convergents = new List<Fraction>();

            var pLast2 = BigInteger.Zero;
            var qLast2 = BigInteger.One;
            var pLast1 = BigInteger.One;
            var qLast1 = BigInteger.Zero;

            foreach (var coeff in a)
            {
                var p = pLast1 * coeff + pLast2;
                var q = qLast1 * coeff + qLast2;

                convergents.Add(new Fraction(p, q));

                (pLast2, qLast2) = (pLast1, qLast1);
                (pLast1, qLast1) = (p, q);
            }
            return convergents;
        }
    }
}