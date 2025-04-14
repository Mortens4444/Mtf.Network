using Mtf.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mtf.Network.Services.Crypting
{
    public class RailFenceCipher : ICipher
    {
        private readonly int key;

        public RailFenceCipher(int key)
        {
            if (key < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Key must be at least 2.");
            }

            this.key = key;
        }

        public string Encrypt(string plainText) => Transform(plainText, true);

        public string Decrypt(string cipherText) => Transform(cipherText, false);

        public byte[] Encrypt(byte[] plainBytes) => Transform(plainBytes, true);

        public byte[] Decrypt(byte[] cipherBytes) => Transform(cipherBytes, false);

        private string Transform(string input, bool encrypt)
        {
            if (String.IsNullOrEmpty(input)) return input;

            var bytes = Encoding.UTF8.GetBytes(input);
            var transformed = Transform(bytes, encrypt);
            return Encoding.UTF8.GetString(transformed);
        }

        private byte[] Transform(byte[] input, bool encrypt)
        {
            if (input == null)
            {
                return null;
            }

            if (input.Length == 0)
            {
                return input;
            }

            if (encrypt)
            {
                var rails = new List<byte>[key];
                for (int i = 0; i < key; i++)
                {
                    rails[i] = new List<byte>();
                }

                int row = 0, dir = 1;
                foreach (var b in input)
                {
                    rails[row].Add(b);

                    if (row == 0)
                    {
                        dir = 1;
                    }
                    else if (row == key - 1)
                    {
                        dir = -1;
                    }

                    row += dir;
                }

                return rails.SelectMany(r => r).ToArray();
            }
            else
            {
                var railLengths = new int[key];
                int row = 0, dir = 1;

                for (int i = 0; i < input.Length; i++)
                {
                    railLengths[row]++;

                    if (row == 0)
                    {
                        dir = 1;
                    }
                    else if (row == key - 1)
                    {
                        dir = -1;
                    }

                    row += dir;
                }

                var rails = new List<byte>[key];
                int pos = 0;
                for (int i = 0; i < key; i++)
                {
                    rails[i] = new List<byte>(input.Skip(pos).Take(railLengths[i]));
                    pos += railLengths[i];
                }

                var result = new byte[input.Length];
                var railPositions = new int[key];
                row = 0; dir = 1;

                for (int i = 0; i < input.Length; i++)
                {
                    result[i] = rails[row][railPositions[row]++];

                    if (row == 0)
                    {
                        dir = 1;
                    }
                    else if (row == key - 1)
                    {
                        dir = -1;
                    }

                    row += dir;
                }

                return result;
            }
        }
    }
}
