using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class SubstitutionCipherTests
    {
        private const string SourceAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private const string TargetAlphabet = "zyxwvutsrqponmlkjihgfedcba"; // Reverse of the source alphabet

        [Test]
        [TestCase("Test", "Tvhg")] // Encrypts using reverse alphabet
        [TestCase("Test test", "Tvhg gvhg")] // Encrypts with spaces (no change)
        [TestCase("Hello World", "Hvool Wliow")] // Reverse alphabet encryption
        [TestCase("Encrypted", "Emxibkgvw")] // Example with custom alphabet
        public void EncryptDecrypt_String_ShouldReturnOriginal(string plainText, string expectedCipherText)
        {
            var cipher = new SubstitutionCipher(SourceAlphabet, TargetAlphabet);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new SubstitutionCipher(SourceAlphabet, TargetAlphabet);
            var plainText = String.Empty;

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(string.Empty));
            Assert.That(decrypted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new SubstitutionCipher(SourceAlphabet, TargetAlphabet);
            string plainText = null;

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        [TestCase(SourceAlphabet, TargetAlphabet, new byte[] { 65, 66, 67 }, new byte[] { 65, 66, 67 })] // Not encrypting simple byte array
        [TestCase(SourceAlphabet, TargetAlphabet, new byte[] { 97, 98, 99 }, new byte[] { 122, 121, 120 })] // Encrypting simple byte array
        [TestCase(SourceAlphabet, TargetAlphabet, new byte[] { 32, 97, 98, 99 }, new byte[] { 32, 122, 121, 120 })] // With white spaces
        [TestCase(SourceAlphabet, TargetAlphabet, new byte[] { 97, 98, 99 }, new byte[] { 122, 121, 120 })] // Simple example
        public void EncryptDecrypt_ByteArray_ShouldReturnOriginal(string sourceAlphabet, string targetAlphabet, byte[] plainBytes, byte[] expectedCipherBytes)
        {
            var cipher = new SubstitutionCipher(sourceAlphabet, targetAlphabet);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherBytes), $"Encryption error: Input={plainBytes}");
            Assert.That(decrypted, Is.EqualTo(plainBytes), $"Decryption error: Input={plainBytes}");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new SubstitutionCipher(SourceAlphabet, TargetAlphabet);
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
            var cipher = new SubstitutionCipher(SourceAlphabet, TargetAlphabet);
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void Constructor_InvalidAlphabets_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SubstitutionCipher("abc", "abcdef"));
        }

        [Test]
        public void Constructor_NullAlphabets_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SubstitutionCipher(null, TargetAlphabet));
            Assert.Throws<ArgumentNullException>(() => new SubstitutionCipher(SourceAlphabet, null));
        }
    }
}
