using System;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class CtrMode : ICipherModeStrategy
    {
        public bool IsCounterBased => true;
        public byte[] Encrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv, blockIndexOffset);
        public byte[] Decrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv, blockIndexOffset);
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => throw new NotSupportedException("don't used in СTR");
        private static byte[] Process(byte[] data, ISymmetricCipher cipher, byte[] iv, long blockIndexOffset)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);
            CipherModeHelpers.RequireIv(iv,cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            int blockCount = data.Length / blockSize;
            var res = new byte[data.Length];

            Parallel.For(0, blockCount, i =>
            {
                byte[] count = BlockCounterMath.Increment(iv, blockIndexOffset + i);
                byte[] block = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] zn =cipher.Encrypt(count);
                byte[] transformed = CipherModeHelpers.Xor(zn, block);
                CipherModeHelpers.SetBlock(res, i, blockSize, transformed);
            });

            return res;
        }
    }
}
