using RSA.Algorithms.Models;

namespace RSA.Algorithms.KeyGenerator
{
    public interface IRsaKeyGenerator
    {
        KeyPair GenerateKey();
    }
}