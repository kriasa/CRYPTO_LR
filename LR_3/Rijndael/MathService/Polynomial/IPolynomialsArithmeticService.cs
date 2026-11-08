using System.Numerics;

namespace Rijndael.MathService
{
    public interface IPolynomialsArithmeticService
    {
        bool IsIrreducible(byte polynomial);
        IReadOnlyList<byte> IrreducibilityPolynomials8();
        IReadOnlyList<BigInteger> FactorizePolynomials(BigInteger polynomial);
    }
}