using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;
using System.Text;

namespace Mtf.Network.UnitTest.Services.Crypting
{
    [TestFixture]
    public class AtbashCipherTests
    {
        [Test]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ZYXWVUTSRQPONMLKJIHGFEDCBA", TestName = "EncryptDecrypt_LettersOnly_ShouldBeInverseAndMatchFlawedLogic ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "zyxwvutsrqponmlkjihgfedcba", TestName = "EncryptDecrypt_LettersOnly_ShouldBeInverseAndMatchFlawedLogic abcdefghijklmnopqrstuvwxyz")]
        [TestCase("MixedCaseTest", "NrcvwXzhvGvhg", TestName = "EncryptDecrypt_LettersOnly_ShouldBeInverseAndMatchFlawedLogic MixedCaseTest")]
        public void EncryptDecrypt_LettersOnly_ShouldBeInverseAndMatchFlawedLogic(string plainText, string expectedCipherTextFlawed)
        {
            var cipher = new AtbashCipher();

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherTextFlawed), $"Encryption (with flawed formula) did not produce expected result: Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), "Decryption (double encryption) did not return original text.");
        }

        [Test]
        [TestCase("Hello World", "Svool Dliow", TestName = "EncryptDecrypt_WithSpacesAndSymbols_ShouldHandleCorrectly Hello World")]
        [TestCase("Test 123!@# End.", "Gvhg 123!@# Vmw.", TestName = "EncryptDecrypt_WithSpacesAndSymbols_ShouldHandleCorrectly Test 123!@# End.")]
        public void EncryptDecrypt_WithSpacesAndSymbols_ShouldHandleCorrectly(string plainText, string expectedCipherTextFlawed)
        {
            var cipher = new AtbashCipher();

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedCipherTextFlawed), $"Encryption (with flawed formula) failed. Text='{plainText}'");
            Assert.That(decrypted, Is.EqualTo(plainText), $"Decryption (double encryption) failed. Text='{plainText}'");
        }

        [Test]
        public void EncryptDecrypt_EmptyString_ShouldReturnEmptyString()
        {
            var cipher = new AtbashCipher();

            var encrypted = cipher.Encrypt(String.Empty);
            var decrypted = cipher.Decrypt(String.Empty);

            Assert.That(encrypted, Is.EqualTo(String.Empty));
            Assert.That(decrypted, Is.EqualTo(String.Empty));
        }

        [Test]
        public void EncryptDecrypt_NullInput_ShouldReturnNull()
        {
            var cipher = new AtbashCipher();

            var encrypted = cipher.Encrypt((string)null);
            var decrypted = cipher.Decrypt((string)null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void EncryptDecrypt_NonLetterInput_ShouldRemainUnchanged()
        {
            var cipher = new AtbashCipher();
            var plainText = "1234567890-=!@#$%^&*()_+[]{};':\",./<>?";

            var encrypted = cipher.Encrypt(plainText);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(plainText));
            Assert.That(decrypted, Is.EqualTo(plainText));
        }

        [Test]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ZYXWVUTSRQPONMLKJIHGFEDCBA", TestName = "EncryptDecryptBytes_UpperCase_ShouldBeInverse")]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "zyxwvutsrqponmlkjihgfedcba", TestName = "EncryptDecryptBytes_LowerCase_ShouldBeInverse")]
        [TestCase("MixedCaseTest", "NrcvwXzhvGvhg", TestName = "EncryptDecryptBytes_MixedCase_ShouldBeInverse")]
        [TestCase("Hello World 123!", "Svool Dliow 123!", TestName = "EncryptDecryptBytes_WithNonLetters_ShouldHandleCorrectly")]
        public void EncryptDecrypt_Bytes_ShouldBeInverseAndHandleCorrectly(string plainText, string expectedCipherText)
        {
            var cipher = new AtbashCipher();
            
            var plainBytes = Encoding.ASCII.GetBytes(plainText);
            var expectedCipherBytes = Encoding.ASCII.GetBytes(expectedCipherText);

            var encryptedBytes = cipher.Encrypt(plainBytes);
            var decryptedBytes = cipher.Decrypt(encryptedBytes);

            Assert.That(encryptedBytes, Is.EqualTo(expectedCipherBytes), $"Byte Encryption did not produce expected result: Text='{plainText}'");
            Assert.That(decryptedBytes, Is.EqualTo(plainBytes), "Byte Decryption (double encryption) did not return original bytes.");
        }

        [Test]
        public void EncryptDecrypt_Bytes_EmptyArray_ShouldReturnEmptyArray()
        {
            var cipher = new AtbashCipher();
            var emptyBytes = new byte[0];

            var encrypted = cipher.Encrypt(emptyBytes);
            var decrypted = cipher.Decrypt(emptyBytes);

            Assert.That(encrypted, Is.EqualTo(emptyBytes));
            Assert.That(decrypted, Is.EqualTo(emptyBytes));
            Assert.That(encrypted, Is.Not.SameAs(emptyBytes));
        }

        [Test]
        public void EncryptDecrypt_Bytes_NullInput_ShouldReturnNull()
        {
            var cipher = new AtbashCipher();

            var encrypted = cipher.Encrypt((byte[])null);
            var decrypted = cipher.Decrypt((byte[])null);

            Assert.That(encrypted, Is.Null);
            Assert.That(decrypted, Is.Null);
        }

        [Test]
        public void EncryptDecrypt_Bytes_NonAsciiLetters_ShouldRemainUnchanged()
        {
            var cipher = new AtbashCipher();
            var plainBytes = new byte[] { 0, 1, 10, 64, 91, 96, 123, 127, 128, 255 };
            var expectedBytes = new byte[] { 0, 1, 10, 64, 91, 96, 123, 127, 128, 255 };

            var encrypted = cipher.Encrypt(plainBytes);
            var decrypted = cipher.Decrypt(encrypted);

            Assert.That(encrypted, Is.EqualTo(expectedBytes));
            Assert.That(decrypted, Is.EqualTo(plainBytes));
        }
    }
}