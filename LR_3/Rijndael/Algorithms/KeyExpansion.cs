using System.ComponentModel;
using Rijndael.Algorithms.Transformations;
using Rijndael.Core.Interfaces;
using Rijndael.MathService;
using Rijndael.Algorithms.Substitution;

namespace Rijndael.Algorithms
{
    public sealed class KeyExpansion : IKeyExpansion
    {
        private readonly IMathGFService _service;
        private readonly ISBoxProvider _sBoxBuild;
        private readonly byte _module;
        private readonly int _nb;
        private readonly int _nk;
        private readonly int _round;
        private readonly byte[] _rcon;
        public KeyExpansion (RijndaelBlockSize blockSize, RijndaelKeySize keySize, byte module, IMathGFService service, ISBoxProvider sBoxBuild)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _module = module;
            _nb = RijndaelSpecification.GetColumnsCount(blockSize);
            _nk = RijndaelSpecification.GetKeyColumnsCount(keySize);
            _round = RijndaelSpecification.GetRoundCount(blockSize, keySize);
            _sBoxBuild = sBoxBuild ?? throw new ArgumentNullException(nameof(sBoxBuild));
            _rcon = GetRcon((_nb * (_round + 1) - 1) / _nk);
        }
        public KeyExpansion(RijndaelBlockSize blockSize, RijndaelKeySize keySize, byte module): 
            this (blockSize, keySize, module, new MathGFService(), new SBoxProvider(module)){}
        public byte[][] GenerateRoundKeys(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (key.Length != _nk * RijndaelSpecification.StateRows) throw new ArgumentException("invalid key length", nameof(key));

            var words = new byte[_nb * (_round + 1) * RijndaelSpecification.StateRows];
            key.CopyTo(words);

            Span<byte> tmp = stackalloc byte[RijndaelSpecification.StateRows];

            for (int w = _nk; w < _nb * (_round + 1); ++w)
            {
                words.AsSpan((w - 1) * RijndaelSpecification.StateRows, RijndaelSpecification.StateRows).CopyTo(tmp);

                if (w % _nk == 0)
                {
                    RotWord(tmp);
                    SubWord(tmp);
                    tmp[0] = _service.Add(tmp[0], _rcon[w / _nk - 1]);
                }else if (_nk == 8 && w % _nk == RijndaelSpecification.StateRows)
                {
                    SubWord(tmp);
                }

                for (int k = 0; k < RijndaelSpecification.StateRows; ++k)
                {
                    words[k + w * RijndaelSpecification.StateRows] = _service.Add(tmp[k], words[k + (w -_nk)* RijndaelSpecification.StateRows]);
                }
            }

            var roundKeys = new byte[_round + 1][];

            for (int r = 0; r <=_round; ++r)
            {
                var totalKey = _nb * RijndaelSpecification.StateRows;
                roundKeys[r] = new byte[totalKey];
                Array.Copy(words, r * totalKey, roundKeys[r], 0, totalKey);
            }

            return roundKeys;
        }
        private void RotWord(Span<byte> w)
        {
            var first = w[0];
            w.Slice(1).CopyTo(w);
            w[3] = first;
        }
        private void SubWord(Span<byte> w)
        {
            for (int i = 0; i < w.Length; ++i)
            {
                w[i] = _sBoxBuild.Sub(w[i]);
            }
        }
        private byte[] GetRcon(int count)
        {
            var rcon = new byte[count];
            if (count == 0) return rcon;

            rcon[0] = 0x01;

            for (int i = 1; i < count; ++i)
            {
                rcon[i] = _service.Multiply(rcon[i - 1],0x02, _module);
            }

            return rcon;
        }
    }
}