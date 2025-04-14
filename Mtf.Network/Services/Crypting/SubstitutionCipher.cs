using Mtf.Network.Interfaces;
using System;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    public class SubstitutionCipher : ICipher
    {
        private readonly string sourceAlphabet;
        private readonly string targetAlphabet;

        public SubstitutionCipher(string sourceAlphabet, string targetAlphabet)
        {
            this.sourceAlphabet = sourceAlphabet ?? throw new ArgumentNullException(nameof(sourceAlphabet));
            this.targetAlphabet = targetAlphabet ?? throw new ArgumentNullException(nameof(targetAlphabet));
            if (sourceAlphabet.Length != targetAlphabet.Length)
            {
                throw new ArgumentException("Alphabets must be of equal length.");
            }
        }

        public string Encrypt(string plainText)
        {
            return Transform(plainText, sourceAlphabet, targetAlphabet);
        }

        public string Decrypt(string cipherText)
        {
            return Transform(cipherText, targetAlphabet, sourceAlphabet);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            return Transform(plainBytes, sourceAlphabet, targetAlphabet);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return Transform(cipherBytes, targetAlphabet, sourceAlphabet);
        }

        private string Transform(string input, string fromAlphabet, string toAlphabet)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            var result = new StringBuilder(input.Length);

            foreach (var ch in input)
            {
                var index = fromAlphabet.IndexOf(ch);
                result.Append(index >= 0 ? toAlphabet[index] : ch);
            }

            return result.ToString();
        }

        private byte[] Transform(byte[] input, string fromAlphabet, string toAlphabet)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            var result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                var current = input[i];
                var c = (char)current;
                var index = fromAlphabet.IndexOf(c);
                result[i] = index >= 0 ? (byte)toAlphabet[index] : current;
            }

            return result;
        }
    }
}
