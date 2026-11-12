using System;

namespace Rijndael.Core.Padding
{
    public interface IPaddingStrategy
    {
        byte[] Pad(byte[]? data, int blockSize);
        byte[] Unpad(byte[]? data, int blockSize);
    }
}