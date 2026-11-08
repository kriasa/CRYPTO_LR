using System.Numerics;

namespace Rijndael.MathService
{
    public interface IMathGFService
    {
        byte Add(byte a, byte b);
        byte Multiply(byte a, byte b, byte? module);
        byte GetInverse(byte a, byte? module);
        bool IsIrreducible(byte polynomial);
        IReadOnlyList<byte> IrreducibilityPolynomials8();
        IReadOnlyList<BigInteger> FactorizePolynomials(BigInteger polynomial, int n);
    }
}