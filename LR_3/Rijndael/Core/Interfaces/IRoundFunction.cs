namespace Rijndael.Core.Interfaces
{
    public interface IRoundFunction
    {
        byte[] Transform(byte[] block, byte[] roundKey);
    }
}