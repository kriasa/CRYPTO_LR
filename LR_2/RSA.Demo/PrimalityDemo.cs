using System.Numerics;
using RSA.MathService;
using RSA.Primality.Test;

namespace RSA.Demo
{
    public static class PrimalityDemo
    {
        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("PrimalityTest");

            var math = new MathTheoryService();
            var fermat = new FermatPrimalityTest(math);
            var solovay = new SolovayStrassenPrimalityTest(math);
            var miller = new MillerRabinPrimalityTest(math);

            double prob = 0.999;
            BigInteger primeNum = 1000000007;
            BigInteger compositeNum = 1000000005;
            BigInteger carmichael = 561;

            PrintStatus("Ферма 1000000007", fermat.IsPrime(primeNum, prob));
            PrintStatus("Соловея-Штрассена 1000000007", solovay.IsPrime(primeNum, prob));
            PrintStatus("Миллера-Рабина 1000000007", miller.IsPrime(primeNum, prob));

            PrintStatus("Ферма: 1000000005", !fermat.IsPrime(compositeNum, prob));
            PrintStatus("Соловея-Штрассена: 1000000005", !solovay.IsPrime(compositeNum, prob));
            PrintStatus("Миллера-Рабина: 1000000005", !miller.IsPrime(compositeNum, prob));

            PrintStatus("Соловея-Штрассена 561", !solovay.IsPrime(carmichael, prob));
            PrintStatus("Миллера-Рабина 561", !miller.IsPrime(carmichael, prob));

            PrintStatus("2 (простое)", miller.IsPrime(2, prob));
            PrintStatus("3 (простое)", miller.IsPrime(3, prob));
            PrintStatus("4 (составное)", !miller.IsPrime(4, prob));
            PrintStatus("1", !miller.IsPrime(1, prob));
            PrintStatus("0", !miller.IsPrime(0, prob));

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