using System;

namespace DES_DEAL.Engine.Padding
{
    public sealed class AnsiXPadding : PaddingStrategyBase
    {
        protected override void FillPaddingBytes(byte[] data)
        {
            Array.Clear(data, 0, data.Length);
        }
        protected override void ValidatePaddingBytes(byte[] data, int paddingStart, int paddingLen)
        {
            for (int i = paddingStart; i < paddingStart + paddingLen - 1; ++i)
            {
                if (data[i] != 0) throw new InvalidPaddingException("Padding ANSI incorrect");
            }
        }
    }
}