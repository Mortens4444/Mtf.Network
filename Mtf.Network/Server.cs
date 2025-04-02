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
            if (!(ar.AsyncState is Socket clientSocket))
            {
                return;
            }

            try
            {
                var state = new StateObject
                {
                    Socket = clientSocket.EndAccept(ar),
                    Buffer = new byte[BufferSize]
                };

                state.ReadFromSocket(ServerReadCallback);
                connectedClients.TryAdd(state.Socket, state.Socket.RemoteEndPoint.ToString());
            }
            catch (SocketException ex)
                when (ex.SocketErrorCode == SocketError.ConnectionReset || // WSAECONNRESET
                    ex.SocketErrorCode == SocketError.ConnectionAborted || // WSAECONNABORTED
                    ex.SocketErrorCode == SocketError.Shutdown) // WSANOTINITIALISED
            {
                Console.WriteLine($"AcceptCallback - Client {clientSocket?.RemoteEndPoint} disconnected abruptly (Error: {ex.SocketErrorCode}).");
                NetUtils.CloseSocket(clientSocket);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"AcceptCallback - Client socket was already disposed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AcceptCallback - Error reading from client {clientSocket?.RemoteEndPoint}: {ex.Message}");
                NetUtils.CloseSocket(clientSocket);
            }
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            if (ar == null)
            {
                return;
            }
            var state = (StateObject)ar.AsyncState;
            if (state == null)
            {
                return;
            }

            var clientSocket = state.Socket;
            try
            {
                if (NetUtils.IsSocketConnected(clientSocket))
                {
                    var read = clientSocket.EndReceive(ar);
                    if (read > 0)
                    {
                        var bytes = new byte[read];
                        Array.Copy(state.Buffer, 0, bytes, 0, read);
                        OnDataArrived(clientSocket, bytes);
                        state.ReadFromSocket(ServerReadCallback);
                    }
                    else
                    {
                        Console.WriteLine($"Client {clientSocket.RemoteEndPoint} disconnected gracefully.");
                        connectedClients.TryRemove(clientSocket, out _);
                        NetUtils.CloseSocket(clientSocket);
                    }
                }
                else
                {
                    Console.WriteLine($"Client {clientSocket.RemoteEndPoint} detected as disconnected before EndReceive.");
                    connectedClients.TryRemove(clientSocket, out _);
                    NetUtils.CloseSocket(clientSocket);
                }
            }
            catch (SocketException ex)
                when (ex.SocketErrorCode == SocketError.ConnectionReset || // WSAECONNRESET
                    ex.SocketErrorCode == SocketError.ConnectionAborted || // WSAECONNABORTED
                    ex.SocketErrorCode == SocketError.Shutdown) // WSANOTINITIALISED
            {
                Console.WriteLine($"Client {clientSocket?.RemoteEndPoint} disconnected abruptly (Error: {ex.SocketErrorCode}).");
                connectedClients.TryRemove(clientSocket, out _);
                NetUtils.CloseSocket(clientSocket);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"Client {clientSocket?.RemoteEndPoint} socket was already disposed.");
                connectedClients.TryRemove(clientSocket, out _);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error reading from client {clientSocket?.RemoteEndPoint}: {ex.Message}");
                connectedClients.TryRemove(clientSocket, out _);
                NetUtils.CloseSocket(clientSocket);
            }
        }

        private void ListenerEngine()
        {
            while (!(CancellationTokenSource?.Token.IsCancellationRequested ?? true))
            {
                try
                {
                    if (Socket != null && Socket.Connected && Socket.Poll(10, SelectMode.SelectRead))
                    {
                        _ = Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("ListenerEngine - Socket was disposed.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ListenerEngine - Unexpected error: {ex.Message}");
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
