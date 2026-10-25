using System.Numerics;
using RSA.MathService.Abstractions;

namespace RSA.MathService.Gcd
{
    public sealed class BinaryGcd : IGcdCalculator
    {
        public BigInteger Gcd(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);

            if(a.IsZero || b.IsZero) return (a.IsZero) ? b : a;
            int k = 0;

            while (a.IsEven && b.IsEven){
                k++;
                (a, b) = (DivOnce(a), DivOnce(b));
            }

            a = Div(a);

            while (a != b)
            {
                if (b.IsEven)
                {
                    b = Div(b);
                }
                else
                {
                    (a,b) =  b > a ? (Div(b-a), a) : (Div(a - b), b);
                }
            }
            return a << k;
        }
        private static BigInteger DivOnce(BigInteger a) => a>>1;
        private static BigInteger Div(BigInteger a){
            while (a.IsEven)
            {
                a >>= 1;
            }
            return a;
        }
    }
}