using System.Numerics;
using System.Collections.Generic;

namespace RSA.Attacks.Wiener
{
    public readonly record struct AttackResult(BigInteger D, BigInteger Phi, IEnumerable<Fraction> Convergents);
    public readonly record struct Fraction(BigInteger K, BigInteger D);
}