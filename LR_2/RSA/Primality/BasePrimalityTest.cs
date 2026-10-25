using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.Primality
{
    public abstract class BasePrimalityTest : IPrimalityTest
    {
        protected readonly IMathTheoryService _mathService;
        protected readonly Random _rng;
        protected BasePrimalityTest(IMathTheoryService mathService, Random? rng = null){
            _mathService = mathService ?? throw new ArgumentNullException(nameof(mathService));
            _rng = rng ?? Random.Shared;
        }
        protected virtual double ErrorRate => 0.5;
        public bool IsPrime(BigInteger n, double minProbability)
        {
            if (minProbability < 0.5 || minProbability >= 1) throw new ArgumentOutOfRangeException(nameof(minProbability));

            if (n == 2 || n == 3) return true;
            if (n < 2 || n.IsEven) return false;

            int itCount = (int)Math.Ceiling(Math.Log(1 - minProbability, ErrorRate));
            for (int i = 0; i < itCount; ++i)
            {
                var a = RandomA(n.GetByteCount()) % (n-3) + 2;
                if (!PrimalityTest(a,n)) return false;
            }
            return true;
        }
        protected abstract bool PrimalityTest(BigInteger a, BigInteger n);
        private BigInteger RandomA(int len)
        {
            if (len <= 0) len = 1;

            var rez = new byte[len];
            _rng.NextBytes(rez);

            return new BigInteger(rez, isUnsigned : true);
        }
    }
}