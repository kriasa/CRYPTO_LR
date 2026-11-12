using System.Reflection;
using Rijndael.Core.Interfaces;
using Rijndael.Algorithms.Transformations;

namespace Rijndael.Algorithms
{
    public sealed class RijndaelRoundFunction : IRoundFunction
    {
        private readonly IRijndaelTransform _t;
        public RijndaelRoundFunction(IRijndaelTransform t) => _t = t ?? throw new ArgumentNullException(nameof(t));
        public void EncryptRound(byte[] c, byte[] roundKey)
        {
            Validate(c, roundKey);
            _t.SubBytes(c);
            _t.ShiftRows(c);
            _t.MixColumns(c);
            _t.AddRoundKey(c, roundKey);
        }
        public void DecryptRound(byte[] m, byte[] roundKey)
        {
            Validate(m, roundKey);

            _t.InvShiftRows(m);
            _t.InvSubBytes(m);
            _t.AddRoundKey(m, roundKey);
            _t.InvMixColumns(m);
        }
        private static void Validate(byte[] block, byte[] roundKey)
        {
            ArgumentNullException.ThrowIfNull(block);
            ArgumentNullException.ThrowIfNull(roundKey);
        }
    }
}