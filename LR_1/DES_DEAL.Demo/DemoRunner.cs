using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine;

namespace DES_DEAL.Demo
{
    public abstract class DemoRunner<TCipher> where TCipher : ISymmetricCipher, new()
    {
        protected readonly string TestDataDirectory;
        protected readonly string OutputDirectory;
        protected readonly int BlockSizeBytes;
        protected readonly string AlgorithmName;
        private const int BYTE = 8;
        private const int BUF_SIZE = 81920;

        private int _total;
        private int _passed;

        public bool AllPassed => _total > 0 && _passed == _total;

        protected DemoRunner(string algorithmName, string testDataDirectory, string outputDirectory)
        {
            AlgorithmName = algorithmName;
            TestDataDirectory = testDataDirectory;
            OutputDirectory = outputDirectory;

            Directory.CreateDirectory(TestDataDirectory);
            Directory.CreateDirectory(OutputDirectory);

            BlockSizeBytes = new TCipher().BlockSize;
        }
        public abstract Task RunAsync();

        protected async Task DemoRandomBytesAsync(byte[] key, string sectionSuffix = "")
        {
            Console.WriteLine("псевдослучайные последовательности");

            int[] sizes = { 1 };
            

            foreach (int size in sizes)
            {
                byte[] data = RandomNumberGenerator.GetBytes(size);
                string label = $"[{size} Б]";
                

                foreach (CipherModeType mode in Enum.GetValues<CipherModeType>())
                {
                    await RunByteArrayCaseAsync(label, mode, PaddingModeType.Zeros, key, new byte[]{0x00});
                    // await RunByteArrayCaseAsync(label, mode, PaddingModeType.Pkcs, key, data);
                }
            }
        }
        // protected async Task DemoPaddingModesAsync(byte[] key, string sectionSuffix = "")
        // {
        //     Console.WriteLine("padding");
        //     int[] sizes = { 16, 1001 };

        //     foreach (PaddingModeType padding in Enum.GetValues<PaddingModeType>())
        //     {
        //         foreach (int size in sizes)
        //         {
        //             byte[] data = RandomNumberGenerator.GetBytes(size);
        //             string label = $"[{size} Б]";
        //             await RunByteArrayCaseAsync(label, CipherModeType.CBC, padding, key, data);
        //         }
        //     }
        // }
        // protected async Task DemoFilesAsync(byte[] key, string sectionSuffix = "")
        // {
        //     Console.WriteLine("File");

        //     string[] files = Directory.Exists(TestDataDirectory)? Directory.GetFiles(TestDataDirectory): Array.Empty<string>();

        //     if (files.Length == 0)
        //     {
        //         Console.WriteLine("TestData empty");
        //         return;
        //     }

        //     foreach (string inputPath in files)
        //     {
        //         string fileName = Path.GetFileName(inputPath);

        //         foreach (CipherModeType mode in Enum.GetValues<CipherModeType>())
        //         {
        //             await RunFileCaseAsync(fileName, inputPath, mode, PaddingModeType.Pkcs, key);
        //         }
        //     }
        // }

        protected async Task RunByteArrayCaseAsync(string label, CipherModeType mode, PaddingModeType padding, byte[] key, byte[] data)
        {
            _total++;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                byte[] iv = GenerateIv(mode);
                var context = new SymmetricCryptoContext<TCipher>(key, mode, padding, iv);

                byte[] cipherText = await context.EncryptAsync(data);
                byte[] plainText = await context.DecryptAsync(cipherText);

                stopwatch.Stop();
                bool ok = data.AsSpan().SequenceEqual(plainText);
                Report(label, mode, padding, ok, stopwatch.Elapsed, key.Length);
                if (ok) _passed++;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Report(label, mode, padding, false, stopwatch.Elapsed, key.Length, ex.Message);
            }
        }

        protected async Task RunFileCaseAsync(
            string fileName, string inputPath, CipherModeType mode, PaddingModeType padding, byte[] key)
        {
            _total++;
            var stopwatch = Stopwatch.StartNew();

            string encryptedPath = Path.Combine(OutputDirectory, $"{fileName}.{key.Length * BYTE}bit.{mode}.enc");
            string decryptedPath = Path.Combine(OutputDirectory, $"{fileName}.{key.Length * BYTE}bit.{mode}.dec");

            try
            {
                byte[] iv = GenerateIv(mode);
                var context = new SymmetricCryptoContext<TCipher>(key, mode, padding, iv);

                await context.EncryptFileAsync(inputPath, encryptedPath);
                await context.DecryptFileAsync(encryptedPath, decryptedPath);

                bool ok = await FilesAreEqualAsync(inputPath, decryptedPath);
                stopwatch.Stop();

                long originalLength = new FileInfo(inputPath).Length;
                Report(fileName, mode, padding, ok, stopwatch.Elapsed, key.Length);
                if (ok) _passed++;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Report(fileName, mode, padding, false, stopwatch.Elapsed, key.Length, ex.Message);
            }
        }

        protected byte[] GenerateIv(CipherModeType mode)
        {
            if (mode == CipherModeType.ECB)
            {
                return null;
            }

            byte[] iv = RandomNumberGenerator.GetBytes(BlockSizeBytes);

            if (mode == CipherModeType.RandomDelta)
            {
                iv[^1] |= 1;
            }

            return iv;
        }

        private static async Task<bool> FilesAreEqualAsync(string pathA, string pathB)
        {
            byte[] hashA = await ComputeFileHashAsync(pathA);
            byte[] hashB = await ComputeFileHashAsync(pathB);
            return hashA.AsSpan().SequenceEqual(hashB);
        }

        private static async Task<byte[]> ComputeFileHashAsync(string path)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: BUF_SIZE, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await SHA256.HashDataAsync(stream);
        }
        private void Report(string label, CipherModeType mode, PaddingModeType padding, bool ok, TimeSpan elapsed, int keyLengthBytes, string error = null)
        {
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"  [{(ok ? "OK" : "FAIL"),-4}]");
            Console.ResetColor();

            Console.Write(
                $" {AlgorithmName}-{keyLengthBytes * 8,-3} | {mode,-11} | {padding,-9} | {label,-24} | " +
                $"{elapsed.TotalMilliseconds,9:F1} мс");

            if (!ok && error is not null)
            {
                Console.Write($"  <- {error}");
            }

            Console.WriteLine();
        }
        protected void PrintSummary()
        {
            Console.ForegroundColor = AllPassed ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{AlgorithmName}: {_passed}/{_total} пройдено");
            Console.ResetColor();
        }
    }
}