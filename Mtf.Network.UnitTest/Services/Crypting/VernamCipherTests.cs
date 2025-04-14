using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class VernamCipherTests
    {
        [Test]
        [TestCase("KEY", "Hello World!", "\u0003 5'*y\u001c*+'!x", TestName = "EncryptDecrypt_String_ShouldReturnOriginal KEY")]
        [TestCase("ABC", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "\0\0\0\u0005\a\u0005\u0006\n\n\v\t\u000f\f\f\f\u0011\u0013\u0011\u0012\u0016\u0016\u0017\u0015\u001b\u0018\u0018", TestName = "EncryptDecrypt_String_ShouldReturnOriginal ABC")]
        [TestCase("DEF", "abcdefghijklmnopqrstuvwxyz", "%'%   #-/..*)+)44471322>=?")]
        [TestCase("ABCD", "NoChange 123.", "\u000f-\0, ,$!asqwo", TestName = "EncryptDecrypt_String_ShouldReturnOriginal ABCD")]
        [TestCase("Z", "Test", "\u000e?).", TestName = "EncryptDecrypt_String_ShouldReturnOriginal Z")]
         public void EncryptDecrypt_String_ShouldReturnOriginal(string key, string plainText, string expectedCipherText)
        {
            var cipher = new VernamCipher(key);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Key({key}), Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Key({key}), Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new VernamCipher("KEY");

            var encrypted = cipher.Encrypt(String.Empty);
            var decrypted = cipher.Decrypt(String.Empty);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new VernamCipher("KEY");

            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        [TestCase("KEY", new byte[] { 64, 66, 32, 97, 98 }, new byte[] { 11, 7, 121, 42, 39 })] // "AB ab", ws=false
        [TestCase("KEY", new byte[] { 65, 66, 32, 97, 98 }, new byte[] { 10, 7, 121, 42, 39 })]  // "AB ab", ws=true
        [TestCase("ABC", new byte[] { 0, 1, 254, 255 }, new byte[] { 65, 67, 189, 190 })] // Wrap around low and high
        [TestCase("DEF", new byte[] { 0, 1, 2, 3 }, new byte[] { 68, 68, 68, 71 })] // Shift with different key
        [TestCase("KEY", new byte[] { 10, 20, 30, 40 }, new byte[] { 65, 81, 71, 99 })] // Example
        public void EncryptDecrypt_ByteArray_ShouldReturnOriginal(string key, byte[] plainBytes, byte[] expectedCipherBytes)
        {
            var cipher = new VernamCipher(key);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherBytes), $"Encryption error: Key({key})");
            Assert.That(decrypted, Is.EqualTo(plainBytes), $"Decryption error: Key({key})");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new VernamCipher("KEY");
            var plainBytes = Array.Empty<byte>();

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
            var cipher = new VernamCipher("KEY");
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(plainBytes); // Decrypting null should also return null

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }
    }
}
