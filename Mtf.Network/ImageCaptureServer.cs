using Mtf.Network.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class ImageCaptureServer
    {
        private const int maxRetryCount = 3;
        private const int fps = 25;
        private readonly IImageSource imageSource;
        private readonly string identifier;

        public ImageCaptureServer(IImageSource imageSource, string identifier)
        {
            this.imageSource = imageSource;
            this.identifier = identifier;
        }

        public Server StartVideoCaptureServer(CancellationTokenSource cancellationTokenSource)
        {
            int retryCount = 0;

            var server = new Server();
            server.Start();
            server.SetBufferSize(409600);

            _ = Task.Run(async () =>
            {
                var waitTime = 1000 / fps;
                while (retryCount < maxRetryCount)
                {
                    try
                    {
                        await CaptureAndSendLoop(server, waitTime, cancellationTokenSource.Token).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        server.SendMessageToAllClients($"ImageSourceCreationFailure|{identifier}|{ex}", true);

                        if (retryCount < maxRetryCount)
                        {
                            await Task.Delay(2000, cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        else
                        {
                            server.SendMessageToAllClients($"ImageSourceCreationFailure|{identifier}|Max retry attempts reached", true);
                        }
                    }
                }
            }, cancellationTokenSource.Token);

            return server;
        }

        private async Task CaptureAndSendLoop(Server server, int delay, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var imageBytes = await imageSource.CaptureAsync(token).ConfigureAwait(false);
                if (imageBytes != null)
                {
                    server.SendBytesInChunksToAllClients(imageBytes);
                }
                else
                {
                    server.SendMessageToAllClients($"ImageSourceFailure|{identifier}", true);
                }

                await Task.Delay(delay, token).ConfigureAwait(false);
            }
        }
    }
}
