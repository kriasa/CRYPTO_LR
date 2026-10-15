using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DES_DEAL.Algorithms.DES;
using DES_DEAL.Core.Enums;
using DES_DEAL.Engine;

namespace DES_DEAL.Demo
{
    /// <summary>
    /// Демонстрация шифрования/дешифрования алгоритмом DES псевдослучайных байтов
    /// и файлов, во всех режимах шифрования и наборах набивки, при помощи типов из
    /// заданий 2-4 (<see cref="SymmetricCryptoContext{TCipher}"/>, <see cref="DesCipher"/>).
    /// Задание 5.
    /// </summary>
    public sealed class DemoRunner
    {
        private const int DesBlockSizeBytes = 8;

        private readonly string _testDataDirectory;
        private readonly string _outputDirectory;

        private int _total;
        private int _passed;

        public bool AllPassed => _total > 0 && _passed == _total;

        public DemoRunner(string testDataDirectory, string outputDirectory)
        {
            _testDataDirectory = testDataDirectory;
            _outputDirectory = outputDirectory;

            Directory.CreateDirectory(_testDataDirectory);
            Directory.CreateDirectory(_outputDirectory);
        }

        public async Task RunAsync()
        {
            PrintHeader("DES - демонстрация шифрования/дешифрования (задание 5)");

            await DemoRandomBytesAsync();
            await DemoPaddingModesAsync();
            await DemoFilesAsync();

            PrintSummary();
        }

        // -----------------------------------------------------------------
        // 1. Псевдослучайные последовательности байтов - все режимы шифрования
        // -----------------------------------------------------------------
        private async Task DemoRandomBytesAsync()
        {
            PrintSection("Псевдослучайные байты - все режимы шифрования (набивка PKCS7)");

            // Разные длины специально: 0 и 7 - заведомо не кратны блоку (8 байт),
            // проверяют набивку; 8 - ровно один блок (граничный случай); остальные -
            // "обычные" данные разного объёма.
            int[] sizes = { 0, 1, 7, 8, 1_000, 500_000 };
            byte[] key = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);

            foreach (int size in sizes)
            {
                byte[] data = RandomNumberGenerator.GetBytes(size);
                string label = $"random[{size} Б]";

                foreach (CipherModeType mode in Enum.GetValues<CipherModeType>())
                {
                    await RunByteArrayCaseAsync(label, mode, PaddingModeType.Pkcs, key, data);
                }
            }
        }

        // -----------------------------------------------------------------
        // 2. Режимы набивки - отдельно, на границе блока и не на границе
        // -----------------------------------------------------------------
        private async Task DemoPaddingModesAsync()
        {
            PrintSection("Режимы набивки (режим шифрования фиксирован - CBC)");

            // 0, 8 и 16 - ровно кратны размеру блока DES: проверяют, что ANSI X.923 /
            // PKCS7 / ISO 10126 в этом случае честно добавляют целый лишний блок набивки
            // (иначе Unpad не смог бы отличить "набивки нет" от "набивка нулевой длины").
            int[] sizes = { 0, 8, 16, 1001 };
            byte[] key = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);

