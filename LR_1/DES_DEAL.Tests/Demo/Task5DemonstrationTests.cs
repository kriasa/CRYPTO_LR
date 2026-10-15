using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DES_DEAL.Algorithms.DES;
using DES_DEAL.Core.Enums;
using DES_DEAL.Engine;
using Xunit;
using Xunit.Abstractions;

namespace DES_DEAL.Tests.Demo
{
    public class Task5DemonstrationTests
    {
        private readonly ITestOutputHelper _output;
        private const int DesBlockSizeBytes = 8;

        public Task5DemonstrationTests(ITestOutputHelper output)
        {
            _output = output; // Позволяет выводить текст в консоль во время тестов xUnit
        }

        [Fact]
        public async Task Demonstrate_DES_RandomBytes_AllModesAndPaddings()
        {
            _output.WriteLine("=== ДЕМОНСТРАЦИЯ ЗАДАНИЯ 5: Псевдослучайные байты ===");

            int[] sizes = { 0, 1, 7, 8, 64, 1024, 10000 };
            byte[] key = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);

            int total = 0;
            int passed = 0;

            foreach (int size in sizes)
            {
                byte[] data = RandomNumberGenerator.GetBytes(size);

                foreach (CipherModeType mode in Enum.GetValues<CipherModeType>())
                {
                    foreach (PaddingModeType padding in Enum.GetValues<PaddingModeType>())
                    {
                        total++;
                        try
                        {
                            byte[] iv = GenerateIv(mode);
                            var context = new SymmetricCryptoContext<DesCipher>(key, mode, padding, iv);

                            byte[] cipherText = await context.EncryptAsync(data);
                            byte[] plainText = await context.DecryptAsync(cipherText);

                            bool ok = data.AsSpan().SequenceEqual(plainText);
                            Assert.True(ok, $"Ошибка на размере {size}, режиме {mode}, набивке {padding}");
                            passed++;
                        }
                        catch (Exception ex)
                        {
                            _output.WriteLine($"FAIL: Размер={size}, Режим={mode}, Паддинг={padding} -> {ex.Message}");
                            throw;
                        }
                    }
                }
            }

            _output.WriteLine($"Успешно пройдено тестов байтов: {passed}/{total}");
        }

        [Fact]
        public async Task Demonstrate_DES_Files_StreamingAndHashing()
        {
            _output.WriteLine("=== ДЕМОНСТРАЦИЯ ЗАДАНИЯ 5: Файлы (текст, код, бинарники) ===");

            string tempDir = Path.Combine(Path.GetTempPath(), "DesDemoFiles_" + Guid.NewGuid());
            string testDataDir = Path.Combine(tempDir, "TestData");
            string outputDir = Path.Combine(tempDir, "Output");

            Directory.CreateDirectory(testDataDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                // Создаем разные тестовые файлы (имитируем текст, код, музыку, картинки)
                string textFile = Path.Combine(testDataDir, "sample.txt");
                string codeFile = Path.Combine(testDataDir, "source.cs");
                string binaryFile = Path.Combine(testDataDir, "data.bin");

                await File.WriteAllTextAsync(textFile, "Привет, мир! Это тестовый текстовый файл для алгоритма DES.");
                await File.WriteAllTextAsync(codeFile, "public class RedBlackTreeAllocator { /* код тестов */ }");

                // Создаем бинарный файл на 500 КБ (имитация музыки/картинки)
                byte[] binData = RandomNumberGenerator.GetBytes(500 * 1024);
                await File.WriteAllBytesAsync(binaryFile, binData);

                byte[] key = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);
                string[] files = { textFile, codeFile, binaryFile };

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);

                    // Проверяем на основных режимах (CBC, CTR, ECB)
                    CipherModeType[] demoModes = { CipherModeType.ECB, CipherModeType.CBC, CipherModeType.CTR };

                    foreach (CipherModeType mode in demoModes)
                    {
                        string encPath = Path.Combine(outputDir, $"{fileName}.{mode}.enc");
                        string decPath = Path.Combine(outputDir, $"{fileName}.{mode}.dec");

                        byte[] iv = GenerateIv(mode);
                        var context = new SymmetricCryptoContext<DesCipher>(key, mode, PaddingModeType.Pkcs, iv);

                        // Потоковое шифрование и дешифрование файла кусками
                        await context.EncryptFileAsync(filePath, encPath);
                        await context.DecryptFileAsync(encPath, decPath);

                        // Сверяем хэши SHA-256 оригинала и расшифрованного файла
                        bool hashesEqual = await FilesAreEqualAsync(filePath, decPath);
                        Assert.True(hashesEqual, $"Хэши не совпали для файла {fileName} в режиме {mode}!");

                        _output.WriteLine($"[OK] Файл {fileName,-12} | Режим: {mode,-6} | Хэши совпали!");
                    }
                }
            }
            finally
            {
                // Убираем за собой временные файлы
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private static byte[]? GenerateIv(CipherModeType mode)
        {
            if (mode == CipherModeType.ECB) return null;
            byte[] iv = RandomNumberGenerator.GetBytes(DesBlockSizeBytes);
            if (mode == CipherModeType.RandomDelta) iv[^1] |= 1;
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
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await SHA256.HashDataAsync(stream);
        }
    }
}