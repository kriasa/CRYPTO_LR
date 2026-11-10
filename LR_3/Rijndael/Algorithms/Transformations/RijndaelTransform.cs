using Rijndael.Algorithms;
using Rijndael.MathService;
using Rijndael.Algorithms.Substitution;
using System.Data;

namespace Rijndael.Algorithms.Transforms
{
    public sealed class RijndaelTransform : IRijndaelTransform
    {
        private readonly IMathGFService _service;
        private readonly ISBoxProvider _sBoxBuild;
        private  readonly int[] _shift;
        private readonly byte _module;
        private readonly int _nb;
        public RijndaelTransform (RijndaelBlockSize blockSize, byte module, IMathGFService service, ISBoxProvider sBoxBuild)
        { 
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _sBoxBuild =  sBoxBuild ?? throw new ArgumentNullException(nameof(sBoxBuild));
            _shift = RijndaelSpecification.GetShift(blockSize);
            _module = module;
            _nb = RijndaelSpecification.GetColumnsCount(blockSize);
        }
        public RijndaelTransform(RijndaelBlockSize blockSize, byte module): 
            this(blockSize, module, new MathGFService(), new SBoxProvider(module)){}
        public void SubBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = _sBoxBuild.Sub(data[i]);
            }
        }
        public void ShiftRows(byte[] data)
        {
            for (int i = 1; i < RijndaelSpecification.Nb_rows; ++i)
            {
                RotateRowLeft(data, i, _shift[i], _nb);
            }
        }
        public void AddRoundKey(byte[] data, byte[] RK)
        {
            if (data.Length != RK.Length) throw new ArgumentException("state and rk must have the same length");

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]^=RK[i];
            }
        }
        public void MixColumns(byte[] data)
        {
            for (int i = 0; i < _nb; ++i)
            {
                MultColumn(data, i, RijndaelSpecification.MixColumnsVector);
            }
        }
        private void MultColumn(byte[] data, int k, byte[] vector)
        {
            var index = k * RijndaelSpecification.Nb_rows;

            var s0 = data[index];
            var s1 = data[index + 1];
            var s2 = data[index + 2];
            var s3 = data[index + 3];

            data[index] = (byte)(
                _service.Multiply(s0, vector[0], _module) ^
                _service.Multiply(s1, vector[3], _module) ^
                _service.Multiply(s2, vector[2], _module) ^
                _service.Multiply(s3, vector[1], _module));

            data[index + 1] = (byte)(
                _service.Multiply(s0, vector[1], _module) ^
                _service.Multiply(s1, vector[0], _module) ^
                _service.Multiply(s2, vector[3], _module) ^
                _service.Multiply(s3, vector[2], _module));

            data[index + 2] = (byte)(
                _service.Multiply(s0, vector[2], _module) ^
                _service.Multiply(s1, vector[1], _module) ^
                _service.Multiply(s2, vector[0], _module) ^
                _service.Multiply(s3, vector[3], _module));

            data[index + 3] = (byte)(
                _service.Multiply(s0, vector[3], _module) ^
                _service.Multiply(s1, vector[2], _module) ^
                _service.Multiply(s2, vector[1], _module) ^
                _service.Multiply(s3, vector[0], _module));
        }
        private static void RotateRowLeft(byte[] data, int row, int shift, int nb)
        {
            ReverseRow(data, row, 0, shift - 1);
            ReverseRow(data, row, shift, nb - 1);
            ReverseRow(data, row, 0, nb - 1);
        }
        private static void ReverseRow(byte[] data, int row, int from, int to)
        {
            while (from < to)
            {
                var k = row + RijndaelSpecification.Nb_rows * from;
                var j = row + RijndaelSpecification.Nb_rows * to;

                (data[k], data[j]) = (data[j], data[k]);
                ++from;
                --to;
            }
        }

        public void InvSubBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = _sBoxBuild.InvSub(data[i]);
            }
        }
        public void InvShiftRows(byte[] data)
        {
            for (int i = 1; i < RijndaelSpecification.Nb_rows; ++i)
            {
                RotateRowLeft(data, i, _nb - _shift[i], _nb);
            }
        }
        public void InvMixColumns(byte[] data)
        {
            for (int i = 0; i < _nb; ++i)
            {
                MultColumn(data, i, RijndaelSpecification.InvMixColumnsVector);
            }
        }
    }
}