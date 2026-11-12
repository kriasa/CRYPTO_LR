using System;
using Rijndael.Core.Modes;
using Rijndael.Core.Padding;

namespace Rijndael.Core
{
    internal static class StrategyFactory
    {
        public static ICipherModeStrategy CreateCipherMode(CipherModeType cipherMode) => cipherMode switch
        {
            CipherModeType.ECB => new EcbMode(),
            CipherModeType.CBC => new CbcMode(),
            CipherModeType.PCBC => new PcbcMode(),
            CipherModeType.CFB => new CfbMode(),
            CipherModeType.OFB => new OfbMode(),
            CipherModeType.CTR => new CtrMode(),
            CipherModeType.RandomDelta => new RandomDeltaMode(),
            _ => throw new ArgumentOutOfRangeException(nameof(cipherMode), cipherMode, "Unknown encryption mode")
        };

        public static IPaddingStrategy CreatePadding(PaddingModeType paddingMode) => paddingMode switch
        {
            PaddingModeType.Zeros => new ZerosPadding(),
            PaddingModeType.AnsiX => new AnsiXPadding(),
            PaddingModeType.Pkcs => new PkcsPadding(),
            PaddingModeType.Iso => new IsoPadding(),
            _ => throw new ArgumentOutOfRangeException(nameof(paddingMode), paddingMode, "Unknown padding mode")
        };
        public static bool RequiresIv(CipherModeType cipherMode) => cipherMode != CipherModeType.ECB;
    }
}