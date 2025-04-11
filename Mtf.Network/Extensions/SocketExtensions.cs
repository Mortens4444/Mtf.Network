using Mtf.Network.Exceptions;
using Mtf.Network.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Mtf.Network.Extensions
{
    public static class SocketExtensions
    {
        public static IEnumerable<string> GetLocalIPAddresses(this Socket socket)
        {
            var ipAddress = socket?.LocalEndPoint?.ToString();
            if (String.IsNullOrEmpty(ipAddress))
            {
                return Enumerable.Empty<string>();
            }
            if (ipAddress.StartsWith("0.0.0.0:", StringComparison.OrdinalIgnoreCase))
            {
                return NetUtils.GetLocalIPAddresses(AddressFamily.InterNetwork);
            }
            return new string[] { ipAddress.Substring(0, ipAddress.IndexOf(':')) };
        }

        public static string GetLocalIPAddressesInfo(this Socket socket)
        {
            var ipAddress = socket?.LocalEndPoint?.ToString();
            if (String.IsNullOrEmpty(ipAddress))
            {
                return String.Empty;
            }
            if (ipAddress.StartsWith("0.0.0.0:", StringComparison.OrdinalIgnoreCase))
            {
                return $"{ipAddress} {String.Join(", ", NetUtils.GetLocalIPAddresses(AddressFamily.InterNetwork))}";
            }
            return ipAddress;
        }

        public static void Connect(this Socket socket, string serverIp, ushort serverPort, int timeoutMs)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (IsSocketConnected(socket))
            {
                return;
            }

            var result = socket.BeginConnect(serverIp, serverPort, null, null);
            var ipAddress = socket.GetLocalIPAddressesInfo();
            if (!result.AsyncWaitHandle.WaitOne(timeoutMs))
            {
                socket.Close();
                throw new ConnectionFailedException(serverIp, serverPort, ipAddress);
            }

            if (!IsSocketConnected(socket))
            {
                socket.Close();
                throw new ConnectionTimedOutException(serverIp, serverPort, ipAddress);
            }
        }

        public static bool IsSocketConnected(this Socket socket) => socket != null && socket.Connected;

        public static void CloseSocket(this Socket socket)
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
                if (socket != null && !socket.IsBound)
                {
                    socket.Close();
                    socket.Dispose();
                }
            }
        }
    }
}
