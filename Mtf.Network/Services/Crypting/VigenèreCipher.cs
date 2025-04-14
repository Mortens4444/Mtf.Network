using Mtf.Network.Interfaces;
using System;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    public class VigenereCipher : ICipher
    {
        private const int alphabetSize = 256;

        private readonly string key;

        public VigenereCipher(string key)
        {
            this.key = key;
        }

        public string Encrypt(string plainText)
        {
            return Transform(plainText, true);
        }

        public string Decrypt(string cipherText)
        {
            return Transform(cipherText, false);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            return Transform(plainBytes, true);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return Transform(cipherBytes, false);
        }

        private string Transform(string text, bool encrypting)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            var result = new StringBuilder();
            var keyIndex = 0;

            foreach (var ch in text)
            {
                var cByte = (byte)ch;
                var kByte = (byte)key[keyIndex % key.Length];

                var shiftedByte = encrypting
                    ? (cByte + kByte) % alphabetSize
                    : (cByte - kByte + alphabetSize) % alphabetSize;

                result.Append((char)shiftedByte);

                keyIndex++;
            }

            return result.ToString();
        }

        private byte[] Transform(byte[] input, bool encrypting)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            var result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                var kByte = (byte)key[i % key.Length];

                result[i] = encrypting
                    ? (byte)((input[i] + kByte) % alphabetSize)
                    : (byte)((input[i] - kByte + alphabetSize) % alphabetSize);
            }

            return result;
        }
    }
}
