using Mtf.Network.Models;
using System;
using System.Net.Sockets;

namespace Mtf.Network
{
    public class HttpClient : Client
    {
        public HttpClient(Uri uri)
            : base(uri.Host, (ushort)uri.Port, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }

        public void Send(HttpPacket httpPacket)
        {
            if (httpPacket == null)
            {
                throw new ArgumentNullException(nameof(httpPacket));
            }
            Send(httpPacket.ToString());
        }
    }
}
