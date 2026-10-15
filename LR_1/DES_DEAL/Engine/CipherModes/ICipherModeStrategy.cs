using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Engine.CipherModes
{
    public interface ICipherModeStrategy
    {
        byte[] Encrypt(byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long blockIndexOffset = 0);
        byte[] Decrypt (byte[] data, ISymmetricCipher cipher, byte[] iv, object[] extraParams, long vlockIndexOffset = 0);
        bool IsCounterBased { get; }
        byte[] DeriveNextChainValue(byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ISymmetricCipher cipher);
    }
}
