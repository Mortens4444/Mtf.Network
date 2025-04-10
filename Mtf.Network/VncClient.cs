using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Services;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Mtf.Network.VncServer;

namespace Mtf.Network
{
    public class VncClient : Socket
    {
        public ushort ListenerPortOfServer { get; set; }
        public Encoding Encoding = Encoding.UTF8;
        private readonly string serverHost;

        public event DataArrivedEventHandler DataArrived;
        public event ErrorOccurredEventHandler ErrorOccurred;

        public VncClient(string serverHost, ushort listenerPort)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            ListenerPortOfServer = listenerPort;
            this.serverHost = serverHost;

            ReceiveTimeout = Constants.SocketConnectionTimeout;
            SendTimeout = Constants.SocketConnectionTimeout;
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, Constants.MaxBufferSize);
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, Constants.MaxBufferSize);

            LingerState = new LingerOption(true, 1);
            NoDelay = true;
            DontFragment = true;

            var result = BeginConnect(this.serverHost, ListenerPortOfServer, null, null);
            var success = result.AsyncWaitHandle.WaitOne(Constants.SocketConnectionTimeout, true);
            if (!success)
            {
                throw new InvalidDataException("Check if VNC server is running, check the service port and the firewall settings");
            }

            DataArrived += DataArrivedHandlerAsync;
            Task.Run(Receiver);
        }

        public bool Send(string message)
        {
            return SendBytes(Encoding.GetBytes(message));
        }

        public Task<bool> SendAsync(string message)
        {
            return SendBytesAsync(Encoding.GetBytes(message));
        }

        public bool SendBytes(byte[] bytes)
        {
            return MessageSender.Send(this, bytes);
        }

        public Task<bool> SendBytesAsync(byte[] bytes)
        {
            return MessageSender.SendAsync(this, bytes);
        }

        protected virtual void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        protected virtual void OnDataArrived(DataArrivedEventArgs e)
        {
            DataArrived?.Invoke(this, e);
        }

        public void SetNewDataArriveEventHandler(DataArrivedEventHandler dataArrivedEventHandler)
        {
            if (dataArrivedEventHandler != null)
            {
                DataArrived -= DataArrivedHandlerAsync;
                DataArrived += dataArrivedEventHandler;
            }
        }

        public async Task DataArrivedHandlerAsync(object sender, DataArrivedEventArgs e)
        {
            var message = await GetMessageAsync(sender, e).ConfigureAwait(false);
            switch (message)
            {
                case VncCommand.ScreenSize:
                    break;
                case "Unknown command":
                    OnErrorOccurred(new InvalidDataException("Server could not recognize the sent command"));
                    break;
                default:
                    OnErrorOccurred(new InvalidDataException("Server sent an unexpected message"));
                    break;
            }
        }

        public static Task<string> GetMessageAsync(object sender, DataArrivedEventArgs e)
        {
            return Task.Run(() =>
            {
                var vncClient = (VncClient)sender;
                return vncClient.Encoding.GetString(e.Data);
            });
        }

        private async Task Receiver()
        {
            try
            {
                while (Connected)
                {
                    if (Available > 0)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                        var readable = Available;

                        var receiveBuffer = new byte[readable];
                        var readBytes = Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);
                        if (readBytes > 0)
                        {
                            var s = new string(Encoding.GetChars(receiveBuffer, 0, readBytes));
                            OnDataArrived(new DataArrivedEventArgs(this, receiveBuffer));
                        }
                    }
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            catch (SocketException) { }
        }
    }
}
