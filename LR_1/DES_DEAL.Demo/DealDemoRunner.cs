using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DES_DEAL.Algorithms.DEAL;
using DES_DEAL.Core.Enums;

namespace DES_DEAL.Demo
{
    public sealed class DealDemoRunner : DemoRunner<DealCipher>
    {
        private static readonly int[] SupportedKeyLengthsBytes = { 16, 24, 32 };
        private const int BYTE = 8;
        private const int FIRST_KEY = 16;
        public DealDemoRunner(string testDataDirectory, string outputDirectory): base("DEAL", testDataDirectory, outputDirectory)
        {
        }

        public override async Task RunAsync()
        {
            Console.WriteLine("DEAL");

            foreach (int keyLengthBytes in SupportedKeyLengthsBytes)
            {
                string suffix = $" [key {keyLengthBytes * BYTE}]";
                byte[] key = RandomNumberGenerator.GetBytes(keyLengthBytes);

                await DemoRandomBytesAsync(key, suffix);
                if (keyLengthBytes == FIRST_KEY)
                {
                    // await DemoPaddingModesAsync(key, suffix);
                }
                if (keyLengthBytes == FIRST_KEY)
                {
                    // await DemoFilesDealLimitedAsync(key, suffix);
                }
            }

            PrintSummary();
        }

        private async Task DemoFilesDealLimitedAsync(byte[] key, string sectionSuffix)
        {
            Console.WriteLine("псевдослучайные последовательности");

            string[] files = Directory.Exists(TestDataDirectory) ? Directory.GetFiles(TestDataDirectory) : Array.Empty<string>();

            if (files.Length == 0) return;

            CipherModeType[] fastModes = { CipherModeType.CBC, CipherModeType.CTR };

            foreach (string inputPath in files)
            {
                string fileName = Path.GetFileName(inputPath);

                foreach (CipherModeType mode in fastModes)
                {
                    await RunFileCaseAsync(fileName, inputPath, mode, PaddingModeType.Pkcs, key);
                }
            }
        }
    }
}