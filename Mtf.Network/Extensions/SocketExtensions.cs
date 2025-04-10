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
    }
}
