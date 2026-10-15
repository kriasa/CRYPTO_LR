using System;
using System.Threading.Tasks.Dataflow;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine.CipherModes;

namespace DES_DEAL.Engine
{
    public sealed class FeistelNetwork
    {
        private readonly IKeyExpansion _keyExpansion;
        private readonly IRoundFunction _roundFunction;
        private byte[][]? _roundKeys;

        public FeistelNetwork(IKeyExpansion keyExpansion, IRoundFunction roundFunction)
        {
            _keyExpansion = keyExpansion ?? throw new ArgumentNullException(nameof(keyExpansion));
            _roundFunction = roundFunction ?? throw new ArgumentNullException(nameof(roundFunction));
        }
        public void SetKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            byte[][] roundKeys = _keyExpansion.GenerateRoundKeys(key);
            if (roundKeys is null || roundKeys.Length == 0) throw new InvalidOperationException("empty set of keys");

            _roundKeys = roundKeys;
        } 
        public byte[] Encrypt(byte[] block) => Transform(block, _roundKeys!);
        public byte[] Decrypt(byte[] block) => Transform(block, ReversedRoundKeys());
        private byte[] Transform(byte[] block, byte[][] roundKeys)
        {
            if (_roundKeys is null) throw new InvalidOperationException("no key values ​​received");
            ArgumentNullException.ThrowIfNull(block);

            if (block.Length % 2 != 0) throw new ArgumentException($"the block length ({block.Length}) is incorrect", nameof(block));

            int halfSize = block.Length / 2;

            byte[] left = new byte[halfSize];
            byte[] right = new byte[halfSize];
            Array.Copy(block, 0, left, 0, halfSize);
            Array.Copy(block, halfSize, right, 0, halfSize);

            foreach (byte[] roundKey in roundKeys)
            {
                byte[] fOutput = _roundFunction.Transform(right, roundKey);
                byte[] newRight = CipherModeHelpers.Xor(left, fOutput);

                left = right;
                right = newRight;
            }

            var result = new byte[block.Length];
            Array.Copy(right, 0, result, 0, halfSize);
            Array.Copy(left, 0, result, halfSize, halfSize);

            return result;
        }
        private byte[][] ReversedRoundKeys()
        {
            int count = _roundKeys!.Length;
            var reversed = new byte[count][];
            for (int i = 0; i < count; ++i)
            {
                reversed[i] = _roundKeys[count - 1 - i];
            }
            return reversed;
        }
    }
}