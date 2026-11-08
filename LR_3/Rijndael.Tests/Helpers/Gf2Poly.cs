using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rijndael.Tests.Helpers
{
    internal static class Gf2Poly
    {
        public static readonly byte[] Irreducible8 =
        {
            0x1B, 0x1D, 0x2B, 0x2D, 0x39, 0x3F, 0x4D, 0x5F, 0x63, 0x65,
            0x69, 0x71, 0x77, 0x7B, 0x87, 0x8B, 0x8D, 0x9F, 0xA3, 0xA9,
            0xB1, 0xBD, 0xC3, 0xCF, 0xD7, 0xDD, 0xE7, 0xF3, 0xF5, 0xF9
        };

        public static int Degree(BigInteger p)
        {
            int d = -1;

            while (p > 0)
            {
                p >>= 1;
                d++;
            }

            return d;
        }
        public static BigInteger Multiply(BigInteger a, BigInteger b)
        {
            BigInteger r = BigInteger.Zero;

            while (!b.IsZero)
            {
                if (!b.IsEven)
                    r ^= a;

                a <<= 1;
                b >>= 1;
            }

            return r;
        }
        public static BigInteger Product(IEnumerable<BigInteger> factors)
        {
            BigInteger r = BigInteger.One;

            foreach (var f in factors)
                r = Multiply(r, f);

            return r;
        }
        public static BigInteger Mod(BigInteger a, BigInteger m)
        {
            if (m <= 0)
                throw new ArgumentOutOfRangeException(nameof(m));

            int dm = Degree(m);

            while (Degree(a) >= dm)
                a ^= m << (Degree(a) - dm);

            return a;
        }
        public static byte MulMod(byte a, byte b, byte module)
            => (byte)Mod(Multiply(a, b), 0x100 | module);

        public static bool IsIrreducible(BigInteger p)
        {
            int d = Degree(p);

            if (d < 1)
                return false;

            for (BigInteger q = 2; Degree(q) <= d / 2; q++)
            {
                if (Mod(p, q).IsZero)
                    return false;
            }

            return true;
        }
    }
}