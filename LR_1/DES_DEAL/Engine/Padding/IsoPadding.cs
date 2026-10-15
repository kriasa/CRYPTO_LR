using System.Security.Cryptography;

namespace DES_DEAL.Engine.Padding
{
    public sealed class IsoPadding : PaddingStrategyBase
    {
        protected override void FillPaddingBytes(byte[] data)
        {
            RandomNumberGenerator.Fill(data);
        }
    }
}