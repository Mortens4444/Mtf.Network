namespace Mtf.Network.Interfaces
{
    public interface IEncryptionHandler
    {
        byte[] Encrypt(byte[] data);

        byte[] Decrypt(byte[] data);
    }
}
