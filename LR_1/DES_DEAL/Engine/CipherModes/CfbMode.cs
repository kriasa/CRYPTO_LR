using System;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class CfbMode : ICipherModeStrategy
    {
        public bool IsCounterBased => false;
        public byte[] Encrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);
            CipherModeHelpers.RequireIv(iv, cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            int blockCount = data.Length / blockSize;
            var res = new byte[data.Length];
            byte[] prevBlock = iv;

            for (int i = 0; i < blockCount; ++i)
            {
                byte[] crypt = cipher.Encrypt(prevBlock);
                byte[] blockM = CipherModeHelpers.GetBlock(data,i,blockSize);
                byte[] zn =CipherModeHelpers.Xor(blockM, crypt);
                CipherModeHelpers.SetBlock(res, i, blockSize, zn);
                prevBlock = zn;
            }
            return res;
        }
        public byte[] Decrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);
            CipherModeHelpers.RequireIv(iv, cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            int blockCount = data.Length / blockSize;
            var res = new byte[data.Length];

            Parallel.For(0, blockCount, i =>
            {
                byte[] blockC = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] prevBlock = i == 0 ? iv : CipherModeHelpers.GetBlock(data, i - 1, blockSize);
                byte[] crypt = cipher.Encrypt(prevBlock);
                byte[] zn = CipherModeHelpers.Xor(crypt, blockC);
                CipherModeHelpers.SetBlock(res, i, blockSize, zn);
            });
            return res;
        }
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => isEncrypting
           ? CipherModeHelpers.LastBlock(outputChunk, cipher.BlockSize)
           : CipherModeHelpers.LastBlock(inputChunk, cipher.BlockSize);
    }
}
