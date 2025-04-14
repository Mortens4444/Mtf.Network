using Mtf.Network.Interfaces;
using System;

namespace Mtf.Network.Services.Crypting
{
    public class AffineCipher : ICipher
    {
        private const int Modulus = 26;
        private readonly int a;
        private readonly int b;

        public AffineCipher(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public string Encrypt(string plainText)
        {
            return Transform(plainText, a, b);
        }

        public string Decrypt(string cipherText)
        {
            int aInverse = MultiplicativeInverse(a, Modulus);
            return Transform(cipherText, aInverse, -aInverse * b);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            throw new NotImplementedException();
        }

        private static string Transform(string input, int a, int b)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            var result = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if (Char.IsLetter(ch))
                {
                    var offset = Char.IsUpper(ch) ? 'A' : 'a';
                    int value = ch - offset;
                    int encryptedValue = (a * value + b) % Modulus;

                    if (encryptedValue < 0)
                    {
                        encryptedValue += Modulus;
                    }

                    result[i] = (char)(encryptedValue + offset);
                }
                else
                {
                    result[i] = ch;
                }
            }

            return new string(result);
        }

        private static int MultiplicativeInverse(int a, int m)
        {
            for (int x = 1; x < m; x++)
            {
                if ((a * x) % m == 1)
                {
                    return x;
                }
            }

            throw new ArgumentException($"No multiplicative inverse for a = {a} under modulo {m}");
        }
    }
}
