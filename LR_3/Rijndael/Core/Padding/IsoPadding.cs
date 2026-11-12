using System.Security.Cryptography;

namespace Rijndael.Core.Padding
{
    public sealed class IsoPadding : PaddingStrategyBase
    {
        protected override void FillPaddingBytes(byte[] data)
        {
            RandomNumberGenerator.Fill(data);
        }
    }
}