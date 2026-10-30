using System.Numerics;
using RSA.Algorithms;
using RSA.Algorithms.KeyGenerator;
using RSA.Algorithms.Models;
using RSA.Attacks.Wiener;
using RSA.MathService;

namespace RSA.Demo
{
    public static class WienerAttackDemo
    {
        private const double MinProbability = 0.999;

        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("WienerAttak");

            var math = new MathTheoryService();
            var fractionService = new ContinuedFraction();
            var attack = new WienerAttack(fractionService);

            foreach (int bitLength in new[] { 128, 256 })
            {
                Console.WriteLine($"УЯЗВИМЫЙ КЛЮЧ {bitLength} бит");

                var generator = new RsaKeyGenerator(RsaService.PrimalityTest.Miller, MinProbability, bitLength);
                KeyPair vulnerableKey = generator.GenerateKey(vulnerable: true);

                RsaPublicKey pubKey = vulnerableKey.PublicKey;
                BigInteger trueD = vulnerableKey.PrivateKey.D;
                BigInteger truePhi = (vulnerableKey.PrivateKey.P - 1) * (vulnerableKey.PrivateKey.Q - 1);

                try
                {
                    AttackResult result = attack.ExecuteAttack(pubKey);

                    PrintStatus("d", result.D == trueD);
                    PrintStatus("phi(N)", result.Phi == truePhi);

                    BigInteger testMessage = 987654321;
                    BigInteger cipher = math.ModPow(testMessage, pubKey.E, pubKey.N);
                    BigInteger decrypted = math.ModPow(cipher, result.D, pubKey.N);
                    PrintStatus("c^d", decrypted == testMessage);

                    bool hasFractions = result.Convergents != null && result.Convergents.Any();
                    PrintStatus("k/d", hasFractions);

                    if (hasFractions)
                    {
                        Console.WriteLine("подходящие дроби");
                        int count = 0;
                        foreach (var frac in result.Convergents!)
                        {
                            Console.WriteLine($"k = {frac.K}, d={frac.D}");
                            count++;
                            if (count >= 10) break;
                        }
                    }
                }
                catch
                {
                    PrintStatus("ERROR", false);
                }
            }

            Console.WriteLine("e = 65537");

            var safeService = new RsaService(RsaService.PrimalityTest.Miller, MinProbability, 128);
            RsaPublicKey safePublicKey = safeService.PublicKey;

            bool attackBlocked = false;
            try
            {
                attack.ExecuteAttack(safePublicKey);
            }
            catch (InvalidOperationException)
            {
                attackBlocked = true;
            }
            catch
            {
                attackBlocked = false;
            }

            PrintStatus("устойчивость безопасного ключа к атаке", attackBlocked);
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