using Mtf.Network.Services;
using System;
using System.Net;
using System.Net.Sockets;

namespace Mtf.Network.Extensions
{
    public static class EndPointExtensions
    {
        public const string IpAny = "0.0.0.0";
        public const string IpAnyWithColon = "0.0.0.0:";

        public static string GetEndPointInfo(this EndPoint endpoint, string separator = "|")
        {
            var endpointText = endpoint?.ToString();
            if (String.IsNullOrEmpty(endpointText))
            {
                return String.Empty;
            }
            if (endpointText.StartsWith(IpAnyWithColon, StringComparison.OrdinalIgnoreCase))
            {
                return $"{endpointText} {String.Join(separator, NetUtils.GetLocalIPAddresses(AddressFamily.InterNetwork))}";
            }
            return endpointText;
        }
    }
}
