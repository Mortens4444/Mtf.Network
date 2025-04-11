using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Extensions;
using Mtf.Network.Interfaces;
using Mtf.Network.Services;
using System;
using System.Linq;
using System.Threading;

namespace Mtf.Network
{
    public class VncServer : IDisposable
    {
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        private readonly IScreenInfoProvider screenInfoProvider;
        private readonly ImageCaptureServer imageCaptureServer;
        private readonly Server commandServer;

        private CancellationTokenSource cancellationTokenSource;
        private int disposed;

        public VncServer(IScreenInfoProvider screenInfoProvider, ushort listenerPort = 0)
        {
            this.screenInfoProvider = screenInfoProvider ?? throw new ArgumentNullException(nameof(screenInfoProvider));

            imageCaptureServer = new ImageCaptureServer(new ScreenRecorderImageSource(screenInfoProvider.GetBounds()), screenInfoProvider.Id);
            imageCaptureServer.ErrorOccurred += ImageCaptureServer_ErrorOccurred;

            commandServer = new Server(listenerPort: listenerPort);
            commandServer.DataArrived += CommandServer_DataArrived;
            commandServer.ErrorOccurred += CommandServer_ErrorOccurred;
        }

        public ImageCaptureServer ImageCaptureServer => imageCaptureServer;

        public Server CommandServer => commandServer;

        public string CommandServerIpAddress => CommandServer?.Socket?.GetLocalIPAddresses().FirstOrDefault(ip => ip.StartsWith("192.")) ?? CommandServer?.Socket?.GetLocalIPAddresses().FirstOrDefault();
        
        public string ImageCaptureServerIpAddress => imageCaptureServer?.Server?.Socket?.GetLocalIPAddresses().FirstOrDefault();

        public string ImageCaptureServerInfo => ImageCaptureServer?.Server?.Socket?.GetLocalIPAddressesInfo();

        public string CommandServerInfo => CommandServer?.Socket?.GetLocalIPAddressesInfo();

        public void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            imageCaptureServer.StartVideoCaptureServer(cancellationTokenSource);
            commandServer.Start();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            imageCaptureServer.Stop();
            commandServer.Stop();
        }

        public override string ToString()
        {
            return $"Commands: {CommandServerInfo}   -   Images: {ImageCaptureServerInfo}";
        }

        protected virtual void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        private void ImageCaptureServer_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e.Exception));
        }

        private void CommandServer_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e.Exception));
        }

        private async void CommandServer_DataArrived(object sender, DataArrivedEventArgs e)
        {
            try
            {
                var vncServer = e.Socket;
                var message = commandServer.Encoding.GetString(e.Data);
                Console.WriteLine($"Message arrived to VNC server: {message}");

                if (message == VncCommand.GetScreenRecorderPort)
                {
                    var screenSizeMessage = $"{VncCommand.ScreenRecorderPortResponse}{VncCommand.Separator}{imageCaptureServer.Server.ListenerPortOfServer}";
                    commandServer.SendMessageToAllClients(screenSizeMessage);
                }
                else if (message == VncCommand.GetScreenSize)
                {
                    var size = screenInfoProvider.GetPrimaryScreenSize();
                    var screenSizeMessage = $"{VncCommand.ScreenSize}{VncCommand.Separator}{size.Width}x{size.Height}";
                    commandServer.Send(e.Socket, screenSizeMessage);
                }
                else if (message.StartsWith($"{VncCommand.KillApp} ", StringComparison.Ordinal))
                {
                    ProcessUtils.KillProcesses(message.Substring(8));
                }
                else if (message.StartsWith($"{VncCommand.KillApp} ", StringComparison.Ordinal))
                {
                    ProcessUtils.KillProcesses(message.Substring(8));
                }
                else if (message.IndexOf(VncCommand.Mouse, StringComparison.Ordinal) > Constants.NotFound)
                {
                    await MouseHandler.ProcessMessageAsync(message).ConfigureAwait(false);
                }
                else if (message.StartsWith(VncCommand.KeyPressed, StringComparison.Ordinal))
                {
                    var data = message.Split(' ');
                    KeyboardSimulator.SendChar(data[1]);
                }
                else if (message.StartsWith(VncCommand.Scroll, StringComparison.Ordinal))
                {
                    var values = message.Split(new string[] { $"{VncCommand.Scroll} " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values)
                    {
                        if (Int32.TryParse(value, out var scrollValue))
                        {
                            // scrollValue < 0 => scrolls down
                            WinAPI.MouseEvent(WinAPI.MOUSEEVENTF_WHEEL, 0, 0, scrollValue, 0);
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(message))
                    {
                        var command = message.GetProgramAndParameters();
                        var parameters = String.Join(" ", command.Skip(1).Select(param => param.Contains(' ') && !param.StartsWith("\"", StringComparison.Ordinal) ? $"\"{param}\"" : param));
                        ProcessUtils.RunProgramOrFile(command[0], parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        ~VncServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            if (disposing)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                imageCaptureServer.Stop();
                imageCaptureServer.Dispose();
                commandServer.Stop();
                commandServer.Dispose();
            }
        }
    }
}
