using Mtf.Cryptography.AsymmetricCiphers;
using Mtf.Cryptography.Interfaces;
using Mtf.Network.Interfaces;
using System;
using System.Net.Sockets;
using System.Text;

namespace Mtf.Network
{
    public class MultiCipherEncryptionHandler : IEncryptionHandler
    {
        private readonly ICipher[] ciphers;

        public MultiCipherEncryptionHandler(ICipher[] ciphers)
        {
            this.ciphers = ciphers ?? Array.Empty<ICipher>();
        }

        public byte[] Transform(byte[] data, bool encrypt)
        {
            return encrypt ? Encrypt(data) : Decrypt(data);
        }

        public byte[] Encrypt(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            if (data.Length == 0)
            {
                return data;
            }

            foreach (var cipher in ciphers)
            {
                data = cipher.Encrypt(data);
            }
            return data;
        }

        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            if (data.Length == 0)
            {
                return data;
            }

            for (int i = ciphers.Length - 1; i >= 0; i--)
            {
                data = ciphers[i].Decrypt(data);
            }
            return data;
        }
    }
}
