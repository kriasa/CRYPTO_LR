using System;
using System.Collections.Generic;
using Rijndael.Algorithms.Substitution;
using Rijndael.MathService;
using Xunit;

namespace Rijndael.Tests.Algorithms.Substitution;

public sealed class SBoxProviderTests
{
    private static readonly byte[] IrreducibleModules =
    {
        0x1B, // AES: x^8 + x^4 + x^3 + x + 1
        0x1D, // x^8 + x^4 + x^3 + x^2 + 1
        0x2B,
        0x2D,
        0x39
    };

    private readonly IMathGFService _mathService = new MathGFService();

    private SBoxProvider CreateProvider(byte module)
        => new(module, _mathService);

    [Theory]
    [InlineData(0x00, 0x63)]
    [InlineData(0x01, 0x7C)]
    [InlineData(0x53, 0xED)]
    [InlineData(0x7C, 0x10)]
    [InlineData(0xFF, 0x16)]
    public void Sub_AESModule_ReturnsExpectedValue(
        byte input, byte expected)
    {
        var provider = CreateProvider(0x1B);

        Assert.Equal(expected, provider.Sub(input));
    }

    [Theory]
    [InlineData(0x00, 0x52)]
    [InlineData(0x63, 0x00)]
    [InlineData(0xED, 0x53)]
    [InlineData(0x10, 0x7C)]
    [InlineData(0x16, 0xFF)]
    public void InvSub_AESModule_ReturnsExpectedValue(
        byte input, byte expected)
    {
        var provider = CreateProvider(0x1B);

        Assert.Equal(expected, provider.InvSub(input));
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void Sub_AnyIrreducibleModule_IsPermutation(byte module)
    {
        var provider = CreateProvider(module);
        var results = new HashSet<byte>();

        for (int i = 0; i < 256; i++)
        {
            results.Add(provider.Sub((byte)i));
        }

        Assert.Equal(256, results.Count);
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void SubAndInvSub_AreMutuallyInverse(byte module)
    {
        var provider = CreateProvider(module);

        for (int i = 0; i < 256; i++)
        {
            byte value = (byte)i;

            Assert.Equal(value, provider.InvSub(provider.Sub(value)));
            Assert.Equal(value, provider.Sub(provider.InvSub(value)));
        }
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void InvSub_ProducesPermutation(byte module)
    {
        var provider = CreateProvider(module);
        var results = new HashSet<byte>();

        for (int i = 0; i < 256; i++)
        {
            results.Add(provider.InvSub((byte)i));
        }

        Assert.Equal(256, results.Count);
    }

    public static IEnumerable<object[]> GetModules()
    {
        foreach (byte module in IrreducibleModules)
        {
            yield return new object[] { module };
        }
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x01)]
    [InlineData(0x05)]
    [InlineData(0xFF)]
    public void Constructor_ReducibleModule_Throws(byte module)
    {
        Assert.Throws<ArgumentException>(
            () => CreateProvider(module));
    }

    [Fact]
    public void Constructor_NullMathService_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new SBoxProvider(0x1B, null!));
    }

    [Fact]
    public void Constructor_WithoutMathService_CreatesProvider()
    {
        var provider = new SBoxProvider(0x1B);

        Assert.Equal(0x63, provider.Sub(0x00));
        Assert.Equal(0x52, provider.InvSub(0x00));
    }
}