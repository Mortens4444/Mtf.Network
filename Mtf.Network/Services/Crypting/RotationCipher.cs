using Mtf.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mtf.Network.Services.Crypting
{
    public class RotationCipher : ICipher
    {
        private readonly int shift;
        private readonly bool rotateLeft;
        private readonly bool cryptWhiteSpaces;

        public RotationCipher(int shift, bool rotateLeft = true, bool cryptWhiteSpaces = true)
        {
            this.shift = shift;
            this.rotateLeft = rotateLeft;
            this.cryptWhiteSpaces = cryptWhiteSpaces;
        }

        public string Encrypt(string plainText)
        {
            return Transform(plainText, shift);
        }

        public string Decrypt(string cipherText)
        {
            return Transform(cipherText, -shift);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            return Transform(plainBytes, shift);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return Transform(cipherBytes, -shift);
        }

        private string Transform(string input, int shiftAmount)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            var chars = input.ToCharArray();
            int len = chars.Length;

            if (!cryptWhiteSpaces)
            {
                var indices = new List<int>();
                var filtered = new List<char>();

                for (int i = 0; i < len; i++)
                {
                    if (!Char.IsWhiteSpace(chars[i]))
                    {
                        indices.Add(i);
                        filtered.Add(chars[i]);
                    }
                }

                var rotated = Rotate(filtered, shiftAmount);
                for (int i = 0; i < indices.Count; i++)
                {
                    chars[indices[i]] = rotated[i];
                }

                return new string(chars);
            }

            return new string(Rotate(chars.ToList(), shiftAmount).ToArray());
        }

        private byte[] Transform(byte[] input, int shiftAmount)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            var output = new byte[input.Length];
            output = Rotate(input.ToList(), shiftAmount).ToArray();
            return output;
        }

        private List<T> Rotate<T>(List<T> list, int shiftAmount)
        {
            if (rotateLeft)
            {
                return RotateLeft(list, shiftAmount);
            }

            return RotateRight(list, shiftAmount);
        }

        private static List<T> RotateLeft<T>(List<T> list, int shiftAmount)
        {
            if (list.Count == 0)
            {
                return list;
            }

            int count = list.Count;
            shiftAmount %= count;
            if (shiftAmount < 0)
            {
                shiftAmount += count;
            }

            var result = new List<T>();
            result.AddRange(list.Skip(shiftAmount));
            result.AddRange(list.Take(shiftAmount));
            return result;
        }

        private static List<T> RotateRight<T>(List<T> list, int shiftAmount)
        {
            if (list.Count == 0)
            {
                return list;
            }

            int count = list.Count;
            shiftAmount %= count;
            if (shiftAmount < 0)
            {
                shiftAmount += count;
            }

            var result = new List<T>();
            result.AddRange(list.Skip(count - shiftAmount));
            result.AddRange(list.Take(count - shiftAmount));
            return result;
        }
    }
}
