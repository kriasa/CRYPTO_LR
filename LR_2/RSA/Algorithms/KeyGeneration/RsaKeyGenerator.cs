using System.ComponentModel;
using System.Numerics;
using System.Reflection.Metadata;
using RSA.Algorithms.Models;
using RSA.MathService.Abstractions;
using RSA.Primality;
using RSA.MathService.Enums;
using System.Security.Cryptography;
using RSA.MathService;
using RSA.Primality.Test;

namespace RSA.Algorithms.KeyGenerator
{
    public partial class RsaKeyGenerator : IRsaKeyGenerator
    {
        private RsaService.PrimalityTest _test;
        private IMathTheoryService _mathService;
        private readonly IPrimalityTest _primalityTest;
        private double _probability {get;}
        private int _BL {get;}
        private static readonly BigInteger PublicExponent = 65537;
        private const int BitPerByte = 8;
        private static readonly BigInteger WienerConst = 81;
        private const int FermatConst = 100;
        public RsaKeyGenerator(RsaService.PrimalityTest test, double probability, int BL) 
            : this(test, probability, BL, new MathTheoryService())
        {
        }
        public RsaKeyGenerator(RsaService.PrimalityTest test, double probability, int BL, IMathTheoryService mathTheoryService)
        {
            _test = test;
            _probability = probability;
            _BL = BL;
            _mathService = mathTheoryService;
            _primalityTest = CreatePrimalityTest(test, _mathService);
        }
        public KeyPair GenerateKey(bool vulnerable = false)
        {
            while (true)
            {
                var p = GeneratePrime();
                var q = GeneratePrime();
                if (p == q) continue;
                var n = p * q;
                var phi = (p - 1) * (q - 1);
                if (vulnerable)
                {
                    var d = GenerateSmall(n, phi);
                    var (_, e, _) = _mathService.ExtendedGcd(d, phi);
                    if (e < 0) e += phi;
                    if (IsWienerVulnerable(d, n) && _mathService.Gcd(e, phi, GcdMethod.Binary) == 1)
                    {
                        return new KeyPair(e, d, p, q, n);
                    }
                }
                else
                {
                    var (_, d, _) = _mathService.ExtendedGcd(PublicExponent, phi);
                    if (d < 0) d += phi;
                    if (!CheckingAttacks(p, q, d, n)) return new KeyPair(PublicExponent, d, p, q, n);
                }
            }
        }
        private BigInteger GenerateSmall(BigInteger n, BigInteger phi)
        {
            int bits = (int)n.GetBitLength() / 4 - 2;
            if (bits <= 1) bits = 2;

            while (true)
            {
                var candidate = GenerateRandom(bits);
                if (candidate > 1 && _mathService.Gcd(candidate, phi, GcdMethod.Binary) == 1)
                {
                    return candidate;
                }
            }
        }
        private static IPrimalityTest CreatePrimalityTest(RsaService.PrimalityTest test, IMathTheoryService mathService)
        {
            return test switch
            {
                RsaService.PrimalityTest.Fermat => new FermatPrimalityTest(mathService),
                RsaService.PrimalityTest.Solovay => new SolovayStrassenPrimalityTest(mathService),
                RsaService.PrimalityTest.Miller => new MillerRabinPrimalityTest(mathService),
                _ => throw new InvalidOperationException("unknown type")
            };
        }
        private bool CheckingValues(BigInteger x) 
            => _primalityTest.IsPrime(x, _probability) && _mathService.Gcd(x-1, PublicExponent, GcdMethod.Binary) == 1;
        
        private bool CheckingAttacks(BigInteger p, BigInteger q, BigInteger d, BigInteger n) 
            => IsFermatVulnerable(p,q) || IsWienerVulnerable(d,n);
        private BigInteger GeneratePrime()
        {
            while (true)
            {
                var candidate = GenerateRandom(_BL);
                if (CheckingValues(candidate)) return candidate;
            }
        }
        private BigInteger GenerateRandom(int BL)
        {
            var rez = new byte[(BL + BitPerByte - 1) / BitPerByte];
            Random.Shared.NextBytes(rez);
            int bitPos = (BL - 1) %  BitPerByte;
            rez[^1] = (byte)((rez[^1] & ((1 << (bitPos + 1)) - 1)) | (1 << bitPos));
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