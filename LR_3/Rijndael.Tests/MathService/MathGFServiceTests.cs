using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Rijndael.MathService;
using Rijndael.MathService.GaloisField;
using Rijndael.Tests.Helpers;
using Xunit;

namespace Rijndael.Tests.MathService
{
    public class MathGFServiceTests
    {
        private readonly IMathGFService _service = new MathGFService();

        [Fact]
        public void Add_Works()
        {
            Assert.Equal(0xD4, _service.Add(0x57, 0x83));
        }

        [Fact]
        public void Multiply_NullModule_UsesAesDefault()
        {
            Assert.Equal(0xC1, _service.Multiply(0x57, 0x83, null));
        }

        [Fact]
        public void Multiply_ExplicitIrreducibleModule_IsUsed()
        {
            Assert.Equal(0x1D, _service.Multiply(0x80, 0x02, 0x1D));
            Assert.Equal(0x31, _service.Multiply(0x57, 0x83, 0x1D));
        }

        [Fact]
        public void GetInverse_NullModule_UsesAesDefault()
        {
            Assert.Equal(0xCA, _service.GetInverse(0x53, null));
        }

        [Fact]
        public void GetInverse_ExplicitIrreducibleModule_IsUsed()
        {
            Assert.Equal(0x8C, _service.GetInverse(0x53, 0x1D));
        }

        [Fact]
        public void AllThirtyIrreducibleModules_AreAccepted()
        {
            foreach (byte m in Gf2Poly.Irreducible8)
            {
                byte inv = _service.GetInverse(0x57, m);
                Assert.Equal(1, _service.Multiply(0x57, inv, m));
            }
        }

        [Theory]
        [InlineData(0x00)] // x^8
        [InlineData(0x01)] // x^8 + 1
        [InlineData(0x05)]
        [InlineData(0x1A)] // нет свободного члена
        [InlineData(0x1F)] // чётное число членов с учётом x^8
        [InlineData(0x41)]
        [InlineData(0x55)]
        [InlineData(0xBB)]
        [InlineData(0xFF)]
        public void Multiply_ReducibleModule_Throws(byte module)
        {
            Assert.Throws<ArgumentException>(
                () => _service.Multiply(0x57, 0x83, module));
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x01)]
        [InlineData(0x05)]
        [InlineData(0xFF)]
        public void GetInverse_ReducibleModule_Throws(byte module)
        {
            Assert.Throws<ArgumentException>(
                () => _service.GetInverse(0x57, module));
        }

        [Fact]
        public void ReducibleModule_Throws_EvenForTrivialOperands()
        {
            Assert.Throws<ArgumentException>(
                () => _service.Multiply(0x00, 0x00, 0x01));

            Assert.Throws<ArgumentException>(
                () => _service.Multiply(0x01, 0x01, 0x01));

            Assert.Throws<ArgumentException>(
                () => _service.GetInverse(0x00, 0x01));
        }

        [Fact]
        public void ReducibleModule_Exception_HasModuleAsParamName()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => _service.Multiply(1, 1, 0x01));

            Assert.Equal("module", ex.ParamName);
        }

        [Fact]
        public void Factorize_NIsLessThanOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _service.FactorizePolynomials(7, 0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _service.FactorizePolynomials(7, -1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Factorize_PolynomialIsConstantOrNegative_Throws(int polynomial)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _service.FactorizePolynomials(polynomial, 1));
        }

        [Fact]
        public void Factorize_SmallestAllowedPolynomial_X()
        {
            Assert.Equal(
                new BigInteger[] { 2 },
                _service.FactorizePolynomials(2, 1));
        }

        [Fact]
        public void Factorize_DelegatesToPolynomialService()
        {
            Assert.Equal(
                new BigInteger[] { 0x07, 0x49 },
                _service.FactorizePolynomials(0x1FF, 8));
        }

        [Fact]
        public void IsIrreducibility_And_IrreducibilityPolynomials8_Delegate()
        {
            Assert.True(_service.IsIrreducible(0x1B));
            Assert.False(_service.IsIrreducible(0x01));
            Assert.Equal(30, _service.IrreducibilityPolynomials8().Count);
        }

        private sealed class SpyGf : IGFArithmeticService
        {
            public int Calls;

            public byte Add(byte a, byte b)
            {
                Calls++;
                return 0;
            }

            public byte Multiply(byte a, byte b, byte module)
            {
                Calls++;
                return 0;
            }

            public byte Multiply(byte a, byte b)
            {
                Calls++;
                return 0;
            }

            public byte GetInverse(byte a, byte module)
            {
                Calls++;
                return 0;
            }

            public byte GetInverse(byte a)
            {
                Calls++;
                return 0;
            }
        }

        private sealed class StubPolys : IPolynomialsArithmeticService
        {
            private readonly bool _answer;

            public StubPolys(bool answer)
            {
                _answer = answer;
            }

            public bool IsIrreducible(byte polynomial) => _answer;

            public IReadOnlyList<byte> IrreducibilityPolynomials8()
                => Array.Empty<byte>();

            public IReadOnlyList<BigInteger> FactorizePolynomials(BigInteger polynomial)
                => Array.Empty<BigInteger>();
        }

        [Fact]
        public void Multiply_RejectedModule_NeverReachesArithmeticService()
        {
            var spy = new SpyGf();
            var service = new MathGFService(spy, new StubPolys(false));

            Assert.Throws<ArgumentException>(
                () => service.Multiply(1, 1, 0x1B));

            Assert.Throws<ArgumentException>(
                () => service.GetInverse(1, 0x1B));

            Assert.Equal(0, spy.Calls);
        }

        [Fact]
        public void Multiply_AcceptedModule_IsDelegated()
        {
            var spy = new SpyGf();
            var service = new MathGFService(spy, new StubPolys(true));

            service.Multiply(1, 1, 0x01);
            service.GetInverse(1, 0x01);

            Assert.Equal(2, spy.Calls);
        }

        [Fact]
        public void NullModule_SkipsValidation_AndUsesDefaultOverloads()
        {
            var spy = new SpyGf();
            var service = new MathGFService(spy, new StubPolys(false));

            service.Multiply(1, 1, null);
            service.GetInverse(1, null);

            Assert.Equal(2, spy.Calls);
        }
    }
}