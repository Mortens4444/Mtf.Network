using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;
using System.Text;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class RailFenceCipherTests
    {
        [Test]
        [TestCase(2, "HELLOWORLD", "HLOOLELWRD")]
        [TestCase(3, "RAILFENCECIPHER", "RFEHALECCPEINIR")]
        [TestCase(3, "WEAREDISCOVEREDFLEEATONCE", "WECRLTEERDSOEEFEAOCAIVDEN")]
        [TestCase(5, "CRYPTOGRAPHY", "CARRPYGHPOYT")]
        public void EncryptDecrypt_String_NoSpaces_ShouldReturnOriginal(int key, string plainText, string expectedCipherText)
        {
            var cipher = new RailFenceCipher(key);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({key}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({key}), Text='{plainText}'");
        }

        [Test]
        [TestCase(3, "Hello World", "Horel ollWd")]
        [TestCase(4, "This is a test.", "Tsshi eti at.s ")]
        [TestCase(2, "A B C D E", "ABCDE    ")]
        public void EncryptDecrypt_String_WithSpacesAndSymbols_ShouldHandleWhitespaceFlag(int key, string plainText, string expectedCipherText)
        {
            var cipher = new RailFenceCipher(key);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({key}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({key}), Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new RailFenceCipher(3);

            var encrypted = cipher.Encrypt(String.Empty);
            var decrypted = cipher.Decrypt(String.Empty);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new RailFenceCipher(3);

            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void EncryptDecrypt_ByteArray_SimpleAscii_ShouldReturnOriginal()
        {
            var key = 3;
            var originalString = "Test 123";
            var plainBytes = Encoding.ASCII.GetBytes(originalString);

            var cipher = new RailFenceCipher(key);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            var decryptedString = Encoding.ASCII.GetString(decrypted);

            Assert.That(decrypted, Is.EqualTo(plainBytes), "Byte array decryption failed.");
            Assert.That(decryptedString, Is.EqualTo(originalString), "Decrypted byte array content mismatch.");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Null_ShouldReturnNull()
        {
            var cipher = new RailFenceCipher(3);
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);

            Assert.That(encrypted, Is.Null);
        }

        [Test]
        public void Decrypt_ByteArray_Null_ShouldThrow()
        {
            var cipher = new RailFenceCipher(3);
            byte[] cipherBytes = null;

            var decrypted = cipher.Decrypt(cipherBytes);

            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new RailFenceCipher(3);
            var plainBytes = Array.Empty<byte>();

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.Empty);
            Assert.That(decrypted, Is.Empty);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void Constructor_InvalidKey_ShouldThrowArgumentOutOfRangeException(int invalidKey)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RailFenceCipher(invalidKey));
        }

        [Test]
        public void Constructor_ValidKey_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => new RailFenceCipher(2));
            Assert.DoesNotThrow(() => new RailFenceCipher(10));
        }
    }
}
