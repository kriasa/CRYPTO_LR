namespace Rijndael.MathService.GaloisField
{
    public interface IGFArithmeticService
    {
        byte Add(byte a, byte b);
        byte Multiply(byte a, byte b, byte module);
        byte Multiply(byte a, byte b);
        byte GetInverse(byte a, byte module);
        byte GetInverse(byte a);
    }
}