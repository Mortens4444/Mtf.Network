using System.Net.Sockets;

namespace Mtf.Network
{
    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc854
    /// </summary>
    public class TelnetClient : Client
    {
        /// <summary>
        /// Telnet is inherently unencrypted, please use SSH instead!
        /// </summary>
        /// <param name="serverHost">Telnet server hostname or IP</param>
        /// <param name="serverPort">Telnet server port</param>
        public TelnetClient(string serverHost, ushort serverPort = 23)
            : base(serverHost, serverPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }
    }
}
