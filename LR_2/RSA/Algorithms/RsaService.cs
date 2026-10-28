using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using RSA.Algorithms.KeyGenerator;
using RSA.MathService.Abstractions;
using RSA.Algorithms.Models;

namespace RSA.Algorithms
{
    public sealed class RsaService : IRsaService
    {
        private KeyPair _currentKeys;
        private readonly IRsaKeyGenerator _keyGenerator;
        private readonly IMathService _mathService;
        public enum PrimalityTest { Fermat, Miller, Solovay };
        public RsaService(PrimalityTest test, double probability, int BL = 1024)
        {
            if (probability < 0.5 || probability >= 1) throw new ArgumentException(nameof(probability));
            _keyGenerator = new RsaKeyGenerator(test, probability, BL);
            _mathService = new MathService();
            RegenerateKeys();
        }
        public BigInteger Encrypt(BigInteger message)
        {
            if (message < 0 || message >= _currentKeys.N) throw new ArgumentException(nameof(message), "incorrect data size for encryption");

            return _mathService.ModPow(message, _currentKeys.E, _currentKeys.N);
        }
        public byte[] Encrypt(byte[] message) => Encrypt(new BigInteger(message, isUnsigned:true)).ToByteArray(isUnsigned: true);
        public BigInteger Decrypt(BigInteger cipher)
        {
            if (cipher < 0 || cipher >= _currentKeys.N) throw new ArgumentException(nameof(cipher));
            return _mathService.ModPow(cipher, _currentKeys.D, _currentKeys.N);
        }
        public byte[] Decrypt(byte[] cipher) => Decrypt(new BigInteger(cipher, isUnsigned:true)).ToByteArray(isUnsigned: true);
        public void RegenerateKeys()
        {
            _currentKeys = _keyGenerator.GenerateKey();
        }
    }
}