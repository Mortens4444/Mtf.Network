using Mtf.Network.Interfaces;
using System;

namespace Mtf.Network.Services.Crypting
{
    public class AtbashCipher : ICipher
    {
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
            return TransformBytes(plainBytes);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return TransformBytes(cipherBytes);
        }

        private static string Transform(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                var ch = input[i];

                if (Char.IsLetter(ch))
                {
                    if (Char.IsUpper(ch))
                    {
                        result[i] = (char)('A' + ('Z' - ch));
                    }
                    else
                    {
                        result[i] = (char)('a' + ('z' - ch));
                    }
                }
                else
                {
                    result[i] = ch;
                }
            }

            return new string(result);
        }

        private static byte[] TransformBytes(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                return null;
            }

            if (inputBytes.Length == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] resultBytes = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++)
            {
                byte currentByte = inputBytes[i];

                if (currentByte >= 'A' && currentByte <= 'Z')
                {
                    resultBytes[i] = (byte)('A' + ('Z' - currentByte));
                }
                else if (currentByte >= 'a' && currentByte <= 'z')
                {
                    resultBytes[i] = (byte)('a' + ('z' - currentByte));
                }
                else
                {
                    resultBytes[i] = currentByte;
                }
            }

            return resultBytes;
        }
    }
}