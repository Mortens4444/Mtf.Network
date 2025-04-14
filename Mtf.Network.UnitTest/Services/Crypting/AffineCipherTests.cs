using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class AffineCipherTests
    {
        [Test]
        [TestCase(5, 8, "HELLOWORLD", "RCLLAOAPLX")]
        [TestCase(5, 8, "helloworld", "rcllaoaplx")]
        [TestCase(7, 11, "MixedCaseTest", "RpqngZlhnOnho")]
        [TestCase(3, 4, "Cryptography", "Kdyxjuwdexzy")]
        [TestCase(17, 20, "AnotherExample", "UhyfjkxKvuqpzk")]
        [TestCase(25, 1, "AlmostInverse", "BqpnjiTogxkjx")] // a=25 (-1 mod 26)
        public void EncryptDecrypt_ValidKey_LettersOnly_ShouldReturnOriginal(int a, int b, string plainText, string expectedCipherText)
        {
            // Arrange
            var cipher = new AffineCipher(a, b );

            // Act
            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            // Assert
            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({a},{b}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({a},{b}), Text='{plainText}'");
        }

        [Test]
        [TestCase(5, 8, "Hello World", "Rclla Oaplx")]
        [TestCase(7, 11, "Test 123!@# End.", "Onho 123!@# Nyg.")]
        [TestCase(3, 4, "  Leading and trailing spaces  ", "  Lqencrw ern jdeclcrw gxekqg  ")]
        public void EncryptDecrypt_WithSpacesAndSymbols_ShouldHandleCorrectly(int a, int b, string plainText, string expectedCipherText)
        {
            // Arrange
            var cipher = new AffineCipher(a, b);

            // Act
            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            // Assert
            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({a},{b}). Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({a},{b}). Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_EmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            var cipher = new AffineCipher(5, 8);

            // Act
            var encrypted = cipher.Encrypt(string.Empty);
            var decrypted = cipher.Decrypt(string.Empty);

            // Assert
            Assert.That(encrypted, Is.EqualTo(string.Empty));
            Assert.That(decrypted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_NullInput_ShouldReturnNull()
        {
            // Arrange
            var cipher = new AffineCipher(5, 8);

            // Act
            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            // Assert
            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void EncryptDecrypt_NonLetterInput_ShouldRemainUnchanged()
        {
            // Arrange
            var cipher = new AffineCipher(5, 8);
            var plainText = "1234567890-=!@#$%^&*()_+[]{};':\",./<>?";

            // Act
            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            // Assert
            Assert.That(encrypted, Is.EqualTo(plainText));
            Assert.That(decrypted, Is.EqualTo(plainText));
        }

        [Test]
        [TestCase(2, 5)]  // lnko(2, 26) = 2 != 1
        [TestCase(13, 1)] // lnko(13, 26) = 13 != 1
        [TestCase(26, 3)] // lnko(26, 26) = 26 != 1
        [TestCase(0, 4)]  // lnko(0, 26) -> ArgumentException-t kellene dobnia az inverz keresésnél
        public void Decrypt_InvalidKeyA_ShouldThrowArgumentException(int invalidA, int b)
        {
            // Arrange
            var cipher = new AffineCipher(invalidA, b);
            var cipherText = "CIPHERTEXT";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => cipher.Decrypt(cipherText), $"Not thrown exception when 'a' ({invalidA}) is invalid.");
        }

        [Test]
        public void Encrypt_ByteArray_ShouldThrowNotImplementedException()
        {
            // Arrange
            var cipher = new AffineCipher(5, 8);
            var data = new byte[] { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => cipher.Encrypt(data));
        }

        [Test]
        public void Decrypt_ByteArray_ShouldThrowNotImplementedException()
        {
            // Arrange
            var cipher = new AffineCipher(5, 8);
            var data = new byte[] { 10, 20, 30 };

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => cipher.Decrypt(data));
        }
    }
}