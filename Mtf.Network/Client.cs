using Mtf.Network.Exceptions;
using Mtf.Network.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class Client : Communicator
    {
        public Client(string serverHost, ushort listenerPort, AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
            : base(addressFamily, socketType, protocolType, listenerPort)
        {
            ServerHostnameOrIPAddress = serverHost;
            CreateSocket(addressFamily, socketType, protocolType);
            SetSocketTimeout(Socket, Constants.SocketConnectionTimeout);
        }

        public string ServerHostnameOrIPAddress { get; set; }

        public int ListenerPortOfClient => ((IPEndPoint)Socket.LocalEndPoint)?.Port ?? Constants.NotFound;

        public void Connect()
        {
            if (!Socket.Connected)
            {
                CancellationTokenSource = new CancellationTokenSource();
                var result = Socket.BeginConnect(ServerHostnameOrIPAddress, ListenerPortOfServer, null, null);
                if (!result.AsyncWaitHandle.WaitOne(Constants.SocketConnectionTimeout))
                {
                    throw new ConnectionFailedException(ServerHostnameOrIPAddress, ListenerPortOfServer);
                }

                _ = Task.Run(Receiver);
            }
        }

        public void Send(string message, bool appendNewLine = false)
        {
            var messageToSend = appendNewLine ? String.Concat(message, Environment.NewLine) : message;
            var data = Encoding.GetBytes(messageToSend);
            _ = Send(data);
            OnMessageSent(messageToSend);
        }

        protected override void DisposeManagedResources()
        {
            NetUtils.CloseSocket(Socket);
            Socket = null;
        }

        private void CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            Socket = new Socket(addressFamily, socketType, protocolType)
            {
                DontFragment = true
            };
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, Constants.MaxBufferSize);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, Constants.MaxBufferSize);
            if (protocolType != ProtocolType.Udp)
            {
                Socket.NoDelay = true;
            }
        }

        private async Task Receiver()
        {
            using (var receiveEventArgs = new SocketAsyncEventArgs())
            {
                var receiveBuffer = new byte[Constants.MaxBufferSize];
                receiveEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

                while (!CancellationTokenSource.Token.IsCancellationRequested && NetUtils.IsSocketConnected(Socket))
                {
                    var taskCompletionSource = new TaskCompletionSource<int>();

                    void completedHandler(object s, SocketAsyncEventArgs e)
                    {
                        _ = e.SocketError == SocketError.Success
                            ? taskCompletionSource.TrySetResult(e.BytesTransferred)
                            : taskCompletionSource.TrySetException(new SocketException((int)e.SocketError));
                    }
                    receiveEventArgs.Completed += completedHandler;
                    try
                    {
                        if (!Socket.ReceiveAsync(receiveEventArgs))
                        {
                            taskCompletionSource.SetResult(receiveEventArgs.BytesTransferred);
                        }

                        var readBytes = await taskCompletionSource.Task.ConfigureAwait(false);

                        if (readBytes > 0)
                        {
                            var data = new byte[readBytes];
                            Array.Copy(receiveBuffer, data, readBytes);
                            OnDataArrived(Socket, data);
                        }

                        await Task.Delay(1).ConfigureAwait(false);
                    }
                    finally
                    {
                        receiveEventArgs.Completed -= completedHandler;
                    }
                }
            }
        }
    }
}
