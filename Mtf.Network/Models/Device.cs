using Mtf.Network;
using Mtf.Network.EventArg;
using System;
using System.Collections.Generic;
using System.Net;

namespace Mtf.Network.Models
{
    public class Device
    {
        public List<Device> DeviceList { get; set; } = new List<Device>();

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public int NumberOfStreams { get; set; }

        public List<ServiceType> Services { get; set; }

        public string CacheContol { get; set; }

        public string ST { get; set; }

        public string EXT { get; set; }

        public string Server { get; set; }

        public string MacAddress { get; set; }

        public string DeviceType { get; set; }

        public string FriendlyName { get; set; }

        public string Manufacturer { get; set; }

        public string ManufacturerUrl { get; set; }

        public string PresentationUrl { get; set; }

        public string ModelDescription { get; set; }

        public string ModelName { get; set; }

        public string ModelNumber { get; set; }

        public string ModelUrl { get; set; }

        public string SerialNumber { get; set; }

        public string ProductSerial { get; set; }

        public string UDN { get; set; }

        public string UPC { get; set; }

        public string USN { get; set; }

        public string Location { get; set; }

        //private string location;
        //     public string Location
        //     {
        //         get
        //         {
        //             return location;
        //         }
        //         set
        //         {
        //             location = value;
        //             var ip = location.Substring("//", "/").Split(':');
        //             LocationIP = ip[0];
        //             LocationPort = ip[1].ConvertToInt32();
        //             LocationEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(LocationIP), LocationPort);
        //         }
        //     }

        //     public void GetDeviceInfo()
        //     {
        //         var content = location.Substring(String.Format("http://{0}:{1}", LocationIP, LocationPort));
        //         var message_to_send = String.Format("GET {0} HTTP/1.1\r\nHost: {1}\r\n\r\n", content, LocationIP);
        //         //byte[] bytes_to_send = message_to_send.ToByteArray();
        //         /*using (*/
        //         var client = new Client(LocationIP, (ushort)LocationPort);
        //         {
        //             client.DataArrived += DataArrived;
        //             client.Send(message_to_send);
        //             //client.Close();
        //         }
        //         //client.Send(bytes_to_send, bytes_to_send.Length, new IPEndPoint(System.Net.IPAddress.Parse(this.location_ip), this.location_port));
        //     }

        //     public void GetDeviceInfoWithWebClient()
        //     {
        //         var wc = new WebClient
        //         {
        //             Credentials = new NetworkCredential(VideosecCamera.DEFAULT_USERNAME, VideosecCamera.DEFAULT_PASSWORD)
        //         };
        //         wc.OpenReadCompleted += OnWebReadCompleted;

        //         wc.OpenReadAsync(new Uri(String.Format("http://{0}/cgi-bin/ret.cgi", LocationIP)));
        //     }

        //     static void OnWebReadCompleted(object sender, OpenReadCompletedEventArgs e)
        //     {
        //         var s = e.Result;
        //         var b = new byte[100000];
        //         int read, total = 0;
        //         while ((read = s.Read(b, total, 1000)) != 0)
        //             total += read;

        //         b.ToASCIIStringZeroByteTerminated();
        //     }

        //     void DataArrived(object sender, DataArrivedEventArgs e)
        //     {
        //         var msg = e.Response.ToASCIIString();
        //         //((Client)sender).Close();
        //         //msg = msg.Substring("<device>", "</device>");
        //         DeviceType = msg.Substring("<deviceType>", "</deviceType>");
        //         FriendlyName = msg.Substring("<friendlyName>", "</friendlyName>");
        //         Manufacturer = msg.Substring("<manufacturer>", "</manufacturer>");
        //         ManufacturerUrl = msg.Substring("<manufacturerURL>", "</manufacturerURL>");
        //         ModelDescription = msg.Substring("<modelDescription>", "</modelDescription>");
        //         ModelName = msg.Substring("<modelName>", "</modelName>");
        //         ModelNumber = msg.Substring("<modelNumber>", "</modelNumber>");
        //         ModelUrl = msg.Substring("<modelURL>", "</modelURL>");
        //         SerialNumber = msg.Substring("<serialNumber>", "</serialNumber>");
        //         UDN = msg.Substring("<UDN>", "</UDN>");
        //         PresentationUrl = msg.Substring("<presentationURL>", "</presentationURL>");
        //     }

        //     public EndPoint LocationEndPoint { get; private set; }

        //     public string LocationIP { get; private set; }

        //     public int LocationPort { get; private set; }

        //     public override string ToString()
        //     {
        //         return String.Format(FriendlyName == String.Empty ? "{0}:{1}" : "{0} ({1}:{2})", FriendlyName, IPAddress, Port);

        //         /*if (this.manufacturer == String.Empty)
        //	return String.Format("{0} ({1}:{2})", this.model_name, this.ip_address, this.port);
        //else
        //	return String.Format("{0} - {1} ({2}:{3})", this.manufacturer, this.model_name, this.ip_address, this.port);*/
        //     }
    }
}
