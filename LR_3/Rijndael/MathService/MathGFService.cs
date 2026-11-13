using Rijndael.MathService.Polynomial;
using Rijndael.MathService.GaloisField;
using System.Numerics;

namespace Rijndael.MathService
{
    public sealed class MathGFService : IMathGFService
    {
        private readonly IGFArithmeticService _gfService;
        private readonly IPolynomialsArithmeticService _polinomialsService;
        public MathGFService()
            : this(new GFArithmeticService(), new PolynomialsArithmeticService())
        {
        }
        public MathGFService(IGFArithmeticService gfService, IPolynomialsArithmeticService polynomialsService)
        {
            _gfService = gfService;
            _polinomialsService = polynomialsService;
        }
        public byte Add(byte a, byte b) => _gfService.Add(a, b);
        public byte Multiply(byte a, byte b, byte? module)
        {
            // ValidateModule(module);
            return module.HasValue ? _gfService.Multiply(a, b, module.Value) : _gfService.Multiply(a, b);
        }
        public byte GetInverse(byte a, byte? module)
        {
            // ValidateModule(module);
            return module.HasValue ? _gfService.GetInverse(a, module.Value) : _gfService.GetInverse(a);
        }
        public bool IsIrreducible(byte polynomial) => _polinomialsService.IsIrreducible(polynomial);
        public IReadOnlyList<byte> IrreducibilityPolynomials8() => _polinomialsService.IrreducibilityPolynomials8();
        public IReadOnlyList<BigInteger> FactorizePolynomials(BigInteger polynomial,int n)
        {
            if (n < 1) throw new ArgumentOutOfRangeException(nameof(n), "n >= 1");
            if (polynomial < 2) throw new ArgumentOutOfRangeException("degree of a polynomial >=1 ",nameof(polynomial));

            return _polinomialsService.FactorizePolynomials(polynomial);
        }
        private void ValidateModule(byte? module)
        {
            if (module.HasValue && !IsIrreducible(module.Value)) throw new ArgumentException("module  isn't irreducibility",nameof(module));
        }
    }
}