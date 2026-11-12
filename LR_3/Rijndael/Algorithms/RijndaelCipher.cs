using Rijndael.Core.Interfaces;
using Rijndael.MathService;
using Rijndael.Algorithms.Transformations;
using Rijndael.Algorithms.Substitution;

namespace Rijndael.Algorithms
{
    public sealed class RijndaelCipher : ISymmetricCipher
    {
        private readonly int _blockBytes;
        private readonly int _rounds;
        private readonly IKeyExpansion _keyExpansion;
        private readonly IRijndaelTransform _transform;
        private readonly IRoundFunction _roundFunc;
        private byte[][]? _roundKeys;
        public int BlockSize => _blockBytes;
        public RijndaelCipher(RijndaelBlockSize block, RijndaelKeySize keySize, byte module = 0x1B)
        {
            var math = new MathGFService();
            if (!math.IsIrreducible(module)) throw new ArgumentException("module must be irreducible", nameof(module));

            var sBox = new SBoxProvider(module, math);
            _transform = new RijndaelTransform(block, module, math, sBox);
            _keyExpansion = new KeyExpansion(block, keySize, module, math, sBox);
            _blockBytes = (int)block;
            _rounds = RijndaelSpecification.GetRoundCount(block, keySize);
            _roundFunc = new RijndaelRoundFunction(_transform);
        }
        public void SetKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            _roundKeys = _keyExpansion.GenerateRoundKeys(key);
        }
        public byte[] Encrypt(byte[] block)
        {
            Validate(block);
            var rk = _roundKeys ?? throw new InvalidOperationException("key is not set"); ;
            var state = (byte[])block.Clone();

            _transform.AddRoundKey(state, rk[0]);
            for (int r = 1; r < _rounds; ++r)
            {
                _roundFunc.EncryptRound(state, rk[r]);
            }
            _transform.SubBytes(state);
            _transform.ShiftRows(state);
            _transform.AddRoundKey(state, rk[^1]);
            return state;
        }
        public byte[] Decrypt(byte[] block)
        {
            Validate(block);
            var rk = _roundKeys ?? throw new InvalidOperationException("key is not set"); ;
            var state = (byte[])block.Clone();

            _transform.AddRoundKey(state, rk[^1]);
            for (int r = _rounds - 1; r >= 1; --r)
            {
                _roundFunc.DecryptRound(state, rk[r]);
            }
            _transform.InvShiftRows(state);
            _transform.InvSubBytes(state);
            _transform.AddRoundKey(state, rk[0]);
            return state;
        }
        private void Validate(byte[] state)
        {
            ArgumentNullException.ThrowIfNull(state);
            if (state.Length != _blockBytes) throw new ArgumentException(nameof(state));
        }
    }
}