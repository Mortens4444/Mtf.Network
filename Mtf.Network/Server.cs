using Mtf.Network.Models;
using Mtf.Network.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class Server : Communicator
    {
        private readonly ConcurrentDictionary<Socket, string> connectedClients = new ConcurrentDictionary<Socket, string>();

        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp,
            ushort listenerPort = 0) : base(addressFamily, socketType, protocolType, listenerPort)
        {
        }

        /// <summary>
        /// Starts listening on ListenerPort
        /// </summary>
        public void Start()
        {
            Stop();
            Initialize(AddressFamily, SocketType, ProtocolType);
            CancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(ListenerEngine, CancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops listening on ListenerPort
        /// </summary>
        public void Stop()
        {
            NetUtils.CloseSocket(Socket);
        }

        public override bool Equals(object obj)
        {
            return obj is Server server && server.ListenerPortOfServer == ListenerPortOfServer;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void Initialize(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            Socket = NetUtils.CreateSocket(IPAddress.Any, ListenerPortOfServer, addressFamily, socketType, protocolType, true);
            if (ListenerPortOfServer == 0)
            {
                ListenerPortOfServer = (ushort)((IPEndPoint)Socket.LocalEndPoint).Port;
            }

            SetBufferSize();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var state = new StateObject
            {
                Socket = ((Socket)ar.AsyncState).EndAccept(ar),
                Buffer = new byte[BufferSize]
            };

            state.ReadFromSocket(ServerReadCallback);
            connectedClients.TryAdd(state.Socket, state.Socket.RemoteEndPoint.ToString());
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            if (NetUtils.IsSocketConnected(state.Socket))
            {
                var read = state.Socket.EndReceive(ar);
                if (read > 0)
                {
                    var bytes = new byte[read];
                    Array.Copy(state.Buffer, 0, bytes, 0, read);
                    OnDataArrived(state.Socket, bytes);
                    state.ReadFromSocket(ServerReadCallback);
                }
                else
                {
                    connectedClients.TryRemove(state.Socket, out _);
                    state.Socket.Close();
                }
            }
        }

        private void ListenerEngine()
        {
            while (!(CancellationTokenSource?.Token.IsCancellationRequested ?? true))
            {
                if (Socket.Poll(10, SelectMode.SelectRead))
                {
                    _ = Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            foreach (var client in connectedClients.Keys)
            {
                NetUtils.CloseSocket(client);
            }
            connectedClients.Clear();
        }

        public void SendBytesToAllClients(byte[] data, bool appendNewLine = false)
        {
            foreach (var clientSocket in connectedClients.Keys.ToList())
            {
                try
                {
                    Send(clientSocket, data, appendNewLine);
                }
                catch (Exception ex)
                {
                    connectedClients.TryRemove(clientSocket, out var value);
                    if (Logger != null)
                    {
                        logErrorAction(Logger, this, $"Sending data failed to {value}", ex);
                    }
                }
            }
        }

        public void SendMessageToAllClients(string message, bool appendNewLine = false)
        {
            SendBytesToAllClients(ConvertMessageToData(message, appendNewLine));
        }

        public void SendMessageToClient(Socket clientSocket, string message, bool appendNewLine = false)
        {
            var data = Encoding.GetBytes(message);
            Send(clientSocket, data, appendNewLine);
        }

        public void SendBytesInChunksToAllClients(byte[] data, int headerSize = 0)
        {
            var chunkSize = Socket.SendBufferSize - headerSize;
            var totalParts = (int)Math.Ceiling((double)data.Length / chunkSize);
            for (int i = 0; i < totalParts; i++)
            {
                var offset = i * chunkSize;
                var partSize = Math.Min(chunkSize, data.Length - offset);
                var partBytes = new byte[partSize];
                Buffer.BlockCopy(data, offset, partBytes, 0, partSize);
                SendBytesToAllClients(partBytes, true);
            }
        }
    }
}
