using System;
using DES_DEAL.Algorithms.DES;
using DES_DEAL.Algorithms.DEAL;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine.CipherModes;

namespace DES_DEAL.Algorithms.DEAL
{
    public sealed class DealKeyExpansion : IKeyExpansion
    {
        public byte[][] GenerateRoundKeys(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);

            int partsCount = DealConstants.GetKeyPartsCount(key.Length);
            int roundCount = DealConstants.GetRoundCount(key.Length);

            byte[][] keyParts = SplitIntoParts(key, partsCount);

            var roundKeys = new byte [roundCount][];
            var desFunc = new DesCipher();
            desFunc.SetKey(DealConstants.ScheduleConstantKey);
            byte[] prev = new byte[DealConstants.DesBlockSizeBytes];

            for (int round = 0; round < roundCount; ++round)
            {
                byte[] keyPart = (byte[])keyParts[round % partsCount].Clone();

                if (round >= partsCount)
                {
                    int shift = round - partsCount;
                    byte roundConst = (byte)(1<<shift);
                    keyPart[^1] ^= roundConst;
                }
                byte[] desInput = (round==0) ? keyPart : CipherModeHelpers.Xor(keyPart, prev);

                byte[] roundKey = desFunc.Encrypt(desInput);
                roundKeys[round] = roundKey;
                prev = roundKey;
            }
            return roundKeys;
        }
        private static byte[][] SplitIntoParts(byte[] key, int partsCount)
        {
            var parts = new byte[partsCount][];
            for (int i = 0; i< partsCount; ++i)
            {
                parts[i] = new byte[DealConstants.DesBlockSizeBytes];
                Array.Copy(key, i * DealConstants.DesBlockSizeBytes, parts[i], 0, DealConstants.DesBlockSizeBytes);
            }
            return parts;
        }
    }
}