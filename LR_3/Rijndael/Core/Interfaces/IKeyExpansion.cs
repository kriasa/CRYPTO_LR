namespace Rijndael.Core.Interfaces
{
    public interface IKeyExpansion
    {
        byte[][] GenerateRoundKeys(byte[] key);
    }
}