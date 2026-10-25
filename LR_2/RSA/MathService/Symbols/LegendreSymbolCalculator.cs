using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Symbols
{
    public sealed class LegendreSymbolCalculator : ILegendreSymbolCalculator
    {
        private readonly IModularExponentiation _modExp;
        public LegendreSymbolCalculator(IModularExponentiation modExp) =>_modExp = modExp ?? throw new ArgumentNullException(nameof(modExp));
        public int LegendreSymbol(BigInteger a, BigInteger p)
        {
            if (p < 3 || p.IsEven) return throw new ArgumentOutOfRangeException(nameof(p), p, "p - prime number");
            var val = Normalize(a, p);
            if (val.IsZero) return 0;

            var rez= _modExp.ModPow(val, (p - 1) / 2, p);
            if (rez.IsOne) return 1;
            if (rez == p-1) return -1;

            throw new ArgumentException("legendreSym != +-1/0", nameof(p));
        }
        private static BigInteger Normalize(BigInteger val, BigInteger mod)
        {
            BigInteger remainder = val % mod;
            return remainder.Sign < 0 ? remainder + mod : remainder;
        }
    }
}