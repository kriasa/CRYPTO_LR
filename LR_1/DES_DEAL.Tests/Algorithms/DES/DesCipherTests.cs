using System;
using System.Linq;
using DES_DEAL.Algorithms.DES;
using Xunit;

namespace DES_DEAL.Tests.Algorithms.DES
{
    public class DesCipherTests
    {
        /// <summary>
        /// Вспомогательный метод для перевода удобной HEX-строки (например "0123456789ABCDEF") 
        /// в массив байтов. Это стандартный прием в криптографических тестах.
        /// </summary>
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                             .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                             .ToArray();
        }

        // Используем Theory вместо Fact. Это позволяет передавать данные прямо в параметры метода.
        // Формат: [Открытый текст], [Ключ], [Ожидаемый Шифротекст]
        [Theory]
        // 1. Классический тестовый вектор NIST
        [InlineData("0123456789ABCDEF", "133457799BBCDFF1", "85E813540F0AB405")]
        // 2. Тест с "нулевым" открытым текстом и ключом
        [InlineData("0000000000000000", "0000000000000000", "8CA64DE9C1B123A7")]
        // 3. Тест с "единичным" открытым текстом и ключом (все биты = 1)
        [InlineData("FFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFF", "7359B2163E4EDC58")]
        public void DesCipher_EncryptAndDecrypt_MatchesOfficialNistVectors(
            string plainTextHex, string keyHex, string expectedCipherHex)
        {
            // Arrange (Подготовка)
            byte[] plainText = StringToByteArray(plainTextHex);
            byte[] key = StringToByteArray(keyHex);
            byte[] expectedCipher = StringToByteArray(expectedCipherHex);

            var cipher = new DesCipher();
            cipher.SetKey(key);

            // Act (Шифрование)
            byte[] actualCipher = cipher.Encrypt(plainText);

            // Assert (Проверка шифрования)
            // Если этот Assert проходит, значит наш BitPermutator, S-блоки и Сеть Фейстеля
            // работают АБСОЛЮТНО ИДЕНТИЧНО мировому стандарту!
            Assert.Equal(expectedCipher, actualCipher);

            // Act (Дешифрование)
            byte[] decryptedText = cipher.Decrypt(actualCipher);

            // Assert (Проверка дешифрования)
            // Убеждаемся, что алгоритм обратим и мы вернули оригинальный текст
            Assert.Equal(plainText, decryptedText);
        }

        [Fact]
        public void Encrypt_InvalidBlockLength_ThrowsArgumentException()
        {
            var cipher = new DesCipher();
            cipher.SetKey(new byte[8]); // Ставим любой валидный ключ

            byte[] invalidBlock = new byte[7]; // 7 байт вместо 8

            Assert.Throws<ArgumentException>(() => cipher.Encrypt(invalidBlock));
            Assert.Throws<ArgumentException>(() => cipher.Decrypt(invalidBlock));
        }

        [Fact]
        public void SetKey_InvalidKeyLength_ThrowsException()
        {
            var cipher = new DesCipher();
            byte[] invalidKey = new byte[10]; // 10 байт вместо 8

            // Ошибка вылетит из недр DesKeyExpansion
            Assert.Throws<ArgumentException>(() => cipher.SetKey(invalidKey));
        }
    }
}