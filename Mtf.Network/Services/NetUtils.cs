using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            int port;
            var rnd = new Random(Environment.TickCount);

            do
            {
                port = rnd.Next(1024, 65535);
            }
            while (!IsPortAvailable(port));

            return port;
        }

        public static bool IsPortAvailable(int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public static string FormatIPEndPoint(IPEndPoint ipEndPoint)
        {
            if (ipEndPoint == null)
            {
                throw new ArgumentNullException(nameof(ipEndPoint));
            }

            var addressBytes = ipEndPoint.Address.GetAddressBytes();
            var h1 = addressBytes[0];
            var h2 = addressBytes[1];
            var h3 = addressBytes[2];
            var h4 = addressBytes[3];

            var portBytes = BitConverter.GetBytes((ushort)ipEndPoint.Port);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portBytes);
            }
            var p1 = portBytes[0];
            var p2 = portBytes[1];

            return $"{h1},{h2},{h3},{h4},{p1},{p2}";
        }

        public static async Task<IPAddress> GetExternalIpAddressAsync()
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync("http://icanhazip.com").ConfigureAwait(false);
                    if (IPAddress.TryParse(response.Trim(), out var ipAddress))
                    {
                        return ipAddress;
                    }
                }
                catch { }

                return null;
            }
        }
    }
}
