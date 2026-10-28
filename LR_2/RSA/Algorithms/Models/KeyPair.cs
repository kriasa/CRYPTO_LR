namespace RSA.Algorithms.Models
{
    public struct KeyPair
    {
        public BigInteger E { get; }
        public BigInteger D { get; }
        public BigInteger P { get; }
        public BigInteger Q { get; }
        public BigInteger N { get; }
        public KeyPair(BigInteger e, BigInteger d, BigInteger p, BigInteger q, BigInteger n)
        {
            E = e; D = d; P = p; Q = q; N = n;
        }
    };
}