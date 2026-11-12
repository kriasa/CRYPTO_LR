namespace Rijndael.Algorithms
{
    public enum RijndaelBlockSize
    {
        Bits128 = 16,
        Bits192 = 24,
        Bits256 = 32
    }
    public enum RijndaelKeySize
    {
        Bits128 = 16,
        Bits192 = 24,
        Bits256 = 32
    }
    public static class RijndaelSpecification
    {
        public const int StateRows = 4;
        public static ReadOnlySpan<byte> MixColumnsVector => [0x02, 0x01, 0x01, 0x03];
        public static ReadOnlySpan<byte> InvMixColumnsVector => [0x0E, 0x09, 0x0D, 0x0B];
        public static int GetRoundCount(RijndaelBlockSize blockSize, RijndaelKeySize keySize)
            => Math.Max((int)blockSize / StateRows, (int)keySize / StateRows) + 6;
        public static int[] GetShift(RijndaelBlockSize blockSize) => blockSize switch
        {
            RijndaelBlockSize.Bits128 => new[] {0, 1, 2, 3},
            RijndaelBlockSize.Bits192 => new[] {0, 1, 2, 3},
            RijndaelBlockSize.Bits256 => new[] {0, 1, 3, 4},
            _ => throw new ArgumentOutOfRangeException(nameof(blockSize)) 
        };
        public static int GetColumnsCount(RijndaelBlockSize blockSize)=>(int)blockSize / StateRows;
        public static int GetKeyColumnsCount(RijndaelKeySize keySize) => (int)keySize / StateRows;
    }
}