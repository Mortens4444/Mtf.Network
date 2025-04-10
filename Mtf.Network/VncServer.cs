using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Extensions;
using Mtf.Network.Interfaces;
using Mtf.Network.Models;
using Mtf.Network.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class VncServer : Socket
    {
        public delegate Task DataArrivedEventHandler(object sender, DataArrivedEventArgs e);
        public delegate Task ErrorOccurredEventHandler(object sender, ExceptionEventArgs e);

        public int ListenerPortOfServer { get; set; }
        public Encoding Encoding = Encoding.UTF8;

        public event DataArrivedEventHandler DataArrived;
        public event ErrorOccurredEventHandler ErrorOccurred;

        private Socket clientSocket;
        private CancellationTokenSource cancellationTokenSource;
        private IScreenInfoProvider screenInfoProvider;

        public VncServer(IScreenInfoProvider screenInfoProvider, int listenerPort = 0)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            cancellationTokenSource = new CancellationTokenSource();
            if (listenerPort == 0)
            {
                listenerPort = NetUtils.GetFreePort();
            }
            this.screenInfoProvider = screenInfoProvider;
            ListenerPortOfServer = listenerPort;

            Bind(new IPEndPoint(IPAddress.Any, listenerPort));
            Listen(Constants.MaxPendingConnections);
            DataArrived += DataArrivedHandlerAsync;
            _ = Task.Run(ListenerEngine);
            _ = Task.Run(() => StartScreenSender(screenInfoProvider.GetBounds()));
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        public void StartScreenSender(Rectangle rectangle)
        {
            _ = Task.Run(() => ScreenSenderAsync(rectangle));

        }
        private async Task ScreenSenderAsync(Rectangle rectangle)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(Constants.ScreenSendDelayMs).ConfigureAwait(false);
                
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }
                
                var socket = Interlocked.Exchange(ref clientSocket, null);
                if (socket == null)
                {
                    continue;
                }

                var image = ImageUtils.GetScreenAreaInByteArray(rectangle, Encoding);
                await SendAsync(socket, image).ConfigureAwait(false);
            }
        }

        public bool Send(Socket socket, string message)
        {
            return MessageSender.Send(socket, Encoding.GetBytes(message));
        }

        public bool Send(Socket socket, byte[] bytes)
        {
            return MessageSender.Send(socket, bytes);
        }

        public Task<bool> SendAsync(Socket socket, string message)
        {
            return MessageSender.SendAsync(socket, Encoding.GetBytes(message));
        }

        public Task<bool> SendAsync(Socket socket, byte[] bytes)
        {
            return MessageSender.SendAsync(socket, bytes);
        }

        private void ListenerEngine()
        {
            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (Poll(10, SelectMode.SelectRead))
                    {
                        BeginAccept(new AsyncCallback(AcceptCallback), this);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var state = new StateObject
                {
                    Socket = ((Socket)ar.AsyncState).EndAccept(ar)
                };
                state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ServerReadCallback, state);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                var handler = state.Socket;
                if (handler.Connected)
                {
                    int read = handler.EndReceive(ar);
                    if (read > 0)
                    {
                        byte[] data = new byte[read];
                        Array.Copy(state.Buffer, 0, data, 0, read);
                        OnDataArrived(new DataArrivedEventArgs(handler, data));
                        handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ServerReadCallback, state);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        protected virtual void OnDataArrived(DataArrivedEventArgs e)
        {
            DataArrived?.Invoke(this, e);
        }

        protected virtual void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        private async Task DataArrivedHandlerAsync(object sender, DataArrivedEventArgs e)
        {
            try
            {
                var vncServer = (VncServer)sender;
                var message = vncServer.Encoding.GetString(e.Data);

                while (message.Contains(VncCommand.GetScreen))
                {
                    clientSocket = e.Socket;
                    message = message.Remove(VncCommand.GetScreen);
                }

                if (message == VncCommand.GetScreenSize)
                {
                    var size = screenInfoProvider.GetPrimaryScreenSize();
                    var screenSizeMessage = $"{VncCommand.ScreenSize}{VncCommand.Separator}{size.Width}x{size.Height}";
                    await vncServer.SendAsync(e.Socket, screenSizeMessage);
                }
                else if (message.StartsWith($"{VncCommand.KillApp} "))
                {
                    ProcessUtils.KillProcesses(message.Substring(8));
                }
                else if (message.IndexOf(VncCommand.Mouse) > Constants.NotFound)
                {
                    await MouseHandler.ProcessMessageAsync(message);
                }
                else if (message.StartsWith(VncCommand.KeyPressed))
                {
                    var data = message.Split(' ');
                    KeyboardSimulator.SendChar(data[1]);
                }
                else if (message.StartsWith(VncCommand.Scrool))
                {
                    var values = message.Split(new string[] { $"{VncCommand.Scrool} " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values)
                    {
                        var scroolValue = Convert.ToInt32(value);
                        // scroolValue < 0 => scrools down
                        WinAPI.MouseEvent(WinAPI.MOUSEEVENTF_WHEEL, 0, 0, scroolValue, 0);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(message))
                    {
                        var command = message.GetProgramAndParameters();
                        var parameters = String.Join(" ", command.Skip(1).Select(param => param.Contains(' ') && !param.StartsWith("\"") ? $"\"{param}\"" : param));
                        ProcessUtils.RunProgramOrFile(command[0], parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }
    }
}
