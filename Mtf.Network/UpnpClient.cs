using Mtf.Network.EventArg;
using Mtf.Network.Extensions;
using Mtf.Network.Models;
using System;
using System.IO.Pipes;
using System.Net;
using System.Net.Http.Headers;
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
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;

        public UpnpClient(string serverHost = "239.255.255.250", ushort listenerPort = 1900)
            : base(serverHost, listenerPort, AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            Encoding = Encoding.ASCII;
            Socket.DontFragment = true;

            DataArrived += UpnpClient_DataArrived;
        }

        private async void UpnpClient_DataArrived(object sender, DataArrivedEventArgs e)
        {
            var responseMessage = e.Data.ToZeroByteTerminatedString(Encoding);
            var device = new Device
            {
                CacheContol = responseMessage.ExtractBetween("Cache-Control:", "\r\n", StringComparison.OrdinalIgnoreCase),
                ST = responseMessage.ExtractBetween("ST:", "\r\n", StringComparison.OrdinalIgnoreCase),
                EXT = responseMessage.ExtractBetween("EXT:", "\r\n", StringComparison.OrdinalIgnoreCase),
                Server = responseMessage.ExtractBetween("SERVER:", "\r\n", StringComparison.OrdinalIgnoreCase),
                USN = responseMessage.ExtractBetween("USN:", "\r\n", StringComparison.OrdinalIgnoreCase),
                Location = responseMessage.ExtractBetween("LOCATION:", "\r\n", StringComparison.OrdinalIgnoreCase)
            };
            if (!String.IsNullOrEmpty(device.Location))
            {
                var response = await GetHttpWebResponseAsync(device.Location).ConfigureAwait(false);
                device.Manufacturer = response.ExtractBetween("<manufacturer>", "</manufacturer>", StringComparison.OrdinalIgnoreCase);
                device.MacAddress = response.ExtractBetween("<MAC>", "</MAC>", StringComparison.OrdinalIgnoreCase);

                var startIndex = response.IndexOf("<deviceType>urn:schemas-pelco-com:device:Camera:", StringComparison.OrdinalIgnoreCase);
                device.UDN = response.ExtractBetween("<UDN>", "</UDN>", StringComparison.OrdinalIgnoreCase, startIndex);
            }

            OnDeviceDiscovered(device);
        }

        /// <summary>
        /// UPnP discovery request minta az M-SEARCH metódussal
        /// </summary>
        public async Task SendDiscoveryMessage()
        {
            var message = new StringBuilder(100)
                .Append("M-SEARCH * HTTP/1.1\r\n")
                .Append($"HOST: {ServerHostnameOrIPAddress}:{ListenerPortOfServer}\r\n")
                .Append("MAN: \"ssdp:discover\"\r\n")
                .Append("MX: 3\r\n")
                .Append("ST: ssdp:all\r\n")
                .Append("\r\n");

            await SendMessageAsync(message.ToString()).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.GetBytes(message);
            var endPoint = new IPEndPoint(IPAddress.Parse(ServerHostnameOrIPAddress), ListenerPortOfServer);

#if NET462_OR_GREATER
            _ = await Socket.SendToAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None, endPoint).ConfigureAwait(false);
#else
            _ = Socket.SendTo(messageBytes, SocketFlags.None, endPoint);
#endif
        }

        protected virtual void OnDeviceDiscovered(Device device)
        {
            DeviceDiscovered?.Invoke(this, new DeviceDiscoveredEventArgs(device));
        }

        private static async Task<string> GetHttpWebResponseAsync(string uri)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}
