using Mtf.Network.Interfaces;

namespace Mtf.Network.Services.Crypting
{
    public class None : ICipher
    {
        public string Decrypt(string cipherText)
        {
            return cipherText;
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            return cipherBytes;
        }

        public string Encrypt(string plainText)
        {
            return plainText;
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            return plainBytes;
        }
    }
}
