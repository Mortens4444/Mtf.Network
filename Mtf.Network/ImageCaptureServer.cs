using Mtf.Cryptography.Interfaces;
using Mtf.Network.EventArg;
using Mtf.Network.Interfaces;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class ImageCaptureServer : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private bool disposed;

        private readonly IImageSource imageSource;
        private readonly string identifier;
        private readonly IPAddress ipAddress;
        private readonly AddressFamily addressFamily;
        private readonly SocketType socketType;
        private readonly ProtocolType protocolType;
        private readonly ushort listenerPort;
        private readonly ICipher[] ciphers;

        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        public int BufferSize { get; set; } = Constants.ImageBufferSize;

        public Server Server { get; set; }

        public int MaxRetryCount { get; set; } = 3;

        public int FPS { get; set; } = 25;

        public ImageCaptureServer(IImageSource imageSource, string identifier,
            IPAddress ipAddress = null, AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp,
            ushort listenerPort = 0, params ICipher[] ciphers)
        {
            this.imageSource = imageSource;
            this.identifier = identifier;
            this.ipAddress = ipAddress ?? IPAddress.Any;
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
            this.listenerPort = listenerPort;
            this.ciphers = ciphers;
        }

        public Server StartVideoCaptureServer(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            int retryCount = 0;

            Server = new Server(addressFamily, socketType, protocolType, ipAddress, listenerPort, ciphers);
            Server.Start();
            Server.SetBufferSize(BufferSize);
            
            _ = Task.Run(async () =>
            {
                var waitTime = 1000 / FPS;
                while (retryCount < MaxRetryCount)
                {
                    try
                    {
                        await CaptureAndSendLoop(waitTime, cancellationTokenSource.Token).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        Server.SendMessageToAllClients($"ImageSourceCreationFailure|{identifier}|{ex}", true);

                        if (retryCount < MaxRetryCount)
                        {
                            await Task.Delay(2000, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        else
                        {
                            Server.SendMessageToAllClients($"ImageSourceCreationFailure|{identifier}|Max retry attempts reached", true);
                        }
                    }
                }
            }, cancellationTokenSource.Token);

            return Server;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            Server?.Stop();
            Server?.Dispose();
        }

        private async Task CaptureAndSendLoop(int delay, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var imageBytes = await imageSource.CaptureAsync(token).ConfigureAwait(false);
                if (imageBytes != null)
                {
                    Server.SendBytesInChunksToAllClients(imageBytes);
                }
                else
                {
                    Server.SendMessageToAllClients($"ImageSourceFailure|{identifier}", true);
                }

                await Task.Delay(delay, token).ConfigureAwait(false);
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
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                    Server?.Dispose();
                }
                disposed = true;
            }
        }

        ~ImageCaptureServer()
        {
            Dispose(false);
        }
    }
}
