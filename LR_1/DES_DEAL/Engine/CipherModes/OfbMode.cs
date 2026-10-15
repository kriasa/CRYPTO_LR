using System;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class OfbMode : ICipherModeStrategy
    {
        public bool IsCounterBased => false;
        public byte[] Encrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv);
        public byte[] Decrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv);
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => CipherModeHelpers.Xor(
            CipherModeHelpers.LastBlock(inputChunk, cipher.BlockSize),
            CipherModeHelpers.LastBlock(outputChunk, cipher.BlockSize));
        private static byte[] Process(byte[] data, ISymmetricCipher cipher, byte[] iv)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);
            CipherModeHelpers.RequireIv(iv, cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            int blockCount = data.Length / blockSize;

            var crypt = new byte[blockCount][];
            byte[] prevBlock =iv;

            for(int i = 0; i < blockCount; ++i)
            {
                prevBlock = cipher.Encrypt(prevBlock);
                crypt[i] = prevBlock;
            }

            var res = new byte[data.Length];
            Parallel.For(0, blockCount, i =>
            {
                byte[] block = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] transformed = CipherModeHelpers.Xor(block, crypt[i]);
                CipherModeHelpers.SetBlock(res, i, blockSize, transformed);
            }
            );

            return res;
        }
    }
}
