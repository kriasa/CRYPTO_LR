namespace Rijndael.Algorithms.Transformations
{
    public interface IRijndaelTransform
    {
        void SubBytes(byte[] data);
        void ShiftRows(byte[] data);
        void AddRoundKey(byte[] data, byte[] RK);
        void MixColumns(byte[] data);
        void InvSubBytes(byte[] data);
        void InvShiftRows(byte[] data);
        void InvMixColumns(byte[] data);
    }
}