using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rijndael.Algorithms;
using Rijndael.Core;
using Rijndael.Core.Modes;
using Rijndael.Core.Padding;
using Rijndael.MathService;

namespace Rijndael.Demo
{
    public sealed class RijndaelDemo
    {
        private const byte Module = 0x1B;
        private const int BitPerByte = 8;
        private const int BufSize = 81920;

        private readonly string _testDataDir;
        private readonly string _outputDir;
        private readonly IMathGFService _service;

        public RijndaelDemo(string testDataDirectory, string outputDirectory)
        {
            _testDataDir = testDataDirectory;
            _outputDir = outputDirectory;
            _service = new MathGFService();
            Directory.CreateDirectory(_outputDir);
        }

        public void RunArrayDemo(CipherModeType mode, PaddingModeType padding)
        {
            // Console.WriteLine(string.Join(", ", _service.IrreducibilityPolynomials8().Select(p => $"0x{p:X2}")));

            foreach (var (block, key) in Combinations())
            {
                int blockSize = (int)block;
                var context = CreateContext(block, key, mode, padding);

                Console.WriteLine();
                Console.WriteLine($"Block {blockSize * BitPerByte} bit, key {(int)key * BitPerByte} bit");

                foreach (byte[] input in TestArrays(blockSize))
                {
                    byte[] output = Array.Empty<byte>();
                    string status;
                    ConsoleColor color;
                    try
                    {
                        context.Encrypt(input, out byte[] encrypted);
                        context.Decrypt(encrypted, out output);

                        if (output.SequenceEqual(input))
                        {
                            status = "[ OK ]";
                            color = ConsoleColor.Green;
                        }
                        else
                        {
                            status = "[FAIL]";
                            color = ConsoleColor.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        status = $"[FAIL] {ex.GetType().Name}: {ex.Message}";
                        color = ConsoleColor.Red;
                    }

                    Write(status, color);
                    Console.WriteLine($" in={Hex(input)}  out={Hex(output)}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Mode {mode}, padding {padding}:");
        }

        private static IEnumerable<byte[]> TestArrays(int blockSize)
        {
            yield return Array.Empty<byte>();
            yield return new byte[] { 0x00 };
            yield return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };
            yield return new byte[] { 0x42 };
            yield return new byte[] { 0x00, 0x00, 0x55, 0xAA };
            yield return new byte[] { 0xAA, 0xBB, 0x00, 0x00 };
            yield return RandomBytes(blockSize - 1,1);
            yield return RandomBytes(blockSize, 1);
        }

        public async Task RunFileDemoAsync(CipherModeType mode, PaddingModeType padding, string fileName)
        {
            string? source = ResolvePath(fileName);
            if (source is null)
            {
                Console.WriteLine($"File not found: {fileName}");
                return;
            }

            string name = Path.GetFileNameWithoutExtension(source);
            string extension = Path.GetExtension(source);
            long sourceSize = new FileInfo(source).Length;

            Console.WriteLine();
            Console.WriteLine($"Mode: {mode}, padding: {padding}");

            foreach (var (block, key) in Combinations())
            {
                string tag = $"b{(int)block * BitPerByte}_k{(int)key * BitPerByte}";
                string encryptedPath = Path.Combine(_outputDir, $"{name}.{tag}.enc");
                string decryptedPath = Path.Combine( _outputDir, $"{name}.{tag}.dec{extension}");
                string title = $"block {(int)block * BitPerByte}, key {(int)key * BitPerByte}";

                try
                {
                    var context = CreateContext(block, key, mode, padding);

                    var watch = Stopwatch.StartNew();

                    await context.EncryptFileAsync(source, encryptedPath);
                    await context.DecryptFileAsync(encryptedPath, decryptedPath);

                    watch.Stop();
                    bool same = FilesEqual(source, decryptedPath);

                    Write(same ? "[ OK ]" : "[FAIL]", same ? ConsoleColor.Green : ConsoleColor.Red);

                    Console.WriteLine($" {title}: {sourceSize} -> " + $"{new FileInfo(encryptedPath).Length} -> " + $"{new FileInfo(decryptedPath).Length} bytes");
                }
                catch (Exception ex)
                {
                    Write("[FAIL]", ConsoleColor.Red);
                    Console.WriteLine($" {title}: {ex.GetType().Name}: {ex.Message}");
                }
            }
            Console.WriteLine();
        }
        private string? ResolvePath(string fileName)
        {
            if (File.Exists(fileName)) return Path.GetFullPath(fileName);

            string inTestData = Path.Combine(_testDataDir, fileName);
            return File.Exists(inTestData) ? inTestData : null;
        }
        private static IEnumerable<(RijndaelBlockSize Block, RijndaelKeySize Key)> Combinations()
        {
            return from block in Enum.GetValues<RijndaelBlockSize>() from key in Enum.GetValues<RijndaelKeySize>() select (block, key);
        }
        private static SymmetricCryptoContext<RijndaelCipher> CreateContext(RijndaelBlockSize block, RijndaelKeySize key,
            CipherModeType mode,
            PaddingModeType padding)
        {
            byte[] keyBytes = RandomBytes((int)key, 0x5A);
            byte[] iv = mode == CipherModeType.ECB ? Array.Empty<byte>() : RandomBytes((int)block, 0xC3);

            if (mode == CipherModeType.RandomDelta) iv[^1] |= 1;

            return new SymmetricCryptoContext<RijndaelCipher>(() => new RijndaelCipher(block, key, Module), keyBytes,mode, padding, iv);
        }
        private static byte[] RandomBytes(int length, int seed)
        {
            var random = new Random(seed);
            byte[] result = new byte[length];
            random.NextBytes(result);
            return result;
        }
        private static string Hex(byte[] data)=> data.Length == 0 ? "(empty)" : Convert.ToHexString(data);
        private static void Write(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
        private static bool FilesEqual(string pathA, string pathB)
        {
            using var fileA = File.OpenRead(pathA);
            using var fileB = File.OpenRead(pathB);

            if (fileA.Length != fileB.Length) return false;

            byte[] bufferA = new byte[BufSize];
            byte[] bufferB = new byte[BufSize];

            while (true)
            {
                int readA = ReadFull(fileA, bufferA);
                int readB = ReadFull(fileB, bufferB);

                if (readA != readB || !bufferA.AsSpan(0, readA).SequenceEqual(bufferB.AsSpan(0, readB))) return false;
                if (readA == 0) return true;
            }
        }
        private static int ReadFull(Stream stream, byte[] buffer)
        {
            int total = 0;

            while (total < buffer.Length)
            {
                int read = stream.Read(buffer, total, buffer.Length - total);
                if (read == 0) break;
                total += read;
            }
            return total;
        }
    }
}