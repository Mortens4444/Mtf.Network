using Mtf.Network.EventArg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mtf.Network
{
    public class VideoCaptureClient
    {
        private readonly string serverIp;
        private readonly ushort serverPort;
        private readonly List<byte> imageDataChunks = new List<byte>();

        private Client client;
        private int started;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;

        public VideoCaptureClient(string serverIp, int serverPort)
        {
            if (String.IsNullOrWhiteSpace(serverIp))
            {
                throw new ArgumentException("Server IP cannot be null or empty.", nameof(serverIp));
            }

            if (serverPort < 1 || serverPort > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(serverPort), "Server port must be between 1 and 65535.");
            }

            this.serverIp = serverIp == "0.0.0.0" ? "localhost" : serverIp;
            this.serverPort = (ushort)serverPort;
        }

        public void Start(int timeout = Constants.SocketConnectionTimeout)
        {
            if (Interlocked.Exchange(ref started, 1) == 0)
            {
                client = new Client(serverIp, serverPort)
                {
                    Timeout = timeout
                };
                client.DataArrived += ClientDataArrivedEventHandler;
                client.SetBufferSize(409600);
                client.Connect();
            }
        }

        public void Stop()
        {
            if (client != null && Interlocked.Exchange(ref started, 0) == 1)
            {
                client.DataArrived -= ClientDataArrivedEventHandler;
                client.Disconnect();
                client.Dispose();
                client = null;
            }
        }

        public static bool IsPngStart(byte[] bytes)
        {
            byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            if (bytes == null || bytes.Length < pngSignature.Length)
            {
                return false;
            }

            for (int i = 0; i < pngSignature.Length; i++)
            {
                if (bytes[i] != pngSignature[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void ClientDataArrivedEventHandler(object sender, DataArrivedEventArgs e)
        {
            if (e.Data == null || e.Data.Length == 0)
            {
                return;
            }

            if (IsPngStart(e.Data))
            {
                imageDataChunks.Clear();
            }

            imageDataChunks.AddRange(e.Data);

            if (imageDataChunks.Count > 0 && IsCompleteImage(imageDataChunks))
            {
                var fullImageData = imageDataChunks.ToArray();
                OnFrameArrived(fullImageData);
                imageDataChunks.Clear();
            }
        }

        private static bool IsCompleteImage(List<byte> imageData)
        {
            var pngEndMarker = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }; // IEND marker
            return imageData.Count >= pngEndMarker.Length &&
                imageData.Skip(imageData.Count - pngEndMarker.Length).SequenceEqual(pngEndMarker);
        }

        protected virtual void OnFrameArrived(byte[] fullImageData)
        {
            using (var stream = new MemoryStream(fullImageData))
            {
                var image = Image.FromStream(stream);
                FrameArrived?.Invoke(this, new FrameArrivedEventArgs(image));
            }
        }
    }
}
