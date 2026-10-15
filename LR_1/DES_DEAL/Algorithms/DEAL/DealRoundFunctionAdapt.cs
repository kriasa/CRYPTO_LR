using System;
using DES_DEAL.Algorithms.DES;
using DES_DEAL.Core.Interfaces;

namespace DES_DEAL.Algorithms.DEAL
{
    public sealed class DesToRoundFunctionAdapt : IRoundFunction
    {
        public readonly Func<ISymmetricCipher> _cipherFactory;
        public DesToRoundFunctionAdapt(Func<ISymmetricCipher> cipherFactory)
        {
            _cipherFactory = cipherFactory ?? throw new ArgumentNullException(nameof(cipherFactory));
        }
        public DesToRoundFunctionAdapt():this(() => new DesCipher()){}
        public byte[] Transform(byte[] block, byte[] roundKey)
        {
            ArgumentNullException.ThrowIfNull(block);
            ArgumentNullException.ThrowIfNull(roundKey);
            var cipher = _cipherFactory();
            cipher.SetKey(roundKey);
            return cipher.Encrypt(block);
        }
    }
}