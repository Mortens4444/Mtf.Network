﻿using Mtf.Network.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    /// <summary>
    /// Implements the ICipher interface using standard RSA encryption with OAEP padding.
    /// This approach encrypts data in blocks and is secure for practical use,
    /// unlike byte-by-byte textbook RSA.
    /// </summary>
    public class RsaCipher : ICipher, IDisposable
    {
        private RSA rsaInstance;
        private readonly bool canDecrypt;
        private readonly RSAEncryptionPadding padding;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the RsaCipher class using provided RSA parameters.
        /// Handles both public-only (for encryption) and public/private keys.
        /// </summary>
        /// <param name="parameters">The RSA parameters (containing Modulus, Exponent, and optionally D, P, Q etc.).</param>
        /// <param name="useOaepPadding">True to use OAEP padding (recommended, SHA256 hash), false to use PKCS#1 v1.5 padding.</param>
        public RsaCipher(RSAParameters parameters, bool useOaepPadding = true)
        {
            // Ellenőrizzük az alapvető szükséges paramétereket
            if (parameters.Modulus == null || parameters.Exponent == null)
            {
                throw new ArgumentException("RSA parameters must include at least Modulus and Exponent.", nameof(parameters));
            }

            rsaInstance = new RSACng();
            //rsaInstance = RSA.Create();
            try
            {
                rsaInstance.ImportParameters(parameters);
            }
            catch (CryptographicException ex)
            {
                rsaInstance.Dispose(); // Takarítás hiba esetén
                throw new ArgumentException("Invalid RSA parameters provided.", nameof(parameters), ex);
            }

            // Eldöntjük, tudunk-e dekódolni a privát kulcs jelenléte alapján
            // A 'D' a privát exponens, ami szükséges.
            canDecrypt = parameters.D != null;

            // Padding beállítása
            padding = useOaepPadding ? RSAEncryptionPadding.OaepSHA256 : RSAEncryptionPadding.Pkcs1;
        }

        /// <summary>
        /// Encrypts plain bytes using RSA with the configured padding scheme.
        /// The underlying RSA implementation handles data blocking automatically.
        /// </summary>
        /// <param name="plainBytes">The bytes to encrypt.</param>
        /// <returns>The encrypted bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if plainBytes is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the cipher instance has been disposed.</exception>
        /// <exception cref="CryptographicException">Thrown if encryption fails (e.g., message too long for key size and padding).</exception>
        public byte[] Encrypt(byte[] plainBytes)
        {
            if (plainBytes == null) throw new ArgumentNullException(nameof(plainBytes));
            if (rsaInstance == null) throw new ObjectDisposedException(nameof(RsaCipher));

            try
            {
                // Az Encrypt metódus kezeli a paddinget és a blokkokra bontást
                return rsaInstance.Encrypt(plainBytes, padding);
            }
            catch (CryptographicException ex)
            {
                // További kontextust adhatunk a hibához
                // Pl. ellenőrizhetjük, hogy az adat mérete nem túl nagy-e a kulcshoz képest
                // int maxDataLength = CalculateMaxDataLength();
                // if (plainBytes.Length > maxDataLength) ...
                throw new CryptographicException($"Encryption failed. Ensure data length ({plainBytes.Length} bytes) is appropriate for the key size ({rsaInstance.KeySize} bits) and padding '{padding.Mode}'. Inner exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts cipher bytes using RSA with the configured padding scheme.
        /// </summary>
        /// <param name="cipherBytes">The encrypted bytes.</param>
        /// <returns>The decrypted plain bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if cipherBytes is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the cipher instance has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the private key is not available for decryption.</exception>
        /// <exception cref="CryptographicException">Thrown if decryption fails (e.g., key mismatch, invalid padding, data corruption).</exception>
        public byte[] Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null) throw new ArgumentNullException(nameof(cipherBytes));
            if (rsaInstance == null) throw new ObjectDisposedException(nameof(RsaCipher));
            if (!canDecrypt)
            {
                throw new InvalidOperationException("Decryption requires the private key, which was not provided or imported.");
            }

            try
            {
                // A Decrypt metódus kezeli a padding eltávolítását.
                // Kivételt dob, ha a padding érvénytelen (pl. rossz kulcs vagy sérült adat).
                return rsaInstance.Decrypt(cipherBytes, padding);
            }
            catch (CryptographicException ex)
            {
                // A dekódolási hiba gyakran rossz kulcsra, sérült adatra vagy rossz padding módra utal.
                throw new CryptographicException($"Decryption failed. Ensure the correct private key and padding mode ('{padding.Mode}') were used, and the data is not corrupted. Inner exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Encrypts a string using UTF8 encoding, standard RSA encryption, and Base64 encoding for the output string.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <returns>A Base64 encoded string representing the encrypted data.</returns>
        public string Encrypt(string plainText)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = Encrypt(plainBytes); // Kezeli a kivételeket
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// Decrypts a Base64 encoded string that was encrypted using the corresponding Encrypt method.
        /// </summary>
        /// <param name="cipherText">The Base64 encoded encrypted string.</param>
        /// <returns>The decrypted plain text string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if cipherText is null.</exception>
        /// <exception cref="FormatException">Thrown if cipherText is not a valid Base64 string.</exception>
        /// <exception cref="CryptographicException">Thrown if decryption fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the private key is not available.</exception>
        public string Decrypt(string cipherText)
        {
            if (cipherText == null) throw new ArgumentNullException(nameof(cipherText));
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = Decrypt(cipherBytes); // Kezeli a kivételeket
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (FormatException ex) // A FromBase64String dobhatja
            {
                throw new FormatException("Invalid Base64 string provided for decryption.", ex);
            }
            // CryptographicException és InvalidOperationException átdobódik a Decrypt(byte[])-ból.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    rsaInstance?.Dispose();
                    rsaInstance = null;
                }
                disposed = true;
            }
        }

        ~RsaCipher()
        {
            Dispose(false);
        }
    }
}
