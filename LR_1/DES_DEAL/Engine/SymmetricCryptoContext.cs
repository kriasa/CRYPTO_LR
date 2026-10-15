using System;
using System.IO;
using System.Threading.Tasks;
using DES_DEAL.Core.Enums;
using DES_DEAL.Core.Interfaces;
using DES_DEAL.Engine.CipherModes;
using DES_DEAL.Engine.Padding;

namespace DES_DEAL.Engine
{
    public sealed class SymmetricCryptoContext<TCipher> where TCipher : ISymmetricCipher, new()
    {
        private const int TargetChunkSizeBytes = 1024 * 1024;
        private const int FileStreamBufferSize = 81920;
        private readonly TCipher _cipher;
        private readonly ICipherModeStrategy _cipherModeStrategy;
        private readonly IPaddingStrategy _paddingStrategy;
        private readonly byte[] _iv;
        private readonly object[] _extraParams;
        private readonly bool _requiresIv;
        private readonly int _chunkSizeBytes;
        public SymmetricCryptoContext(byte[] key, CipherModeType cipherMode, PaddingModeType paddingMode, byte[]? iv = null, params object[] extraParams)
        {
            ArgumentNullException.ThrowIfNull(key);
            _cipher = new TCipher();
            _cipher.SetKey(key);
            _cipherModeStrategy = StrategyFactory.CreateCipherMode(cipherMode);
            _paddingStrategy = StrategyFactory.CreatePadding(paddingMode);

            int blockSize = _cipher.BlockSize;

            _requiresIv = StrategyFactory.RequiresIv(cipherMode);
            _chunkSizeBytes = (Math.Max(1, TargetChunkSizeBytes / blockSize)) * blockSize;

            if (_requiresIv)
            {
                ArgumentNullException.ThrowIfNull(iv);
                if (iv.Length != blockSize) throw new ArgumentException("len(IV) = len(block)", nameof(iv));
            }
            _iv = iv!;
            _extraParams = extraParams ?? Array.Empty<object>();
        }
        public void Encrypt(byte[] data, out byte[] res) => res = EncryptCore(data);
        public void Decrypt(byte[] data, out byte[] res) => res = DecryptCore(data);
        public Task<byte[]> EncryptAsync(byte[] data) => Task.Run(() => EncryptCore(data));
        public Task<byte[]> DecryptAsync(byte[] data) => Task.Run(() => DecryptCore(data));
        public void EncryptFile(string inputFilePath, string outputFilePath) => EncryptFileAsync(inputFilePath, outputFilePath).GetAwaiter().GetResult();

