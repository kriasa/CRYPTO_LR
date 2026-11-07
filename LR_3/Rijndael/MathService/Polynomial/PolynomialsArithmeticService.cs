using System.Collections;

namespace Rijndael.MathService
{
    public sealed class PolynomialsArithmeticService
    {
        private readonly int bitPerByte = 8;
        private readonly int sieveConst = 31;
        public bool IrreducibilityTest(byte polynomial)
        {
            return FastTest(polynomial) && TrialDivision(polynomial);
        }
        private bool FastTest(byte polynomial)
        {
            var count = 0;

            for (int i = 0; i < bitPerByte; ++i)
            {
                count += (polynomial >> i) & 1;
            }
            return count % 2 != 0;
        }
        private bool TrialDivision(byte polynomial)
        {
            throw new ArgumentNullException();
        }
        private List<byte> PolynomialSieve()
        {
            var isPrime = new BitArray(sieveConst, true);

            for (int p = 2; p * p <= sieveConst; ++p)
            {
                if (isPrime[p])
                {
                    for (int j = p * p; j <= sieveConst; j += p)
                    {
                        isPrime[j] = false;
                    }
                }
            }

            var res = new List<byte>();
            for (int i = 2; i <= sieveConst; ++i)
            {
                if (isPrime[i]) res.Add((byte)i);
            }
            return res;
        }
    }
}