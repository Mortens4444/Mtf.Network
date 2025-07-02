using Microsoft.Extensions.Logging;
using Mtf.Cryptography.Interfaces;
using Mtf.Extensions;
using Mtf.Network.EventArg;
using Mtf.Network.Services;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class Communicator : Disposable, ICommunicator
    {
        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<MessageEventArgs> MessageSent;

        protected Action<ILogger, Communicator, string, Exception> LogErrorAction { get; }
        public ICipher[] Ciphers { get; }

        public Communicator(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, ushort listenerPortOfServer, params ICipher[] ciphers)
        {
            ListenerPortOfServer = listenerPortOfServer;
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            this.Ciphers = ciphers;
            LogErrorAction = LoggerMessage.Define<Communicator, string>(LogLevel.Error, new EventId(1, "SerialDevice"), "Error occurred in communicator: {Device}, {EventDetails}");
        }

        public ILogger Logger { get; set; }

        public AddressFamily AddressFamily { get; private set; }

        public SocketType SocketType { get; private set; }

        public ProtocolType ProtocolType { get; private set; }

        public Socket Socket { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public ushort ListenerPortOfServer { get; set; }

        protected int BufferSize { get; set; } = Constants.MaxBufferSize;

        public void SetBufferSize(int bufferSize = Constants.MaxBufferSize)
        {
            BufferSize = bufferSize;
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, bufferSize);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, bufferSize);
        }

        /// <summary>
        /// A value that specifies the amount of time after which a synchronous Overload:System.Net.Sockets.Socket.Send call will time out.
        /// </summary>
        /// <param name="value">The time-out value, in milliseconds. If you set the property with a value between 1 and 499, the value will be changed to 500.
        /// The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</param>
        public void SetTimeout(int value = 0)
        {
            SetSocketTimeout(Socket, value);
        }

        public bool Send(string message, bool appendNewLine = false)
        {
            var result = Send(Socket, message, appendNewLine);
            if (result)
            {
                OnMessageSent(message);
            }
            return result;
        }

        public async Task<bool> SendAsync(string message, bool appendNewLine = false)
        {
            var result = await SendAsync(Socket, message, appendNewLine).ConfigureAwait(false);
            if (result)
            {
                OnMessageSent(message);
            }
            return result;
        }

        public bool Send(byte[] bytes, bool appendNewLine = false)
        {
            try
            {
                if (Send(Socket, bytes))
                {
                    if (appendNewLine)
                    {
                        Socket.Send(Encoding.GetBytes(Environment.NewLine));
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendAsync(byte[] bytes, bool appendNewLine = false)
        {
            if (Socket?.Connected != true)
                return false;

            try
            {
                if (!await SendAsync(Socket, bytes).ConfigureAwait(false))
                {
                    return false;
                }

                if (appendNewLine)
                {
                    var newLineBytes = Encoding.GetBytes(Environment.NewLine);
                    if (!await SendAsync(Socket, newLineBytes).ConfigureAwait(false))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Task<bool> SendAsync(Socket socket, byte[] data)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (socket?.Connected != true)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            using (var args = new SocketAsyncEventArgs())
            {
                args.SetBuffer(data, 0, data.Length);

                EventHandler<SocketAsyncEventArgs> handler = null;
                handler = (s, e) =>
                {
                    args.Completed -= handler;
                    args.Dispose();

                    var success = e.SocketError == SocketError.Success && e.BytesTransferred == data.Length;
                    tcs.SetResult(success);
                };

                args.Completed += handler;

                if (!socket.SendAsync(args))
                {
                    handler(socket, args);
                }
            }

            return tcs.Task;
        }

        public bool Send(Socket socket, string message, bool appendNewLine = false)
        {
            return Send(socket, ConvertMessageToData(message, appendNewLine));
        }

        public Task<bool> SendAsync(Socket socket, string message, bool appendNewLine = false)
        {
            return Communicator.SendAsync(socket, ConvertMessageToData(message, appendNewLine));
        }

        public bool Send(Socket socket, byte[] bytes, bool appendNewLine = false)
        {
            if (socket?.Connected != true)
            {
                return false;
            }

            bytes = Transform(bytes, true);
            var success = socket.Send(bytes, SocketFlags.None) == bytes?.Length;
            if (success && appendNewLine)
            {
                var enterBytes = Encoding.GetBytes(Environment.NewLine);
                success &= socket.Send(enterBytes) == enterBytes.Length;
            }
            return success;
        }

        public Task<bool> SendAsync(Socket socket, byte[] bytes, bool appendNewLine = false)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (socket?.Connected != true)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            bytes = Transform(bytes, true);

            using (var args = new SocketAsyncEventArgs())
            {
                args.SetBuffer(bytes, 0, bytes.Length);

                EventHandler<SocketAsyncEventArgs> handler = null;
                handler = (s, e) =>
                {
                    args.Completed -= handler;
                    args.Dispose();

                    if (e.SocketError == SocketError.Success && e.BytesTransferred == bytes.Length)
                    {
                        if (appendNewLine)
                        {
                            SendNewLine(socket).ContinueWith(t =>
                            {
                                tcs.SetResult(t.Result);
                            }, TaskScheduler.Default);
                        }
                        else
                        {
                            tcs.SetResult(true);
                        }
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                };

                args.Completed += handler;

                if (!socket.SendAsync(args))
                {
                    handler(socket, args);
                }
            }

            return tcs.Task;
        }

        protected void SendPublicKey(IAsymmetricCipher cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException(nameof(cipher));
            }

            var base64Key = Convert.ToBase64String(cipher.PublicKey);
            var message = $"RSA key:{base64Key}";
            Send(Socket, Encoding.GetBytes(message), true); // \n, ha kell
        }

        private Task<bool> SendNewLine(Socket socket)
        {
            var tcs = new TaskCompletionSource<bool>();
            var newLineBytes = Encoding.GetBytes(Environment.NewLine);

            using (var args = new SocketAsyncEventArgs())
            {
                args.SetBuffer(newLineBytes, 0, newLineBytes.Length);

                EventHandler<SocketAsyncEventArgs> handler = null;
                handler = (s, e) =>
                {
                    args.Completed -= handler;
                    args.Dispose();

                    var ok = e.SocketError == SocketError.Success && e.BytesTransferred == newLineBytes.Length;
                    tcs.SetResult(ok);
                };

                args.Completed += handler;

                if (!socket.SendAsync(args))
                {
                    handler(socket, args);
                }
            }

            return tcs.Task;
        }

        protected void OnDataArrived(Socket socket, byte[] data)
        {
            data = Transform(data, false);
            DataArrived?.Invoke(this, new DataArrivedEventArgs(socket, data));
        }

        protected void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        protected void OnMessageSent(string message)
        {
            MessageSent?.Invoke(this, new MessageEventArgs(message));
        }

        protected static void SetSocketTimeout(Socket socket, int value)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
        }

        protected override void DisposeManagedResources()
        {
            if (Socket.IsSocketConnected())
            {
                Socket.CloseSocket();
                Socket = null;
            }
        }

        protected static string TransformMessage(string message, bool appendNewLine)
        {
            return appendNewLine ? String.Concat(message, Environment.NewLine) : message;
        }

        protected byte[] ConvertMessageToData(string message, bool appendNewLine)
        {
            var messageToSend = TransformMessage(message, appendNewLine);
            return Encoding.GetBytes(messageToSend);
        }

        public void SendAsymmetricCiphersPublicKeys()
        {
            foreach (var cipher in Ciphers)
            {
                if (cipher is IAsymmetricCipher asymmetricCipher)
                {
                    SendPublicKey(asymmetricCipher);
                }
            }
        }

        public override string ToString()
        {
            if (Socket == null)
            {
                return "Not listening...";
            }

            var result = $"{Socket.LocalEndPoint}";
            if (result.StartsWith(SocketExtensions.IpAny, StringComparison.Ordinal))
            {
                result = result.Replace(SocketExtensions.IpAnyWithoutColon, Socket.GetLocalIPAddressesInfo(NetUtils.GetLocalIPAddresses, "|"));
            }
            return result;
        }

        private byte[] Transform(byte[] data, bool encrypt)
        {
            if (Ciphers != null)
            {
                if (encrypt)
                {
                    for (int i = 0; i < Ciphers.Length; i++)
                    {
                        data = Ciphers[i].Encrypt(data);
                    }
                }
                else
                {
                    for (int i = Ciphers.Length - 1; i >= 0; i--)
                    {
                        data = Ciphers[i].Decrypt(data);
                    }
                }
            }
            return data;
        }
    }
}
