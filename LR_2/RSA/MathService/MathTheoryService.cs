using RSA.MathService.Abstractions;
using RSA.MathService.Enums;
using RSA.MathService.Gcd;
using RSA.MathService.Modular;
using RSA.MathService.Symbols;
using RSA.MathService.Totient;
using System.Numerics;

namespace RSA.MathService
{
    public sealed class MathTheoryService : IMathTheoryService
    {
        private readonly IJacobiSymbolCalculator _jacobi;
        private readonly ICarmichaelCalculator _lambda;
        private readonly IEulerTotientCalculator _phiFactorization;
        private readonly IEulerTotientCalculator _phiDefinition;
        private readonly IEulerTotientCalculator _phiDft;
        private readonly IExtendedGcdCalculator _extendedGcd;
        private readonly IGcdCalculator _gcdBinary;
        private readonly IGcdCalculator _gcd;
        private readonly ILegendreSymbolCalculator _legendre;
        private readonly IModularExponentiation _modExp;
        private readonly IFactorizer _factorizer;
        public MathTheoryService()
        {
            _modExp = new ModularExponentiation();
            _gcd = new EuclideanGcd();
            _factorizer = new TrialDivisionFactorizer();
            _gcdBinary = new BinaryGcd();

            _extendedGcd = new ExtendedEuclideanGcd();
            _phiDefinition = new DefinitionEulerTotient(_gcdBinary);
            _phiFactorization = new FactorizationEulerTotient(_factorizer);
            _phiDft = new FourierEulerTotient(_gcdBinary);
            _legendre = new LegendreSymbolCalculator(_modExp);
            _jacobi = new JacobiSymbolCalculator();
            _lambda = new CarmichaelCalculator(_factorizer, _gcdBinary);
        }
        public int JacobiSymbol(BigInteger a, BigInteger n)=>_jacobi.JacobiSymbol(a,n);
        public int LegendreSymbol(BigInteger a, BigInteger p)=>_legendre.LegendreSymbol(a,p);
        public (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)=>_extendedGcd.ExtendedGcd(a,b);
        public BigInteger ModularPow(BigInteger a, BigInteger pow, BigInteger mod)=>_modExp.ModPow(a, pow, mod);
        public BigInteger Lambda(BigInteger n)=> _lambda.Lambda(n);
        public BigInteger Gcd(BigInteger a, BigInteger b, GcdMethod method)
        {
            return method switch
            {
                GcdMethod.Classical => _gcd.Gcd(a,b),
                GcdMethod.Binary => _gcdBinary.Gcd(a, b),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };
        }
        public BigInteger Phi(BigInteger n, EulerMethod method)
        {
            return method switch
            {
                EulerMethod.Definition => _phiDefinition.Phi(n),
                EulerMethod.Factorization => _phiFactorization.Phi(n),
                EulerMethod.Dft => _phiDft.Phi(n),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };
        }
    }
}