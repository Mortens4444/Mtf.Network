using Mtf.Network.EventArg;
using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Mtf.Network
{
    public class VideoCaptureClient : IDisposable
    {
        public int BufferSize { get; set; } = Constants.ImageBufferSize;
        private readonly string serverIp;
        private readonly ushort serverPort;
        private MemoryStream receiveBuffer;
        private long processedPosition;

        private Client client;
        private int started;
        private bool disposed;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] PngEndMarker = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }; // IEND chunk type + CRC

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

            receiveBuffer = new MemoryStream(BufferSize);
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
                var startIndex = FindSequence(buffer, bufferLength, processedPosition, PngSignature);
                if (startIndex == -1)
                {
                    break;
                }

                processedPosition = startIndex;

                var searchEndFrom = processedPosition + PngSignature.Length;
                var endIndexMarkerStart = FindSequence(buffer, bufferLength, searchEndFrom, PngEndMarker);

                if (endIndexMarkerStart == -1)
                {
                    break;
                }

                var frameEndPosition = endIndexMarkerStart + PngEndMarker.Length - 1;
                var frameLength = (int)(frameEndPosition - processedPosition + 1);

                if (frameLength <= 0 || processedPosition + frameLength > bufferLength)
                {
                    OnErrorOccurred(new InvalidDataException($"Invalid frame length calculated: {frameLength} at position {processedPosition}"));
                    processedPosition = frameEndPosition + 1;
                    continue;
                }

                var fullImageData = new byte[frameLength];
                Buffer.BlockCopy(buffer, (int)processedPosition, fullImageData, 0, frameLength);
                OnFrameArrived(fullImageData);

                processedPosition = frameEndPosition + 1;
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

        private static int FindSequence(byte[] bufferToSearch, long bufferLength, long startOffset, byte[] sequenceToFind)
        {
            if (sequenceToFind == null || sequenceToFind.Length == 0 || bufferToSearch == null || bufferLength == 0)
            {
                return -1;
            }

            if (startOffset < 0)
            {
                startOffset = 0;
            }

            var maxSearchIndex = bufferLength - sequenceToFind.Length;
            if (startOffset > maxSearchIndex)
            {
                return -1;
            }

            for (long i = startOffset; i <= maxSearchIndex; i++)
            {
                var match = true;
                for (int j = 0; j < sequenceToFind.Length; j++)
                {
                    if (bufferToSearch[i + j] != sequenceToFind[j])
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
                    FrameArrived?.Invoke(this, new FrameArrivedEventArgs(image));
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
