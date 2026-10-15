using System;

namespace DES_DEAL.Engine.Padding
{
    public sealed class ZerosPadding : IPaddingStrategy
    {
        public byte[] Pad(byte[]? data, int blockSize)
        {
            ValidArgs(data, blockSize);

            int remainder = data!.Length % blockSize;
            int paddingLen = remainder == 0 ? 0 :blockSize - remainder; 
            var res = new byte[data.Length + paddingLen];
            Array.Copy(data, res, data.Length);
            return res;
        }
        public byte[] Unpad(byte[]? data, int blockSize)
        {
            ValidArgs(data, blockSize);
            
            int maxPad = blockSize - 1;
            int end = data!.Length;
            int tmp = 0;
            while (end > 0 && tmp < maxPad && data[end-1] == 0)
            {
                end --;
                tmp++;
            }
            var res = new byte [end];
            Array.Copy(data, res, end);
            return res;
        }
        private void ValidArgs(byte[]? data, int blockSize)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (blockSize <= 0) throw new ArgumentOutOfRangeException(nameof(blockSize), "The block size must be positive");
        }
    }
}