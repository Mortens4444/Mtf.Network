using Mtf.Network.Extensions;
using Mtf.Network.Interfaces;
using Mtf.Network.Models;
using Mtf.Network.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            ushort listenerPort = 0,
            params ICipher[] ciphers) : base(addressFamily, socketType, protocolType, listenerPort, ciphers)
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
            CancellationTokenSource?.Cancel();
            Socket.CloseSocket();
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
            try
            {
                var clientSocket = Socket.EndAccept(ar);

                var state = new StateObject
                {
                    Socket = clientSocket
                };

                connectedClients.TryAdd(clientSocket, clientSocket.RemoteEndPoint.ToString());

                state.ReadFromSocket(ServerReadCallback);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
            finally
            {
                Socket.BeginAccept(AcceptCallback, null);
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
                if (clientSocket.IsSocketConnected())
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
                        Console.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} disconnected gracefully.");
                        connectedClients.TryRemove(clientSocket, out _);
                        clientSocket.CloseSocket();
                    }
                }
                else
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} detected as disconnected before EndReceive.");
                    connectedClients.TryRemove(clientSocket, out _);
                    clientSocket.CloseSocket();
                }
            }
            catch (SocketException ex)
                when (ex.SocketErrorCode == SocketError.ConnectionReset || // WSAECONNRESET
                    ex.SocketErrorCode == SocketError.ConnectionAborted || // WSAECONNABORTED
                    ex.SocketErrorCode == SocketError.Shutdown) // WSANOTINITIALISED
            {
                Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} disconnected abruptly (Error: {ex.SocketErrorCode}).");
                connectedClients.TryRemove(clientSocket, out _);
                clientSocket.CloseSocket();
            }
            catch (ObjectDisposedException)
            {
                Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} socket was already disposed.");
                connectedClients.TryRemove(clientSocket, out _);
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine($"Error reading from client {clientSocket?.RemoteEndPoint}: {ex.Message}");
                connectedClients.TryRemove(clientSocket, out _);
                clientSocket.CloseSocket();
            }
        }

        private void ListenerEngine()
        {
            while (!(CancellationTokenSource?.Token.IsCancellationRequested ?? true))
            {
                try
                {
                    if (Socket != null && Socket.Poll(10, SelectMode.SelectRead))
                    {
                        _ = Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ListenerEngine)} - Socket was disposed.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ListenerEngine)} - Unexpected error: {ex.Message}");
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            foreach (var client in connectedClients.Keys)
            {
                client.CloseSocket();
            }
            connectedClients.Clear();
        }

        public bool SendBytesToAllClients(byte[] data, bool appendNewLine = false)
        {
            var result = true;
            foreach (var clientSocket in connectedClients.Keys.ToList())
            {
                try
                {
                    result &= Send(clientSocket, data, appendNewLine);
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
            return result;
        }

        public bool SendMessageToAllClients(string message, bool appendNewLine = false)
        {
            return SendBytesToAllClients(ConvertMessageToData(message, appendNewLine));
        }

        public bool SendMessageToClient(Socket clientSocket, string message, bool appendNewLine = false)
        {
            var data = Encoding.GetBytes(message);
            return Send(clientSocket, data, appendNewLine);
        }

        public void SendBytesInChunksToAllClients(byte[] header, byte[] data)
        {
            SendBytesInChunksToAllClients(header);
            SendBytesInChunksToAllClients(data);
        }

        public bool SendBytesInChunksToAllClients(byte[] data, int headerSize = 0)
        {
            if (data == null)
            {
                return false;
            }

            var result = true;
            var chunkSize = Socket.SendBufferSize - headerSize;
            var totalParts = (int)Math.Ceiling((double)data.Length / chunkSize);
            Debug.WriteLine($"{nameof(Server)} - Sending data in {totalParts} chunk(s).");
            for (int i = 0; i < totalParts; i++)
            {
                var offset = i * chunkSize;
                var partSize = Math.Min(chunkSize, data.Length - offset);
                var partBytes = new byte[partSize];
                Buffer.BlockCopy(data, offset, partBytes, 0, partSize);
                result &= SendBytesToAllClients(partBytes, true);
            }
            return result;
        }
    }
}
