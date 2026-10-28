using System.ComponentModel;
using System.Numerics;
using System.Reflection.Metadata;
using RSA.Algorithms.Models;

namespace RSA.Algorithms.KeyGenerator
{
    public sealed class RsaKeyGenerator : IRsaKeyGenerator
    {
        private PrimaryTest _test;
        private IMathService _mathService;
        private IPrimaryTest _primaryTest;
        private readonly double _probabilite {get;}
        private readonly int _BL {get;}
        private readonly BigInteger eMin = 65537;
        private const int BitPerByte = 8;
        private readonly BigInteger WienerConst = 81;
        private const int FermatConst = 16;
        public RsaKeyGenerator(PrimalityTest test, double probability, int BL)
        {
            _test = test ;
            _probabilite = probability;
            _BL = BL;
            _mathService = new MathService();
            TestSelect();
        }
        public KeyPair GenerateKey()
        {
            while (true)
            {
                var p = GeneratePrime();
                var q = GeneratePrime();
                var n = p * q;
                var phi = (p - 1) * (q - 1);
                var (_, d, _) = _mathService.ExtendedGcd(eMin, phi);
                if (d < 0) d += phi;
                if (p != q && !CheckingAttacks(p,q,d,n)) return new KeyPair(eMin, d, p, q, n);
            }
        }
        private void TestSelect(){
            _primaryTest = _test switch
            {
                PrimaryTest.Fermat => new FermatPrimaryTest(_mathService) ?? throw new ArgumentNullException(),
                PrimaryTest.Solovay => new SolovayStrassenPrimaryTest(_mathService) ?? throw new ArgumentNullException(),
                PrimaryTest.Miller => new MillerRabinPrimaryTest(_mathService) ?? throw new ArgumentNullException(),
                _ => throw new InvalidOperationException("unknown type")
            };
        }
        private bool CheckingValues(BigInteger x) 
            => !_primaryTest.IsPrime(x, _probabilite) || _mathService.Gcd(x-1, eMin) != 1;
        
        private bool CheckingAttacks(BigInteger p, BigInteger q, BigInteger d, BigInteger n) 
            => IsFermatVulnerable(p,q) || IsWienerVulnerable(d,n);
        private BigInteger GeneratePrime()
        {
            while (true)
            {
                var candidate = GenerateRandom(_BL);
                if (!CheckingValues(candidate)) return candidate;
            }
        }
        private BigInteger GenerateRandom(int BL)
        {
            var rez = new byte[BL / BitPerByte];
            Random.Shared.NextBytes(rez);
            int bitPos = (BL - 1) %  BitPerByte;
            rez[^1] |= (byte)(1 << bitPos);
            rez[0] |= 1;
            return new BigInteger(rez, isUnsigned: true);
        }
        private bool IsFermatVulnerable(BigInteger p, BigInteger q)
        {
            return  BigInteger.Abs(p - q) < (BigInteger.One << (_BL - FermatConst));
        }
        private bool IsWienerVulnerable(BigInteger d, BigInteger n)
        {
            return WienerConst * BigInteger.Pow(d, 4) < n;
        }
    }
}