using System;

namespace DES_DEAL.Engine.Padding
{

    public abstract class PaddingStrategyBase : IPaddingStrategy
    {
        private const int MAX_SIZE = 255;
        public byte[] Pad(byte[]? data, int blockSize)
        {
            ValidArgs(data, blockSize);

            int remainder = data!.Length % blockSize;
            int paddingLen = blockSize - remainder;
            var res = new byte[data.Length + paddingLen];
            Array.Copy(data, res, data.Length);

            var paddingBytes = new byte[paddingLen];
            FillPaddingBytes(paddingBytes);
            paddingBytes[paddingLen-1] = (byte)paddingLen;
            Array.Copy(paddingBytes, 0, res, data.Length, paddingLen);

            return res;
        }
        public byte[] Unpad(byte[]? data, int blockSize)
        {
            ValidArgs(data, blockSize);

            if (data!.Length == 0 || data.Length % blockSize != 0) throw new InvalidPaddingException("The padding is missing or the data is incorrect");

            int paddingLen = data[data.Length - 1];
            if (paddingLen < 1 || paddingLen > data.Length || paddingLen > blockSize) throw new InvalidPaddingException($"Incorrect padding length ({paddingLen})");

            int paddingStart = data.Length - paddingLen;
            ValidatePaddingBytes(data, paddingStart, paddingLen);

            var res = new byte[paddingStart];
            Array.Copy(data, res, paddingStart);
            return res;
        }
        protected abstract void FillPaddingBytes(byte[] data);
        protected virtual void ValidatePaddingBytes(byte[] data, int paddingStart, int paddingLen)
        {
        }
        private void ValidArgs(byte[]? data, int blockSize)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (blockSize <= 0 || blockSize > MAX_SIZE) throw new ArgumentOutOfRangeException(nameof(blockSize), "The block size must be positive");
        }
    }
}