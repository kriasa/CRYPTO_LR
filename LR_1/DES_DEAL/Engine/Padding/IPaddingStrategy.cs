using System;

namespace DES_DEAL.Engine.Padding
{
    public interface IPaddingStrategy
    {
        byte[] Pad(byte[]? data, int blockSize);
        byte[] Unpad(byte[]? data, int blockSize);
    }
}