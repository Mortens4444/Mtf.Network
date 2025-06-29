using Mtf.Network.Services.Crypting;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Mtf.Network.UnitTest
{
    [TestFixture]
    public class RsaCipherTests
    {
        private RSAParameters rsaParameters;
        private RsaCipher rsaCipher;

        [SetUp]
        public void SetUp()
        {
            // Generate RSA keys for testing
            using (var rsa = RSA.Create())
            {
                rsaParameters = rsa.ExportParameters(true);
            }

            rsaCipher = new RsaCipher(rsaParameters);
        }

        [Test]
        public void Encrypt_Decrypt_ValidInput_ShouldReturnSameResultWithFileKeys()
        {
            var keyFilePath = "key.xml";
            if (!File.Exists(keyFilePath))
            {
                using (var rsaInstance = new RSACng { KeySize = 2048 })
                {
                    var xml = rsaInstance.ToXmlString(true);
                    File.WriteAllText(keyFilePath, xml);
                }
            }

            var cipher = new RsaCipher(keyFilePath);
            var originalText = "hello";
            var encrypted = cipher.Encrypt(originalText);
            var decrypted = cipher.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(originalText));
        }

        [Test]
        public void Encrypt_Decrypt_ValidInput_ShouldReturnSameResult()
        {
            var originalText = "This is a test message!";
            var encryptedText = rsaCipher.Encrypt(originalText);
            var decryptedText = rsaCipher.Decrypt(encryptedText);

            Assert.That(decryptedText, Is.EqualTo(originalText));
        }

        [Test]
        public void Encrypt_NullInput_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => rsaCipher.Encrypt((string)null));
            Assert.Throws<ArgumentNullException>(() => rsaCipher.Encrypt((byte[])null));
        }

        [Test]
        public void Decrypt_NullInput_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => rsaCipher.Decrypt((string)null));
            Assert.Throws<ArgumentNullException>(() => rsaCipher.Decrypt((byte[])null));
        }

        [Test]
        public void Decrypt_InvalidBase64Input_ShouldThrowFormatException()
        {
            var invalidBase64 = "invalidBase64";
            Assert.Throws<FormatException>(() => rsaCipher.Decrypt(invalidBase64));
        }

        [Test]
        public void Decrypt_WithoutPrivateKey_ShouldThrowInvalidOperationException()
        {
            var rsaParametersWithoutPrivateKey = new RSAParameters
            {
                Modulus = rsaParameters.Modulus,
                Exponent = rsaParameters.Exponent
            };

            var rsaCipherWithoutPrivateKey = new RsaCipher(rsaParametersWithoutPrivateKey);
            var encryptedText = rsaCipher.Encrypt("Test");

            Assert.Throws<InvalidOperationException>(() => rsaCipherWithoutPrivateKey.Decrypt(encryptedText));
        }

        [Test]
        public void EncryptDecrypt_WithOaepPadding_ShouldReturnSameResult()
        {
            var originalText = "This is a test message!";
            var rsaCipherWithOaep = new RsaCipher(rsaParameters, useOaepPadding: true);

            var encryptedText = rsaCipherWithOaep.Encrypt(originalText);
            var decryptedText = rsaCipherWithOaep.Decrypt(encryptedText);

            Assert.That(decryptedText, Is.EqualTo(originalText));
        }

        [Test]
        public void EncryptDecrypt_WithPkcs1Padding_ShouldReturnSameResult()
        {
            var originalText = "This is a test message!";
            var rsaCipherWithPkcs1 = new RsaCipher(rsaParameters, useOaepPadding: false);

            var encryptedText = rsaCipherWithPkcs1.Encrypt(originalText);
            var decryptedText = rsaCipherWithPkcs1.Decrypt(encryptedText);

            Assert.That(decryptedText, Is.EqualTo(originalText));
        }

        [TearDown]
        public void TearDown()
        {
            rsaCipher.Dispose();
        }
    }
}
