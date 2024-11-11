using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Mtf.Network.Services
{
    public static class NetUtils
    {
        public static bool IsSocketConnected(Socket socket) => socket != null && socket.Connected;

        public static void CloseSocket(Socket socket)
        {
            try
            {
                if (socket?.Connected ?? false)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch { }
            finally
            {
                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                }
            }
        }

        public static Socket CreateSocket(IPAddress ip, int port, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool server)
        {
            var socket = new Socket(addressFamily, socketType, protocolType);
            if (port == 0)
            {
                port = GetFreePort();
            }

            if (server)
            {
                socket.Bind(new IPEndPoint(ip, port));
                socket.Listen(Constants.MaxPendingConnections);
            }
            else
            {
                var result = socket.BeginConnect(ip, port, null, null);
                _ = result.AsyncWaitHandle.WaitOne(Constants.SocketConnectionTimeout, true);
            }

            return socket;
        }

        public static int GetFreePort()
        {
            var rnd = new Random(Environment.TickCount);
            int port;

            do
            {
                port = rnd.Next(1024, 65535);
            }
            while (!IsPortAvailable(port));

            return port;
        }

        public static bool IsPortAvailable(int port)
        {
            var igp = IPGlobalProperties.GetIPGlobalProperties();
            var tci = igp.GetActiveTcpConnections();

            foreach (var connection_info in tci)
            {
                if (connection_info.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
