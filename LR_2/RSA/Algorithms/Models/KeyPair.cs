using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace RSA.Algorithms.Models
{
    public struct KeyPair
    {
        public RsaPublicKey PublicKey {get;}
        public RsaPrivateKey PrivateKey {get;}
        public KeyPair(BigInteger e,BigInteger d, BigInteger p, BigInteger q, BigInteger n)
            => (PublicKey, PrivateKey) = (new RsaPublicKey (e,n), new RsaPrivateKey (d, n, p, q));
        public KeyPair(RsaPublicKey publicKey, RsaPrivateKey privateKey) 
            => (PublicKey, PrivateKey) = (publicKey, privateKey);
    }
    public struct RsaPublicKey
    {
        public BigInteger E { get; }
        public BigInteger N { get; }
        public RsaPublicKey(BigInteger e, BigInteger n) => (E, N) = (e, n);
    }
    public struct RsaPrivateKey
    {
        public BigInteger D { get; }
        public BigInteger N { get; }
        public BigInteger P { get; }
        public BigInteger Q { get; }
        public RsaPrivateKey(BigInteger d, BigInteger n, BigInteger p, BigInteger q) => (D, N, P, Q) = (d, n, p, q);
    }
}