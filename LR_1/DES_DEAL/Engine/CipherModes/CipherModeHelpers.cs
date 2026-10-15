using System;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Core.Utilities;

namespace DES_DEAL.Engine.CipherModes
{
    internal static class CipherModeHelpers
    {
        public static void ValideArgs(byte[] data, int blockSize)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (blockSize <= 0) throw new ArgumentOutOfRangeException(nameof(blockSize), "Block can't be empty");
            if (data.Length % blockSize !=0) throw new ArgumentException("Incorrect input data length", nameof(data));
        }
        public static void RequireIv(byte[] iv, int blockSize)
        {
            ArgumentNullException.ThrowIfNull(iv);
            if (iv.Length != blockSize) throw new ArgumentException("len (IV) = len (block)!", nameof(iv));
        }
        public static byte[] GetBlock(byte[] data, int blockIndex, int blockSize)
        {
            var block = new byte[blockSize];
            Array.Copy(data, blockIndex * blockSize, block, 0, blockSize);
            return block;
        }
        public static byte[] Xor(byte[] a, byte[] b){
            
            var res = new byte[a.Length];
            for (int i = 0; i<a.Length; ++i)
            {
                res[i] = (byte)(a[i]^b[i]);
            }
            return res;
        }
        public static void SetBlock(byte[] data, int blockIndex, int blockSize, byte[] block)
        {
            Array.Copy (block, 0, data, blockIndex * blockSize, blockSize);
        }
        public static byte[] LastBlock(byte[] data, int blockSize)
        {
            var block = new byte[blockSize];
            Array.Copy(data, data.Length - blockSize, block, 0, blockSize);
            return block;
        }
        public static byte[] Concat(byte[] a, byte[] b)
        {
            var res = new byte[a.Length + b.Length];
            Array.Copy(a, 0, res, 0, a.Length);
            Array.Copy(b, 0, res, a.Length, b.Length);
            return res;
        }
    }
}