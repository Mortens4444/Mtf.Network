using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class VigenereCipherTests
    {
        [Test]
        [TestCase("KEY", "Hello World!", "\u0093ªÅ·´y¢´Ë·©z", TestName = "EncryptDecrypt_String_ShouldReturnOriginal Hello World!")]
        [TestCase("KEY", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "\u008c\u0087\u009c\u008f\u008a\u009f\u0092\u008d¢\u0095\u0090¥\u0098\u0093¨\u009b\u0096«\u009e\u0099®¡\u009c±¤\u009f", TestName = "EncryptDecrypt_String_ShouldReturnOriginal ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("KEY", "abcdefghijklmnopqrstuvwxyz", "¬§¼¯ª¿²­Âµ°Å¸³È»¶Ë¾¹ÎÁ¼ÑÄ¿", TestName = "EncryptDecrypt_String_ShouldReturnOriginal abcdefghijklmnopqrstuvwxyz")]
        [TestCase("KEY", "ABC Def", "\u008c\u0087\u009ck\u0089¾±", TestName = "EncryptDecrypt_String_ShouldReturnOriginal ABC Def")]
        [TestCase("KEY", "NoChange 123.", "\u0099´\u009c³¦Ç²ªy|w\u008cy", TestName = "EncryptDecrypt_String_ShouldReturnOriginal NoChange 123")]
        [TestCase("KEY", "", "", TestName = "EncryptDecrypt_String_ShouldReturnOriginal")]
        public void EncryptDecrypt_String_ShouldReturnOriginal(string key, string plainText, string expectedCipherText)
        {
            var cipher = new VigenereCipher(key);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({key}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({key}), Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new VigenereCipher("KEY");

            var encrypted = cipher.Encrypt(String.Empty);
            var decrypted = cipher.Decrypt(String.Empty);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new VigenereCipher("KEY");

            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        [TestCase("KEY", new byte[] { 65, 66, 32, 97, 98 }, new byte[] { 140, 135, 121, 172, 167 })]  // "AB ab", ws=true -> "RI JV SV"
        [TestCase("KEY", new byte[] { 0, 1, 254, 255 }, new byte[] { 75, 70, 87, 74 })] // Wrap around low and high
        [TestCase("KEY", new byte[] { 0, 1, 2, 3 }, new byte[] { 75, 70, 91, 78 })] // Same logic for encryption
        [TestCase("KEY", new byte[] { 10, 20, 30, 40 }, new byte[] { 85, 89, 119, 115 })] // Zero shift
        public void EncryptDecrypt_ByteArray_ShouldReturnOriginal(string key, byte[] plainBytes, byte[] expectedCipherBytes)
        {
            var cipher = new VigenereCipher(key);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherBytes), $"Encryption error: Key({key})");
            Assert.That(decrypted, Is.EqualTo(plainBytes), $"Decryption error: Key({key})");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new VigenereCipher("KEY");
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
            var cipher = new VigenereCipher("KEY");
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(plainBytes); // Decrypting null should also return null

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }
    }
}
