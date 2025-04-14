using Mtf.Network.Interfaces;
using System;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    public class VernamCipher : ICipher
    {
        private readonly string key;

        public VernamCipher(string key)
        {
            this.key = key;
        }

        public string Encrypt(string plainText)
        {
            return Transform(plainText);
        }

        public string Decrypt(string cipherText)
        {
            return Transform(cipherText);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            return Transform(plainBytes);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return Transform(cipherBytes);
        }

        private string Transform(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            var result = new StringBuilder();
            var keyIndex = 0;

            foreach (var ch in text)
            {
                var kChar = key[keyIndex % key.Length];
                var encryptedChar = (char)(ch ^ kChar);
                result.Append(encryptedChar);
                keyIndex++;
            }

            return result.ToString();
        }

        private byte[] Transform(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            var result = new byte[input.Length];
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var keyIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                var inputByte = input[i];

                var kByte = keyBytes[keyIndex % keyBytes.Length];
                var transformed = (byte)(inputByte ^ kByte);

                result[i] = transformed;
                keyIndex++;
            }

            return result;
        }
    }
}
