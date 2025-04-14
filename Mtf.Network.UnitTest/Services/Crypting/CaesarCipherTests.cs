using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class CaesarCipherTests
    {
        [Test]
        [TestCase(3, "Hello World!", "Khoor#Zruog$")]  // Space and ! shifted (assuming ASCII/UTF8)
        [TestCase(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "BCDEFGHIJKLMNOPQRSTUVWXYZ[")]
        [TestCase(1, "abcdefghijklmnopqrstuvwxyz", "bcdefghijklmnopqrstuvwxyz{")]
        [TestCase(25, "ABC Def", "Z[\\9]~\u007f", TestName = "EncryptDecrypt_String_ShouldReturnOriginal ABC Def")] // Shift -1 equivalent
        [TestCase(0, "NoChange 123.", "NoChange 123.")]
        [TestCase(258, "Test", "Vguv")] // Shift 2 equivalent (258 % 256 = 2)
        public void EncryptDecrypt_String_ShouldReturnOriginal(int shift, string plainText, string expectedCipherText)
        {
            var cipher = new CaesarCipher(shift);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Shift({shift}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Shift({shift}), Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new CaesarCipher(5);

            var encrypted = cipher.Encrypt(String.Empty);
            var decrypted = cipher.Decrypt(String.Empty);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new CaesarCipher(5);

            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        [TestCase(5, new byte[] { 65, 66, 32, 97, 98 }, new byte[] { 70, 71, 37, 102, 103 })]  // "AB ab", ws=true -> "FG%fg" (space shifted)
        [TestCase(3, new byte[] { 0, 1, 254, 255 }, new byte[] { 3, 4, 1, 2 })] // Wrap around low and high
        [TestCase(254, new byte[] { 0, 1, 2, 3 }, new byte[] { 254, 255, 0, 1 })] // Shift -2 equivalent
        [TestCase(0, new byte[] { 10, 20, 30, 40 }, new byte[] { 10, 20, 30, 40 })] // Zero shift
        public void EncryptDecrypt_ByteArray_ShouldReturnOriginal(int shift, byte[] plainBytes, byte[] expectedCipherBytes)
        {
            var cipher = new CaesarCipher(shift);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherBytes), $"Encryption error: Shift({shift})");
            Assert.That(decrypted, Is.EqualTo(plainBytes), $"Decryption error: Shift({shift})");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new CaesarCipher(5);
            var plainBytes = new byte[0];

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(plainBytes)); // Should be empty
            Assert.That(decrypted, Is.EqualTo(plainBytes)); // Should be empty
            Assert.That(encrypted, Is.Empty);
            Assert.That(decrypted, Is.Empty);
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Null_ShouldReturnNull()
        {
            var cipher = new CaesarCipher(5);
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(plainBytes); // Decrypting null should also return null

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }
    }
}