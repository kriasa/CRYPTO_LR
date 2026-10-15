namespace DES_DEAL.Core.Interfaces
{
    public interface IKeyExpansion
    {
        byte[][] GenerateRoundKeys(byte[] key);
    }
}