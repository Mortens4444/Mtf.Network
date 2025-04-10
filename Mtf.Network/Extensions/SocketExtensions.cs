using Mtf.Network.Services;
using System;
using System.Net.Sockets;

namespace Mtf.Network.Extensions
{
    public static class SocketExtensions
    {
        public static string GetLocalIPAddresses(this Socket socket)
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
    }
}
