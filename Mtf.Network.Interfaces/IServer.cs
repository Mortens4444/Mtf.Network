using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Mtf.Network.Interfaces
{
    public interface IServer
    {
        ConcurrentDictionary<Socket, RSAParameters> ClientPublicKeys { get; }
    }
}
