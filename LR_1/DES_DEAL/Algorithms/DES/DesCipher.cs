using System;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Core.Utilities;
using DES_DEAL.Engine;

namespace DES_DEAL.Algorithms.DES
{
    public sealed class DesCipher : ISymmetricCipher
    {
        private readonly FeistelNetwork _feistelNetwork;
        public int BlockSize => DesConstants.BlockSizeBytes;

        public DesCipher()
        {
            _feistelNetwork = new FeistelNetwork(new DesKeyExpansion(), new DesRoundFunction());
        }

        public void SetKey(byte[] key) => _feistelNetwork.SetKey(key);

        public byte[] Encrypt(byte[] block)
        {
            ValidateBlockLength(block);

            byte[] permuted = BitPermutator.Permute(
                block, DesConstants.InitialPermutation, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            byte[] processed = _feistelNetwork.Encrypt(permuted);

            return BitPermutator.Permute(
                processed, DesConstants.FinalPermutation, BitOrder.MsbToLsb, BitIndexBase.OneBased);
        }

        public byte[] Decrypt(byte[] block)
        {
            ValidateBlockLength(block);

            byte[] permuted = BitPermutator.Permute(
                block, DesConstants.InitialPermutation, BitOrder.MsbToLsb, BitIndexBase.OneBased);

            byte[] processed = _feistelNetwork.Decrypt(permuted);

            return BitPermutator.Permute(
                processed, DesConstants.FinalPermutation, BitOrder.MsbToLsb, BitIndexBase.OneBased);
        }

        private void ValidateBlockLength(byte[] block)
        {
            ArgumentNullException.ThrowIfNull(block);
            if (block.Length != BlockSize) throw new ArgumentException($"ken(block) = {BlockSize} byte", nameof(block));
        }
    }
}