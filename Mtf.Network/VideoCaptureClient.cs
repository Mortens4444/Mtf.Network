using Mtf.Network.EventArg;
using Mtf.Network.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Mtf.Network
{
    public class VideoCaptureClient : IDisposable
    {
        public int BufferSize { get; set; } = Constants.ImageBufferSize;

        private readonly string serverIp;
        private readonly ushort serverPort;
        private readonly AddressFamily addressFamily;
        private readonly SocketType socketType;
        private readonly ProtocolType protocolType;
        private readonly ICipher[] ciphers;
        
        private MemoryStream receiveBuffer;
        private long processedPosition;
        private Client client;
        private int started;
        private bool disposed;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] PngEndMarker = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }; // IEND chunk type + CRC

        public VideoCaptureClient(string serverIp, int serverPort, AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp, params ICipher[] ciphers)
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
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
            this.ciphers = ciphers;

            receiveBuffer = new MemoryStream(BufferSize);
        }

        public void Start(int timeout = Constants.SocketConnectionTimeout)
        {
            if (Interlocked.Exchange(ref started, 1) == 0)
            {
                client = new Client(serverIp, serverPort, addressFamily, socketType, protocolType, ciphers)
                {
                    Timeout = timeout
                };
                client.DataArrived += ClientDataArrivedEventHandler;
                client.SetBufferSize(BufferSize);
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

            receiveBuffer?.Dispose();
            receiveBuffer = new MemoryStream(BufferSize);
            processedPosition = 0;
        }

        private void ClientDataArrivedEventHandler(object sender, DataArrivedEventArgs e)
        {
            if (e.Data == null || e.Data.Length == 0)
            {
                return;
            }

            try
            {
                var originalPosition = receiveBuffer.Position;
                receiveBuffer.Seek(0, SeekOrigin.End);
                receiveBuffer.Write(e.Data, 0, e.Data.Length);
                receiveBuffer.Position = originalPosition;

                ProcessBuffer();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
                CompactBuffer(receiveBuffer.Length);
            }
        }

        private void ProcessBuffer()
        {
            var buffer = receiveBuffer.GetBuffer();
            var bufferLength = receiveBuffer.Length;

            while (true)
            {
                var start = FindSequence(buffer, bufferLength, processedPosition, PngSignature);
                if (start == -1)
                {
                    break;
                }

                var end = FindSequence(buffer, bufferLength, start + PngSignature.Length, PngEndMarker);
                if (end == -1)
                {
                    break;
                }

                var frameEnd = end + PngEndMarker.Length;
                var frameLength = frameEnd - start;

                if (frameLength <= 0 || frameEnd > bufferLength)
                {
                    OnErrorOccurred(new InvalidDataException($"Invalid frame length at position {start}."));
                    processedPosition = frameEnd;
                    continue;
                }

                var frameData = new byte[frameLength];
                Buffer.BlockCopy(buffer, start, frameData, 0, frameLength);
                OnFrameArrived(frameData);

                processedPosition = frameEnd;
            }

            CompactBuffer(processedPosition);
        }

        private void CompactBuffer(long bytesToRemove)
        {
            if (bytesToRemove <= 0)
            {
                return;
            }

            var buffer = receiveBuffer.GetBuffer();
            var bufferLength = receiveBuffer.Length;
            var remainingLength = bufferLength - bytesToRemove;

            if (remainingLength > 0)
            {
                Buffer.BlockCopy(buffer, (int)bytesToRemove, buffer, 0, (int)remainingLength);
            }

            receiveBuffer.SetLength(remainingLength);
            receiveBuffer.Position = remainingLength;
            processedPosition = 0;
        }

        private static int FindSequence(byte[] buffer, long length, long offset, byte[] pattern)
        {
            if (pattern == null || pattern.Length == 0 || buffer == null || length == 0 || offset >= length)
            {
                return -1;
            }

            var maxIndex = length - pattern.Length;
            for (var i = offset; i <= maxIndex; i++)
            {
                if (buffer[i] != pattern[0])
                {
                    continue;
                }

                var match = true;
                for (var j = 1; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return (int)i;
                }
            }
            return -1;
        }

        protected void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        protected virtual void OnFrameArrived(byte[] fullImageData)
        {
            try
            {
                using (var stream = new MemoryStream(fullImageData))
                {
                    var image = Image.FromStream(stream, false, false);
                    var clonedImage = (Image)image.Clone();
                    FrameArrived?.Invoke(this, new FrameArrivedEventArgs(clonedImage));
                }
            }
            catch (ArgumentException ex)
            {
                OnErrorOccurred(new InvalidDataException("Failed to load image from received data.", ex));
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                    receiveBuffer?.Dispose();
                }
                disposed = true;
            }
        }

        ~VideoCaptureClient()
        {
            Dispose(false);
        }
    }
}
