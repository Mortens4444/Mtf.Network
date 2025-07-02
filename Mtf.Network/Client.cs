using Mtf.Cryptography.Interfaces;
using Mtf.Extensions;
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
        private Task receiverTask;

        public Client(Server server, AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            params ICipher[] ciphers) : this(server.IpAddress.ToString(), server.ListenerPortOfServer,
                addressFamily, socketType, protocolType, ciphers)
        { }

        public Client(string serverHost,
            ushort listenerPort,
            AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            params ICipher[] ciphers)
            : base(addressFamily, socketType, protocolType, listenerPort, ciphers)
        {
            ServerHostnameOrIPAddress = serverHost;
            CreateSocket();
        }

        public int Timeout { get; set; } = Constants.SocketConnectionTimeout;

        public string ServerHostnameOrIPAddress { get; set; }

        public int ListenerPortOfClient => ((IPEndPoint)Socket.LocalEndPoint)?.Port ?? Constants.NotFound;

        public void Connect()
        {
            if (!Socket.Connected)
            {
                CancellationTokenSource = new CancellationTokenSource();
                Socket.Connect(ServerHostnameOrIPAddress, ListenerPortOfServer, Timeout, NetUtils.GetLocalIPAddresses);

                SendAsymmetricCiphersPublicKeys();

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

        private void CreateSocket()
        {
            Socket = new Socket(AddressFamily, SocketType, ProtocolType)
            {
                DontFragment = true
            };
            SetBufferSize();
            if (ProtocolType != ProtocolType.Udp)
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
            while (Socket.IsSocketConnected())
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
