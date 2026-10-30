using System.Numerics;
using System.Text;
using RSA.Algorithms;

namespace RSA.Demo
{
    public static class RsaDemo
    {
        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("RSA");

            IRsaService rsa = new RsaService(RsaService.PrimalityTest.Miller, 0.99, 128);
            BigInteger n = rsa.PublicKey.N;
            BigInteger e = rsa.PublicKey.E;

            Console.WriteLine($"N {n.GetBitLength()}");
            Console.WriteLine($"E {e}\n");

            Console.WriteLine("BigInt");
            TestNumber(rsa, 0, "0");
            TestNumber(rsa, 1, "1");
            TestNumber(rsa, 2, "2");
            TestNumber(rsa, 1234567890123456789, "1234567890123456789");

            Console.WriteLine("byte[]");

            byte[] zeroByte = new byte[] { 0x00 };
            byte[] decZero = rsa.Decrypt(rsa.Encrypt(zeroByte));
            bool zeroOk = new BigInteger(decZero, isUnsigned: true) == 0;
            PrintStatus("new byte[] { 0x00 }", zeroOk);

            TestBytesExact(rsa, new byte[] { 0x42 }, "{ 0x42 }");
            TestBytesExact(rsa, new byte[] { 0x00, 0x00, 0x55, 0xAA }, "{ 0x00, 0x00, 0x55, 0xAA }");
            TestBytesExact(rsa, new byte[] { 0xFF, 0xFF, 0x01 }, "{ 0xFF, 0xFF, 0x01 }");
            TestBytesExact(rsa, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF }, "{ 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF }");

            byte[] randomBytes = new byte[16];
            Random.Shared.NextBytes(randomBytes);
            randomBytes[^1] |= 1;
            TestBytesExact(rsa, randomBytes, "random byte");

            Console.WriteLine("text");
            TestString(rsa, "A", "\"A\"");
            TestString(rsa, "Hello, World!", "\"Hello, World!\"");

            Console.WriteLine("RegenerateKeys");

            BigInteger oldN = rsa.PublicKey.N;
            byte[] secretBytes = Encoding.ASCII.GetBytes("secret data");
            byte[] oldCipher = rsa.Encrypt(secretBytes);

            rsa.RegenerateKeys();
            PrintStatus("смена ключа N", rsa.PublicKey.N != oldN);

            bool cantDecryptOld;
            try
            {
                byte[] corrupted = rsa.Decrypt(oldCipher);
                cantDecryptOld = !corrupted.AsSpan().SequenceEqual(secretBytes);
            }
            catch
            {
                cantDecryptOld = true;
            }
            PrintStatus("Защита от расшифровки старых данных новым ключом", cantDecryptOld);
        }

        private static void TestNumber(IRsaService rsa, BigInteger num, string title)
        {
            BigInteger cipher = rsa.Encrypt(num);
            BigInteger decrypted = rsa.Decrypt(cipher);
            PrintStatus(title, decrypted == num);
        }

        private static void TestBytesExact(IRsaService rsa, byte[] data, string title)
        {
            byte[] cipher = rsa.Encrypt(data);
            byte[] decrypted = rsa.Decrypt(cipher);
            PrintStatus(title, data.AsSpan().SequenceEqual(decrypted));
        }

        private static void TestString(IRsaService rsa, string text, string title)
        {
            byte[] raw = Encoding.ASCII.GetBytes(text);
            byte[] cipher = rsa.Encrypt(raw);
            byte[] decrypted = rsa.Decrypt(cipher);
            string restored = Encoding.ASCII.GetString(decrypted);
            PrintStatus(title, text == restored);
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