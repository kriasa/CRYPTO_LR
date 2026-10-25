using System.ComponentModel.DataAnnotations;
using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Modular
{
    public sealed class ModularExponentiation : IModularExponentiation
    {
        public BigInteger ModPow(BigInteger bas, BigInteger exp, BigInteger mod) 
        {
            ValidateArg (exp, mod);
            if (mod.IsOne) return BigInteger.Zero;
            var power = Normalize(bas, mod);

            BigInteger res = BigInteger.One;

            while (exp > 0)
            {
                if ((exp & 1) == 1) res = (res * power) % mod;
                power = (power * power) % mod;
                exp >>= 1;
            }
            return res;
        }
        private static BigInteger Normalize(BigInteger val, BigInteger mod)
        {
            BigInteger remainder = val % mod;
            return remainder.Sign < 0 ? remainder + mod : remainder;
        }
        private static void ValidateArg(BigInteger exp, BigInteger mod)
        {
            if (mod <= BigInteger.Zero) throw new ArgumentOutOfRangeException(nameof(mod), "mod > 0");

            if (exp < BigInteger.Zero) throw new ArgumentOutOfRangeException(nameof(exp), "exp >= 0");
        }
    }
}