using System.Numerics;
using RSA.MathService;
using RSA.MathService.Enums;

namespace RSA.Demo
{
    public static class MathTheoryDemo
    {
        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("MathService");

            var math = new MathTheoryService();

            Console.WriteLine("Jacobi/Legendre");
            PrintStatus("Legendre (2 / 7) = 1", math.LegendreSymbol(2, 7) == 1);
            PrintStatus("Legendre (3 / 7) = -1", math.LegendreSymbol(3, 7) == -1);
            PrintStatus("Jacobi (2 / 9) = 1", math.JacobiSymbol(2, 9) == 1);
            PrintStatus("Jacobi (5 / 21) = 1", math.JacobiSymbol(5, 21) == 1);
            PrintStatus("Jacobi (6 / 21) = 0", math.JacobiSymbol(6, 21) == 0);

            Console.WriteLine("GCD");
            PrintStatus("gcd gcd(1071, 462)=21", math.Gcd(1071, 462, GcdMethod.Classical) == 21);
            PrintStatus("binaryGcd gcd(1071, 462)=21", math.Gcd(1071, 462, GcdMethod.Binary) == 21);

            BigInteger aBig = 1234567890123456789;
            BigInteger bBig = 987654321;
            bool gcdMatch = math.Gcd(aBig, bBig, GcdMethod.Classical) == math.Gcd(aBig, bBig, GcdMethod.Binary);
            PrintStatus("gcd==binaryGcd", gcdMatch);

            Console.WriteLine("ExtendedEuclid");
            BigInteger a = 240, b = 46;
            var (gcd, x, y) = math.ExtendedGcd(a, b);
            bool bezoutOk = a * x + b * y == gcd && gcd == 2;
            PrintStatus("a*x + b*y = gcd(a, b)", bezoutOk);

            Console.WriteLine("ModPow");
            PrintStatus("7^560 mod 561 = 1", math.ModPow(7, 560, 561) == 1);
            PrintStatus("3^4 mod 5 = 1", math.ModPow(3, 4, 5) == 1);
            PrintStatus("2^10 mod 1000 = 24", math.ModPow(2, 10, 1000) == 24);

            Console.WriteLine("PHI");
            PrintStatus("по определению: phi(36) = 12", math.Phi(36, EulerMethod.Definition) == 12);
            PrintStatus("через факторизацию: phi(36) = 12", math.Phi(36, EulerMethod.Factorization) == 12);
            PrintStatus("через ДПФ: phi(36) = 12", math.Phi(36, EulerMethod.Dft) == 12);

            Console.WriteLine("Carmichael");
            PrintStatus("lambda(36) = 6", math.Lambda(36) == 6);
            PrintStatus("lambda(15) = 4", math.Lambda(15) == 4);
            PrintStatus("lambda(561) = 80", math.Lambda(561) == 80);

        }

        private static void PrintStatus(string title, bool ok)
        {
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"  [{(ok ? "OK" : "FAIL"),-4}]");
            Console.ResetColor();
            Console.WriteLine($" {title}");
        }
    }
}