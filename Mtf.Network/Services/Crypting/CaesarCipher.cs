using Mtf.Network.Interfaces;
using System;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    public class CaesarCipher : ICipher
    {
        private readonly int shift;

        public CaesarCipher(int shift)
        {
            this.shift = shift;
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

        private string Transform(string input, bool encrypt)
        {
            if (String.IsNullOrEmpty(input)) return input;

            var bytes = Encoding.UTF8.GetBytes(input);
            var transformed = Transform(bytes, encrypt);
            return Encoding.UTF8.GetString(transformed);
        }

        private byte[] Transform(byte[] input, bool encrypt)
        {
            if (input == null || input.Length == 0) return input;

            var result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                var b = input[i];
                result[i] = (byte)(encrypt
                    ? (b + shift) % 256
                    : (b - shift + 256) % 256);
            }
            return result;
        }
    }
}
