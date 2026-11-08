using Rijndael.MathService;

namespace Rijndael.Algorithms.Substitution
{
    public sealed class SBoxProvider : ISBoxProvider
    {
        private const int SBoxSize = 256;
        private const byte AffineConst = 0x63;
        private const byte InvAffineConst = 0x05;
        private const int BitPerByte = 8;
        private readonly byte _module;
        private readonly IMathGFService _service;
        private readonly Lazy<byte[]> _sBox;
        private readonly Lazy<byte[]> _invSBox;
        public SBoxProvider(IMathGFService service, byte module){

            ArgumentNullException.ThrowIfNull(service);
            _service = service;

            if (!_service.IsIrreducible(module)) throw new ArgumentException("module must be irreducible",nameof(module));
            _module = module;

            _sBox = new Lazy<byte[]>(GenerateSBox);
            _invSBox = new Lazy<byte[]>(GenerateInvSBox);
        }
        public byte Sub(byte value) => _sBox.Value[value];
        public byte InvSub(byte value) => _invSBox.Value[value];
        private byte[] GenerateSBox()
        {
            var sBoxTable = new byte[SBoxSize];

            for (int i = 0; i < SBoxSize; ++i)
            {
                sBoxTable[i] = ApplyAffineTransformation(_service.GetInverse((byte)i, _module));
            }

            return sBoxTable;
        }
        private byte[] GenerateInvSBox()
        {
            var invSBoxTable = new byte[SBoxSize];

            for (int i = 0; i < SBoxSize; ++i)
            {
                invSBoxTable[i] = _service.GetInverse(ApplyInvAffineTransformation((byte)i), _module);
            }

            return invSBoxTable;
        }
        private static byte ApplyAffineTransformation(byte x) 
            => (byte)(x ^ RotateLeft(x,1) ^ RotateLeft(x, 2) ^ RotateLeft(x, 3) ^ RotateLeft(x, 4) ^ AffineConst);
        private static byte ApplyInvAffineTransformation(byte x) =>(byte)(RotateLeft(x, 1) ^ RotateLeft(x, 3) ^ RotateLeft(x, 6) ^ InvAffineConst);
        private static byte RotateLeft(byte x, byte n)
        {
            var shift = x >> (BitPerByte - n);
            return (byte)((x << n) | shift);
        }
    }
}