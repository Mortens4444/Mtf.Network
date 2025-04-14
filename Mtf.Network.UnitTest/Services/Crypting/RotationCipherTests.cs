using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class RotationCipherTests
    {
        // Test cases for encryption and decryption with string inputs
        [Test]
        [TestCase("Rotation", "otationR", 1)] // Rotate left by 1
        [TestCase("Rotation", "nRotatio", -1)] // Rotate right by 1
        [TestCase("Rotation Cipher", "otation CipherR", 1)] // Rotate left by 1
        [TestCase("Rotation Cipher", "rRotation Ciphe", -1)] // Rotate right by 1
        [TestCase("123 456", "123 456", 0)] // No shift
        public void EncryptDecrypt_String_ShouldReturnOriginal(string plainText, string expectedCipherText, int shift)
        {
            var cipher = new RotationCipher(shift);

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherText), $"Encryption error: Text='{plainText}' with shift={shift}");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption error: Text='{plainText}' with shift={shift}");
        }

        [Test]
        public void EncryptDecrypt_String_Empty_ShouldReturnEmpty()
        {
            var cipher = new RotationCipher(1);
            var plainText = String.Empty;

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_Null_ShouldReturnNull()
        {
            var cipher = new RotationCipher(1);
            string plainText = null;

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        // Test cases for encryption and decryption with byte array inputs
        [Test]
        [TestCase(new byte[] { 65, 66, 67 }, new byte[] { 66, 67, 65 }, 1)] // Rotate left by 1
        [TestCase(new byte[] { 65, 66, 67, 68 }, new byte[] { 68, 65, 66, 67 }, -1)] // Rotate right by 1
        [TestCase(new byte[] { 49, 50, 51, 32, 52, 53 }, new byte[] { 49, 50, 51, 32, 52, 53 }, 0)] // No shift
        public void EncryptDecrypt_ByteArray_ShouldReturnOriginal(byte[] plainBytes, byte[] expectedCipherBytes, int shift)
        {
            var cipher = new RotationCipher(shift);

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherBytes), $"Encryption error: Bytes={BitConverter.ToString(plainBytes)} with shift={shift}");
            Assert.That(decrypted, Is.EqualTo(plainBytes), $"Decryption error: Bytes={BitConverter.ToString(plainBytes)} with shift={shift}");
        }

        [Test]
        public void EncryptDecrypt_ByteArray_Empty_ShouldReturnEmpty()
        {
            var cipher = new RotationCipher(1);
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
            var cipher = new RotationCipher(1);
            byte[] plainBytes = null;

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void Constructor_InvalidShift_ShouldNotThrowException()
        {
            // It should not throw an exception when shifting by 0 or negative shift
            Assert.DoesNotThrow(() => new RotationCipher(0));
            Assert.DoesNotThrow(() => new RotationCipher(-1));
        }

        [Test]
        public void Constructor_DefaultRotateLeft_ShouldNotThrowException()
        {
            // Check that the default constructor with rotateLeft == true works fine
            Assert.DoesNotThrow(() => new RotationCipher(1));
        }

        [Test]
        public void EncryptDecrypt_String_WithWhiteSpace_ShouldRespectCryptWhiteSpacesFlag()
        {
            var cipher = new RotationCipher(1, cryptWhiteSpaces: false);
            var input = "Test 123";
            var expectedEncrypted = "est1 23T"; // Spaces should not rotate

            var encrypted = cipher.Encrypt(input);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedEncrypted), $"Encryption failed with input: {input}");
            Assert.That(decrypted, Is.EqualTo(input), $"Decryption failed with input: {input}");
        }
    }
}
