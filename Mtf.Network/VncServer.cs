using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Extensions;
using Mtf.Network.Interfaces;
using Mtf.Network.Services;
using System;
using System.Diagnostics;
using System.Drawing;
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
            if (screenInfoProvider == null)
            {
                throw new ArgumentNullException(nameof(screenInfoProvider));
            }

            this.screenInfoProvider = screenInfoProvider;

            imageCaptureServer = new ImageCaptureServer(new ScreenRecorderImageSource(screenInfoProvider.GetBounds()), screenInfoProvider.Id);
            imageCaptureServer.ErrorOccurred += ImageCaptureServer_ErrorOccurred;

            commandServer = new Server(listenerPort: listenerPort);
            commandServer.DataArrived += CommandServer_DataArrived;
            commandServer.ErrorOccurred += CommandServer_ErrorOccurred;
        }

        public string ImageCaptureServer => imageCaptureServer.Server.Socket.GetLocalIPAddresses();

        public string CommandServer => imageCaptureServer.Server.Socket.GetLocalIPAddresses();

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
            return $"Commands: {commandServer}   -   Images: {imageCaptureServer.Server}";
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

                if (message == VncCommand.GetScreenSize)
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
                else if (message == VncCommand.GetScreenRecorderPort)
                {
                    Debugger.Break();
                    var screenSizeMessage = $"{VncCommand.ScreenRecorderPortResponse}{VncCommand.Separator}{imageCaptureServer.Server.ListenerPortOfServer}";
                    if (commandServer.SendMessageToAllClients(screenSizeMessage))
                    {
                        Console.WriteLine("Sent");
                    }
                    else
                    {
                        Console.Error.WriteLine("Not sent");
                    }
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
                        if (Int32.TryParse(value, out var scroolValue))
                        {
                            // scroolValue < 0 => scrools down
                            WinAPI.MouseEvent(WinAPI.MOUSEEVENTF_WHEEL, 0, 0, scroolValue, 0);
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
