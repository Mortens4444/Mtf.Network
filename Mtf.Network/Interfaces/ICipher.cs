namespace Mtf.Network.Interfaces
{
    public interface ICipher
    {
        string Encrypt(string plainText);

        string Decrypt(string cipherText);

        byte[] Encrypt(byte[] plainBytes);

        byte[] Decrypt(byte[] cipherBytes);
    }
}
