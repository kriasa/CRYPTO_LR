using System.Numerics;
using RSA.Algorithms.Models;
using RSA.Attacks.Abstractions;

namespace RSA.Attacks.Wiener
{
    public sealed class WienerAttack : IWienerAttack
    {
        private readonly IContinuedFraction _fractions;
        public WienerAttack(IContinuedFraction fractions)
        {
            _fractions = fractions;
        }
        public AttackResult ExecuteAttack(RsaPublicKey publicKey)
        {
            var continuedFractions = _fractions.GetConvergents(publicKey);
            var e = publicKey.E;
            var n = publicKey.N;
            foreach (var fraction in continuedFractions)
            {
                var (k, d) = fraction;
                if (k.IsZero) continue;
                if ((e * d - 1) % k != 0) continue;
                var phi = (e * d - 1) / k;
                if (CheckedFraction( k, d, e, n, phi)) return new AttackResult(d, phi, continuedFractions);
            }
            throw new InvalidOperationException("failed to attack");
        }
        private static bool CheckedFraction (BigInteger k, BigInteger d,BigInteger e, BigInteger n,BigInteger phi)
        {
            if ((e * d - 1) % k !=0) return false;
            var sum = n - phi + 1;
            var discriminant = sum * sum  - 4 * n;
            if (discriminant < 0) return false;
            var root = BigIntSqrt(discriminant);
            var (p,q) = ((root + sum) / 2, (sum - root) / 2);
            return root * root == discriminant && (root + sum).IsEven && p.Sign > 0 && q.Sign >0;
        }
        private static BigInteger BigIntSqrt(BigInteger d)
        {
            if (d.IsZero) return BigInteger.Zero;

            var x = BigInteger.One << ((int)(d.GetBitLength() + 1) / 2);
            while (true)
            {
                var xNew = (x + d/x)>>1;
                if (xNew >= x) return x;
                x = xNew;
            }
        }
    }
}