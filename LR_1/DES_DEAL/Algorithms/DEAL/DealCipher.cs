using DES_DEAL.Algorithms.DES;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine;

namespace DES_DEAL.Algorithms.DEAL
{
    public sealed class DealCipher : ISymmetricCipher
    {
        private readonly FeistelNetwork _network;
        public int BlockSize => DealConstants.BlockSizeBytes;
        public DealCipher()
        {
            _network = new FeistelNetwork(new DealKeyExpansion(), new DesToRoundFunctionAdapt());
        }
        public void SetKey(byte[] key) => _network.SetKey(key);
        public byte[] Encrypt(byte[] block) => _network.Encrypt(block);
        public byte[] Decrypt(byte[] block) => _network.Decrypt(block);
    }
}