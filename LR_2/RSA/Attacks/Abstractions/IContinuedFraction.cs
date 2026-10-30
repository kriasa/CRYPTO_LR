using RSA.Attacks.Wiener;
using System.Numerics;
using RSA.Algorithms.Models;

namespace RSA.Attacks.Abstractions
{
    public interface IContinuedFraction
    {
        IEnumerable<Fraction> GetConvergents(RsaPublicKey publicKey);
    }
}