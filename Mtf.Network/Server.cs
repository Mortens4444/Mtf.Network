using Mtf.Cryptography.Interfaces;
using Mtf.Extensions;
using Mtf.Network.Commands;
using Mtf.Network.Interfaces;
using Mtf.Network.Models;
using Mtf.Network.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class Server : Communicator, IServer
    {
        public ConcurrentDictionary<Socket, string> ConnectedClients { get; private set; } = new ConcurrentDictionary<Socket, string>();
        
        public ConcurrentDictionary<Socket, RSAParameters> ClientPublicKeys { get; private set; } = new ConcurrentDictionary<Socket, RSAParameters>();

        public IPAddress IpAddress { get; private set; }

        private List<ICommand> RegisteredCommands = LoadCommands(typeof(RsaKeyCommand).Namespace);

        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            IPAddress ipAddress = null,
            ushort listenerPort = 0,
            params ICipher[] ciphers) : base(addressFamily, socketType, protocolType, listenerPort, ciphers)
        {
            IpAddress = ipAddress ?? IPAddress.Any;
        }

        /// <summary>
        /// Starts listening on ListenerPort
        /// </summary>
        public void Start()
        {
            Stop();
            Initialize();
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

        private void Initialize()
        {
            Socket = NetUtils.CreateSocket(IpAddress, ListenerPortOfServer, AddressFamily, SocketType, ProtocolType, true);
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

                ConnectedClients.TryAdd(clientSocket, clientSocket.RemoteEndPoint.ToString());

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

                        var message = Encoding.GetString(bytes);
                        foreach (var command in RegisteredCommands)
                        {
                            if (command.CanHandle(message))
                            {
                                command.Execute(message, clientSocket, this);
                                return;
                            }
                        }

                        OnDataArrived(clientSocket, bytes);
                        state.ReadFromSocket(ServerReadCallback);
                    }
                    else
                    {
                        Console.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} disconnected gracefully.");
                        ConnectedClients.TryRemove(clientSocket, out _);
                        clientSocket.CloseSocket();
                    }
                }
                else
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} detected as disconnected before EndReceive.");
                    ConnectedClients.TryRemove(clientSocket, out _);
                    clientSocket.CloseSocket();
                }
            }
            catch (SocketException ex)
                when (ex.SocketErrorCode == SocketError.ConnectionReset || // WSAECONNRESET
                    ex.SocketErrorCode == SocketError.ConnectionAborted || // WSAECONNABORTED
                    ex.SocketErrorCode == SocketError.Shutdown) // WSANOTINITIALISED
            {
                Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} disconnected abruptly (Error: {ex.SocketErrorCode}).");
                ConnectedClients.TryRemove(clientSocket, out _);
                clientSocket.CloseSocket();
                OnErrorOccurred(ex);
            }
            catch (ObjectDisposedException ex)
            {
                Console.Error.WriteLine($"{nameof(Server)} {nameof(ServerReadCallback)} - Client {clientSocket?.RemoteEndPoint} socket was already disposed.");
                ConnectedClients.TryRemove(clientSocket, out _);
                OnErrorOccurred(ex);
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine($"Error reading from client {clientSocket?.RemoteEndPoint}: {ex.Message}");
                ConnectedClients.TryRemove(clientSocket, out _);
                clientSocket.CloseSocket();
                OnErrorOccurred(ex);
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
                catch (ObjectDisposedException ex)
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ListenerEngine)} - Socket was disposed.");
                    OnErrorOccurred(ex);
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{nameof(Server)} {nameof(ListenerEngine)} - Unexpected error: {ex.Message}");
                    OnErrorOccurred(ex);
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            foreach (var client in ConnectedClients.Keys)
            {
                client.CloseSocket();
            }
            ConnectedClients.Clear();
        }

        public bool SendBytesToAllClients(byte[] data, bool appendNewLine = false)
        {
            var result = true;
            foreach (var clientSocket in ConnectedClients.Keys.ToList())
            {
                try
                {
                    result &= Send(clientSocket, data, appendNewLine);
                }
                catch (Exception ex)
                {
                    ConnectedClients.TryRemove(clientSocket, out var value);
                    if (Logger != null)
                    {
                        LogErrorAction(Logger, this, $"Sending data failed to {value}", ex);
                    }
                    OnErrorOccurred(ex);
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

        private static List<ICommand> LoadCommands(string targetNamespace)
        {
            var commandType = typeof(ICommand);
            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => commandType.IsAssignableFrom(t))
                .Where(t => t.Namespace == targetNamespace);

            var instances = new List<ICommand>();
            foreach (var type in types)
            {
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var instance = (ICommand)Activator.CreateInstance(type);
                    instances.Add(instance);
                }
            }

            return instances;
        }
    }
}
