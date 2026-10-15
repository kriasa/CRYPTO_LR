using System;
using System.Numerics;

namespace DES_DEAL.Engine.CipherModes
{
    internal static class BlockCounterMath
    {
        private const int BITS_PER_BYTE = 8;
        public static byte[] Increment(byte[] counter, BigInteger delta)
        {
            int len  = counter.Length;
            BigInteger modules = BigInteger.One << (len * BITS_PER_BYTE);
            BigInteger val = new BigInteger(counter, isUnsigned: true, isBigEndian: true);
            BigInteger res = (val + delta) % modules;
            return ToFixedLengthBigEndian(res, len);
        }
        public static byte[] Increment(byte[] counter, long delta)=>Increment(counter, (BigInteger)delta);
        private static byte[] ToFixedLengthBigEndian(BigInteger val, int len)
        {
            byte[] raw = val.ToByteArray(isUnsigned: true, isBigEndian: true);
            if (raw.Length == len) return raw;
            var res = new byte[len];
            Array.Copy(raw, 0, res, len - raw.Length, raw.Length);
            return res;
        }
    }
}