using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mtf.Network
{

    /// <summary>
    /// NOT TESTED
    /// </summary>
    public class UpnpClient : Client
    {
        public UpnpClient(string serverHost = "239.255.255.250", ushort listenerPort = 1900)
            : base(serverHost, listenerPort, AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
        }

        /// <summary>
        /// UPnP discovery request minta az M-SEARCH metódussal
        /// </summary>
        public async Task SendDiscoveryMessage()
        {
            var message = "M-SEARCH * HTTP/1.1\r\n" +
                          $"HOST: {ServerHostnameOrIPAddress}:{ListenerPortOfServer}\r\n" +
                          "MAN: \"ssdp:discover\"\r\n" +
                          "MX: 3\r\n" +
                          "ST: ssdp:all\r\n" +
                          "\r\n";

            await SendMessageAsync(message).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.GetBytes(message);
            var endPoint = new IPEndPoint(IPAddress.Parse(ServerHostnameOrIPAddress), ListenerPortOfServer);
            _ = await Socket.SendToAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None, endPoint).ConfigureAwait(false);
        }
    }
}
