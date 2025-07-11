namespace Mtf.Network.Interfaces
{
    public interface IEncryptionHandler
    {
        byte[] Transform(byte[] data, bool encrypt);

        byte[] Encrypt(byte[] data);

        byte[] Decrypt(byte[] data);
    }
}
