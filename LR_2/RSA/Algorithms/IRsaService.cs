using System.Numerics;

namespace RSA.Algorithms
{
    public interface IRsaService
    {
        byte[] Encrypt(byte[] message);
        BigInteger Encrypt(BigInteger message);
        byte[] Decrypt(byte[] cipher);
        BigInteger Decrypt(BigInteger cipher);
        void RegenerateKeys();
    }
}