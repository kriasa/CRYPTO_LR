using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Symbols
{
    public sealed class JacobiSymbolCalculator : IJacobiSymbolCalculator
    {
        public int JacobiSymbol(BigInteger a, BigInteger n)
        {
            if (n < 1 || n.IsEven) throw new ArgumentOutOfRangeException(nameof(n));
            a = Normalize(a, n);

            int rez = 1;

            while (!a.IsZero)
            {
                while (a.IsEven)
                {
                    rez *= n % 8 == 1 || n % 8 == 7 ? 1 : -1;
                    a >>= 1;
                }
                (a, n) = (n, a);
                rez = a % 4 == 3 && n % 4 == 3 ? -rez : rez;
                a = Normalize(a, n);
            }
            return n == 1 ? rez : 0;
        }
        private static BigInteger Normalize(BigInteger val, BigInteger mod)
        {
            BigInteger remainder = val % mod;
            return remainder.Sign < 0 ? remainder + mod : remainder;
        }
    }
}