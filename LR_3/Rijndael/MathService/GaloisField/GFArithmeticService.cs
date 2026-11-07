namespace Rijndael.MathService.GaloisField
{
    public sealed class GFArithmeticService : IGFArithmeticService
    {
        private readonly byte defaultMod  = 0x1B;
        private readonly byte mask = 0x80;
        private readonly byte power = 254;
        public byte Add(byte a, byte b)
        {
            return (byte)(a ^ b);
        }
        public byte Multiply(byte a, byte b, byte module)
        {
            byte res = 0;
            if (a == 0 || b == 0) return res;

            while (b > 0)
            {
                if ((b & 1) == 1) res ^= a;

                a = (byte)(((a & mask) != 0) ? ((a << 1) ^ module) : a << 1);
                b >>= 1;
            }
            return res;
        }
        public byte Multiply(byte a, byte b) => Multiply(a, b, defaultMod);
        public byte GetInverse(byte a, byte module)
        {
            return ModularPow(a, power, module);
        }
        public byte GetInverse(byte a) => GetInverse(a, defaultMod);
        public byte ModularPow (byte a, byte exp, byte mod)
        {
            byte res = 1;
            while (exp > 0)
            {
                if ((exp & 1) == 1) res=Multiply(res, a, mod);
                a = Multiply(a, a, mod);
                exp >>= 1;
            }
            return res;
        }

    }
}