        public void DecryptFile(string inputFilePath, string outputFilePath) => DecryptFileAsync(inputFilePath, outputFilePath).GetAwaiter().GetResult();
        public async Task EncryptFileAsync(string inputFilePath, string outputFilePath)
        {
            ValidateFilePaths(inputFilePath, outputFilePath);

            await using var input = OpenRead(inputFilePath);
            await using var output = OpenWrite(outputFilePath);

            long totalLength = input.Length;
            byte[] runningIv = _iv;
            long blockIndexOffset = 0;
            long position = 0;
            int blockSize = _cipher.BlockSize;

            while (position < totalLength)
            {
                int toRead = (int)Math.Min(_chunkSizeBytes, totalLength - position);
                byte[] chunk = await ReadExactAsync(input, toRead).ConfigureAwait(false);
                position += chunk.Length;
                bool isLastChunk = position >= totalLength;

                byte[] toProcess = isLastChunk ? _paddingStrategy.Pad(chunk, blockSize) : chunk;

                byte[] processed = await Task.Run(() => _cipherModeStrategy.Encrypt(
                        toProcess, _cipher, runningIv, _extraParams, blockIndexOffset))
                    .ConfigureAwait(false);

                await output.WriteAsync(processed.AsMemory()).ConfigureAwait(false);

                AdvanceState(toProcess.Length, toProcess, processed, isEncrypting: true,
                    ref runningIv, ref blockIndexOffset);
            }

            if (totalLength == 0)
            {
                byte[] toProcess = _paddingStrategy.Pad(Array.Empty<byte>(), blockSize);
                byte[] processed = _cipherModeStrategy.Encrypt(
                    toProcess, _cipher, runningIv, _extraParams, blockIndexOffset);
                await output.WriteAsync(processed.AsMemory()).ConfigureAwait(false);
            }
        }
        public async Task DecryptFileAsync(string inputFilePath, string outputFilePath)
        {
            ValidateFilePaths(inputFilePath, outputFilePath);

            await using var input = OpenRead(inputFilePath);
            int blockSize = _cipher.BlockSize;

            long totalLength = input.Length;
            if (totalLength == 0 || totalLength % blockSize != 0)
                throw new InvalidPaddingException("len(C_file) % len(block) != 0");

            await using var output = OpenWrite(outputFilePath);

            byte[] runningIv = _iv;
            long blockIndexOffset = 0;
            long position = 0;

            while (position < totalLength)
            {
                int toRead = (int)Math.Min(_chunkSizeBytes, totalLength - position);
                byte[] chunk = await ReadExactAsync(input, toRead).ConfigureAwait(false);
                position += chunk.Length;
                bool isLastChunk = position >= totalLength;

                byte[] decrypted = await Task.Run(() => _cipherModeStrategy.Decrypt(
                        chunk, _cipher, runningIv, _extraParams, blockIndexOffset))
                    .ConfigureAwait(false);

                byte[] toWrite = isLastChunk ? _paddingStrategy.Unpad(decrypted, blockSize) : decrypted;
                await output.WriteAsync(toWrite.AsMemory()).ConfigureAwait(false);

                AdvanceState(chunk.Length, chunk, decrypted, isEncrypting: false,
                    ref runningIv, ref blockIndexOffset);
            }
        }
        private void AdvanceState(int processedByteLength, byte[] inputChunk, byte[] outputChunk, bool isEncrypting, ref byte[] runningIv, ref long blockIndexOffset)
        {
            int blockSize = _cipher.BlockSize;
            if (_cipherModeStrategy.IsCounterBased)
            {
                blockIndexOffset += processedByteLength / blockSize;
            }
            else if (_requiresIv)
            {
                runningIv = _cipherModeStrategy.DeriveNextChainValue(inputChunk, outputChunk, isEncrypting, _cipher);
            }
        }
        private byte[] EncryptCore(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            int blockSize = _cipher.BlockSize;
            byte[] paddedData = _paddingStrategy.Pad(data, blockSize);
            return _cipherModeStrategy.Encrypt(paddedData, _cipher, _iv, _extraParams);
        }

        private byte[] DecryptCore(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            int blockSize = _cipher.BlockSize;
            byte[] decryptedWithPadding = _cipherModeStrategy.Decrypt(data, _cipher, _iv, _extraParams);
            return _paddingStrategy.Unpad(decryptedWithPadding, blockSize);
        }

        private static void ValidateFilePaths(string inputFilePath, string outputFilePath)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath)) throw new ArgumentException("empty path", nameof(inputFilePath));
            if (string.IsNullOrWhiteSpace(outputFilePath)) throw new ArgumentException("empty path", nameof(outputFilePath));
        }

        private static FileStream OpenRead(string path) => new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            FileStreamBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        private static FileStream OpenWrite(string path) => new FileStream(
            path, FileMode.Create, FileAccess.Write, FileShare.None,
            FileStreamBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        private static async Task<byte[]> ReadExactAsync(FileStream stream, int count)
        {
            var buffer = new byte[count];
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(totalRead, count - totalRead)).ConfigureAwait(false);
                if (read == 0) throw new IOException("the file ended earlier");
                totalRead += read;
            }

            return buffer;
        }
    }
}