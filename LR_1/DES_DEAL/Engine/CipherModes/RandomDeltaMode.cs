using System;
using System.Numerics;
using System.Threading.Tasks;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public sealed class RandomDeltaMode : ICipherModeStrategy
    {
        private const int BITS_PER_BYTE = 8;
        public bool IsCounterBased => true;
        public byte[] Encrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv, blockIndexOffset);
        public byte[] Decrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0) => Process(data, cipher, iv, blockIndexOffset);
        public byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher) => throw new NotSupportedException("don't used in RD");
        private static byte[] Process(byte[] data, ISymmetricCipher cipher, byte[] iv, long blockIndexOffset)
        {
            CipherModeHelpers.ValideArgs(data, cipher.BlockSize);
            CipherModeHelpers.RequireIv(iv, cipher.BlockSize);

            int blockSize = cipher.BlockSize;
            if (blockSize % 2 != 0) throw new ArgumentException("RD requires even block size", nameof(blockSize));
            int halfSize = blockSize / 2;

            var counterIV = new byte[halfSize];
            var delta = new byte[halfSize];
            Array.Copy(iv, 0, counterIV, 0, halfSize);
            Array.Copy(iv, halfSize, delta, 0, halfSize);

            BigInteger deltaVal = new BigInteger(delta, isUnsigned: true, isBigEndian: true);
            BigInteger modulus = BigInteger.One << (halfSize * BITS_PER_BYTE);
            if (BigInteger.GreatestCommonDivisor(deltaVal, modulus) != BigInteger.One) throw new ArgumentException("gcd(delta,2^{8*halfSize}) = 1");

            int blockCount = data.Length / blockSize;
            var res = new byte[data.Length];

            Parallel.For(0, blockCount, i =>
            {
                byte[] countI = BlockCounterMath.Increment(counterIV, deltaVal*(blockIndexOffset + i));
                byte[] tag = CipherModeHelpers.Concat(countI, delta);
                byte[] block = CipherModeHelpers.GetBlock(data, i, blockSize);
                byte[] zn = cipher.Encrypt(tag);
                byte[] transformed = CipherModeHelpers.Xor(zn, block);
                CipherModeHelpers.SetBlock(res, i, blockSize, transformed);
            });

            return res;
        }
    }
}
