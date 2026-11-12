namespace Rijndael.Core.Interfaces
{
    public interface IRoundFunction
    {
        void EncryptRound(byte[] block, byte[] roundKey);
        void DecryptRound(byte[] block, byte[] roundKey);
    }
}