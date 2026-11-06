namespace Rijndael.MathService.GaloisField
{
    public sealed class GFArithmeticService
    {
        public byte Add(byte a, byte b)
        {
            return (byte)a ^ b;
        }
        public byte Multiply(byte a, byte b, byte module)
        {
            
        }
        public byte GetInverse(byte a, byte b, byte module)
        {
            
        }

    }
}