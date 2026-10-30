using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using RSA.Algorithms.KeyGenerator;
using RSA.MathService.Abstractions;
using RSA.Algorithms.Models;
using RSA.MathService;

namespace RSA.Algorithms
{
    public partial class RsaService : IRsaService
    {
        private KeyPair _currentKeys;
        public RsaPublicKey PublicKey => _currentKeys.PublicKey;
        private readonly IRsaKeyGenerator _keyGenerator;
        private readonly IMathTheoryService _mathService;
        private const int minBitLen = 128;
        public enum PrimalityTest { Fermat, Miller, Solovay };
        public RsaService(PrimalityTest test, double probability, int BL = 1024) : this(test, probability,new MathTheoryService(), BL)
        {
        }
        public RsaService(PrimalityTest test, double probability, IMathTheoryService mathTheoryService, int BL = 1024)
        {
            if (probability < 0.5 || probability >= 1) throw new ArgumentOutOfRangeException(nameof(probability), "0.5<=probability<1");
            if (BL < minBitLen) throw new ArgumentOutOfRangeException(nameof(BL), "BL(p) BL(q) > 128");
            _keyGenerator = new RsaKeyGenerator(test, probability, BL, mathTheoryService);
            _mathService = mathTheoryService;
            RegenerateKeys();
        }
        public BigInteger Encrypt(BigInteger message)
        {
            if (message < 0 || message >= PublicKey.N) throw new ArgumentException("incorrect data size for encryption", nameof(message));

            return _mathService.ModPow(message, PublicKey.E, PublicKey.N);
        }
        public byte[] Encrypt(byte[] message){
            var messageNew = new BigInteger(message, isUnsigned:true);
            var cipher = Encrypt(messageNew);
            var rez = new byte[PublicKey.N.GetByteCount()];
            cipher.TryWriteBytes(rez, out _, isUnsigned: true);
            return rez;
        }
        public BigInteger Decrypt(BigInteger cipher)
        {
            if (cipher < 0 || cipher >= PublicKey.N) throw new ArgumentException(nameof(cipher));

            return _mathService.ModPow(cipher, _currentKeys.PrivateKey.D, _currentKeys.PrivateKey.N);
        }
        public byte[] Decrypt(byte[] cipher) => Decrypt(new BigInteger(cipher, isUnsigned:true)).ToByteArray(isUnsigned: true);
        public void RegenerateKeys()
        {
            _currentKeys = _keyGenerator.GenerateKey();
        }
    }
}