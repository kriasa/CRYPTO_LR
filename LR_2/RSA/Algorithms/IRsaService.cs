using System.Numerics;
using RSA.Algorithms.Models;

namespace RSA.Algorithms
{
    public interface IRsaService
    {
        public RsaPublicKey PublicKey {get;}
        byte[] Encrypt(byte[] message);
        BigInteger Encrypt(BigInteger message);
        byte[] Decrypt(byte[] cipher);
        BigInteger Decrypt(BigInteger cipher);
        void RegenerateKeys();
    }
}