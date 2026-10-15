using System;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class PcbcMode : ICipherModeStrategy
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
                byte[] blockM = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] zn = CipherModeHelpers.Xor(blockM, prevBlock);
                byte[] blockC = cipher.Encrypt(zn);
                CipherModeHelpers.SetBlock(res, i, blockSize, blockC);
                prevBlock = CipherModeHelpers.Xor(blockM, blockC);
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
            byte[] prevBlock =iv;
            for(int i=0; i<blockCount;++i)
            {
                byte[] blockC = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] decrypted = cipher.Decrypt(blockC);
                byte[] zn = CipherModeHelpers.Xor(decrypted, prevBlock);
                CipherModeHelpers.SetBlock(res, i, blockSize, zn);
                prevBlock = CipherModeHelpers.Xor(zn, blockC);
            }
            return res;
        }
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => CipherModeHelpers.Xor(
            CipherModeHelpers.LastBlock(inputChunk, cipher.BlockSize),
            CipherModeHelpers.LastBlock(outputChunk, cipher.BlockSize));
    }
}
