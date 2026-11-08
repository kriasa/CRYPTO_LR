using System.Collections;
using System.ComponentModel;
using System.Numerics;
using Rijndael.MathService.GaloisField;

namespace Rijndael.MathService.Polynomial
{
    public sealed class PolynomialsArithmeticService : IPolynomialsArithmeticService
    {
        private const int BitPerByte = 8;
        private const byte Polynomials8Beginning = 0;
        private const byte Polynomials8End = 255;
        private readonly record struct DivideZnach(BigInteger Quotient, BigInteger Remainder);
        public bool IsIrreducible(byte polynomial)
        {
            return FastTest(polynomial) && TrialDivision(polynomial);
        }
        public IReadOnlyList<byte> IrreducibilityPolynomials8()
        {
            var isPrime = new List<byte>();

            for (int p = Polynomials8Beginning; p <= Polynomials8End; ++p)
            {
                var polynomial = (byte)p;
                if (IsIrreducible(polynomial)) isPrime.Add(polynomial);
            }
            return isPrime;
        }
        public IReadOnlyList<BigInteger> FactorizePolynomials(BigInteger polynomial)
        {
            var res = new List<BigInteger> ();
            if (polynomial < 2) throw new ArgumentOutOfRangeException("degree of a polynomial >=1 ", nameof(polynomial));
            
            while ((polynomial & 1) == 0)
            {
                res.Add(2);
                polynomial >>= 1;
            }

            BigInteger divisor = 3;
            while (2 * GetHighestBit(divisor) <= GetHighestBit(polynomial))
            {
                if (divisor > 3 && HasEvenParity(divisor))
                {
                    divisor += 2;
                    continue;
                }
                while (true)
                {
                    var (quotient, remainder) = Divide(polynomial, divisor);
                    if (remainder != 0) break;

                    res.Add(divisor);
                    polynomial = quotient;
                }
                divisor += 2;
            }

            if (polynomial > 1) res.Add(polynomial);
            return res;
        }
        private bool FastTest(byte polynomial)
        {
            if ((polynomial & 1) == 0) return false;
            return !HasEvenParity((BigInteger)polynomial | (BigInteger.One << BitPerByte));
        }
        private bool TrialDivision(byte polynomial)
        {
            var fullPoly = polynomial | (1 << BitPerByte);

            for (int i = 3; GetHighestBit(i) <= 4; i+=2)
            {
                if (HasEvenParity(i) && i > 3) continue;

                var remainder = Divide(fullPoly, i).Remainder;
                if (remainder == 0) return false;
            }
            return true;
        }
        private DivideZnach Divide (BigInteger a, BigInteger b)
        {
            var remainder = a;
            var quotient = BigInteger.Zero;

            while (GetHighestBit(remainder) >= GetHighestBit(b))
            {
                var (highestBitA, highestBitB)= (GetHighestBit(remainder), GetHighestBit(b));
                var shiftedB = b << (highestBitA - highestBitB);
                remainder ^= shiftedB;
                quotient ^= BigInteger.One << (highestBitA - highestBitB);
            }
            return new DivideZnach(quotient, remainder);
        }
        private static int GetHighestBit (BigInteger a)
        {
            var highestBit = -1;
            while (a > 0)
            {
                a>>=1;
                highestBit++;
            }
            return highestBit;
        }
        private static bool HasEvenParity(BigInteger a)
        {
            int count = 0;
            while (a > 0)
            {
                count += (int)(a & 1);
                a >>= 1;
            }
            return count % 2 == 0;
        }
    }
}