            foreach (PaddingModeType padding in Enum.GetValues<PaddingModeType>())
            {
                foreach (int size in sizes)
                {
                    byte[] data = RandomNumberGenerator.GetBytes(size);
                    string label = $"padding-check[{size} Б]";
                    await RunByteArrayCaseAsync(label, CipherModeType.CBC, padding, key, data);
                }
            }
        }

        // -----------------------------------------------------------------
        // 3. Файлы из TestData - все режимы шифрования
        // -----------------------------------------------------------------
        private async Task DemoFilesAsync()
        {
            PrintSection($"Файлы из {_testDataDirectory}");

            string[] files = Directory.Exists(_testDataDirectory)
                ? Directory.GetFiles(_testDataDirectory)
                : Array.Empty<string>();

            if (files.Length == 0)
            {
                Console.WriteLine("  (папка TestData пуста - положи туда txt/mp3/png/mp4/.cs и т.д. и запусти снова)");
                return;
            }

            byte[] key = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);

            foreach (string inputPath in files)
            {
                string fileName = Path.GetFileName(inputPath);

                foreach (CipherModeType mode in Enum.GetValues<CipherModeType>())
                {
                    await RunFileCaseAsync(fileName, inputPath, mode, PaddingModeType.Pkcs, key);
                }
            }
        }

        // -----------------------------------------------------------------
        // Общая логика одного случая: массив байт
        // -----------------------------------------------------------------
        private async Task RunByteArrayCaseAsync(
            string label, CipherModeType mode, PaddingModeType padding, byte[] key, byte[] data)
        {
            _total++;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                byte[] iv = GenerateIv(mode);
                var context = new SymmetricCryptoContext<DesCipher>(key, mode, padding, iv);

                byte[] cipherText = await context.EncryptAsync(data);
                byte[] plainText = await context.DecryptAsync(cipherText);

                stopwatch.Stop();
                bool ok = data.AsSpan().SequenceEqual(plainText);
                Report(label, mode, padding, ok, stopwatch.Elapsed, data.Length);
                if (ok) _passed++;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Report(label, mode, padding, false, stopwatch.Elapsed, data.Length, ex.Message);
            }
        }

        // -----------------------------------------------------------------
        // Общая логика одного случая: файл
        // -----------------------------------------------------------------
        private async Task RunFileCaseAsync(
            string fileName, string inputPath, CipherModeType mode, PaddingModeType padding, byte[] key)
        {
            _total++;
            var stopwatch = Stopwatch.StartNew();

            string encryptedPath = Path.Combine(_outputDirectory, $"{fileName}.{mode}.enc");
            string decryptedPath = Path.Combine(_outputDirectory, $"{fileName}.{mode}.dec");

            try
            {
                byte[] iv = GenerateIv(mode);
                var context = new SymmetricCryptoContext<DesCipher>(key, mode, padding, iv);

                // Именно файловые перегрузки из части 5 - потоковая обработка кусками,
                // а не чтение всего файла в массив байт целиком.
                await context.EncryptFileAsync(inputPath, encryptedPath);
                await context.DecryptFileAsync(encryptedPath, decryptedPath);

                bool ok = await FilesAreEqualAsync(inputPath, decryptedPath);
                stopwatch.Stop();

                long originalLength = new FileInfo(inputPath).Length;
                Report(fileName, mode, padding, ok, stopwatch.Elapsed, originalLength);
                if (ok) _passed++;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Report(fileName, mode, padding, false, stopwatch.Elapsed, 0, ex.Message);
            }
        }

        /// <summary>
        /// Генерирует IV нужного режима. Для RandomDelta вторая половина IV - дельта
        /// счётчика, и она обязана быть нечётной (gcd(delta, 2^(8*halfSize)) == 1,
        /// см. RandomDeltaMode) - принудительно выставляем младший бит последнего байта.
        /// </summary>
        private static byte[] GenerateIv(CipherModeType mode)
        {
            if (mode == CipherModeType.ECB)
            {
                return null;
            }

            byte[] iv = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);

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
            await using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await SHA256.HashDataAsync(stream);
        }

        // -----------------------------------------------------------------
        // Вывод в консоль
        // -----------------------------------------------------------------

        private static void Report(
            string label, CipherModeType mode, PaddingModeType padding,
            bool ok, TimeSpan elapsed, long sizeBytes, string error = null)
        {
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"  [{(ok ? "OK" : "FAIL"),-4}]");
            Console.ResetColor();

            Console.Write(
                $" DES | {mode,-11} | {padding,-9} | {label,-24} | " +
                $"{FormatSize(sizeBytes),10} | {elapsed.TotalMilliseconds,9:F1} мс");

            if (!ok && error is not null)
            {
                Console.Write($"  <- {error}");
            }

            Console.WriteLine();
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} Б";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} КБ";
            return $"{bytes / 1024.0 / 1024.0:F1} МБ";
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', title.Length));
            Console.WriteLine(title);
            Console.WriteLine(new string('=', title.Length));
        }

        private static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"--- {title} ---");
        }

        private void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.ForegroundColor = AllPassed ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Итого: {_passed}/{_total} проверок пройдено.");
            Console.ResetColor();
        }
    }
}