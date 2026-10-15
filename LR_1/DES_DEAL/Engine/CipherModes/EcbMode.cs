using System;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class EcbMode : ICipherModeStrategy
    {
        public bool IsCounterBased => false;
        public byte [] Encrypt(byte[] data, ISymmetricCipher cipher,byte[] iv, object[] extraParams, long blockIndexOffset = 0) => ProcessBlocks(data, cipher, cipher.Encrypt);
        public byte[] Decrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => ProcessBlocks(data, cipher, cipher.Decrypt);
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => throw new NotSupportedException("ECB does not use IV");
        private static byte[] ProcessBlocks(byte[] data, ISymmetricCipher cipher, Func<byte[], byte[]> F)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            int blockCount = data.Length / blockSize;
            var res = new byte[data.Length];

            Parallel.For(0, blockCount, i =>
            {
                byte[] block = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] transformed = F(block);
                CipherModeHelpers.SetBlock(res, i, blockSize, transformed);
            });

            return res;
        }
    }
}
