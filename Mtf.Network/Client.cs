using Mtf.Network.Exceptions;
using Mtf.Network.Services;
using System;
using System.Net;
using System.Net.Sockets;
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
        }

        public int Timeout { get; set; } = Constants.SocketConnectionTimeout;

        public string ServerHostnameOrIPAddress { get; set; }

        public int ListenerPortOfClient => ((IPEndPoint)Socket.LocalEndPoint)?.Port ?? Constants.NotFound;
        private Task receiverTask;

        public void Connect()
        {
            if (!Socket.Connected)
            {
                CancellationTokenSource = new CancellationTokenSource();
                var result = Socket.BeginConnect(ServerHostnameOrIPAddress, ListenerPortOfServer, null, null);
                if (!result.AsyncWaitHandle.WaitOne(Timeout))
                {
                    throw new ConnectionFailedException(ServerHostnameOrIPAddress, ListenerPortOfServer);
                }

                receiverTask = Task.Run(Receiver, CancellationTokenSource.Token);
            }
        }

        public void Disconnect()
        {
            if (Socket.Connected)
            {
                Socket.Disconnect(true);
            }
        }

        protected override void DisposeManagedResources()
        {
            if (receiverTask != null)
            {
                var disposeTask = Task.WhenAny(receiverTask, Task.Delay(Constants.DisposeTimeout));
                disposeTask.Wait();
            }
            base.DisposeManagedResources();
        }

        private void CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            Socket = new Socket(addressFamily, socketType, protocolType)
            {
                DontFragment = true
            };
            SetBufferSize();
            if (protocolType != ProtocolType.Udp)
            {
                Socket.NoDelay = true;
            }
        }

        private void Receiver()
        {
            if (Socket == null)
            {
                throw new InvalidOperationException("Socket is not initialized.");
            }

            var receiveBuffer = new byte[BufferSize];
            while (NetUtils.IsSocketConnected(Socket))
            {
                try
                {
                    var num = Socket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);
                    if (num > 0)
                    {
                        var receivedData = new byte[num];
                        Array.Copy(receiveBuffer, receivedData, num);
                        OnDataArrived(Socket, receivedData);
                    }
                }
                catch (SocketException ex)
                {
                    OnErrorOccurred(ex);
                    break;
                }
            }
        }
    }
}
