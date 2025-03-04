using Microsoft.Extensions.Logging;
using Mtf.Network.EventArg;
using Mtf.Network.Services;
using System;
using System.Net.Sockets;
using System.Text;

namespace Mtf.Network
{
    public class Communicator : Disposable
    {
        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<MessageEventArgs> MessageSent;

        protected readonly Action<ILogger, Communicator, string, Exception> logErrorAction;

        public Communicator(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, ushort listenerPortOfServer)
        {
            ListenerPortOfServer = listenerPortOfServer;
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            logErrorAction = LoggerMessage.Define<Communicator, string>(LogLevel.Error, new EventId(1, "SerialDevice"), "Error occurred in communicator: {Device}, {EventDetails}");
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
            var messageToSend = TransformMessage(message, appendNewLine);
            var result = Send(Socket, messageToSend);
            if (result)
            {
                OnMessageSent(messageToSend);
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

        public bool Send(Socket socket, string message, bool appendNewLine = false)
        {
            return Send(socket, ConvertMessageToData(message, appendNewLine));
        }

        public bool Send(Socket socket, byte[] bytes, bool appendNewLine = false)
        {
            var success = false;
            if (socket?.Connected ?? false)
            {
                success = socket.Send(bytes, SocketFlags.None) == bytes?.Length;
                if (success && appendNewLine)
                {
                    var enterBytes = Encoding.GetBytes(Environment.NewLine);
                    success &= socket.Send(enterBytes) == enterBytes.Length;
                }
            }
            return success;
        }

        protected void OnDataArrived(Socket socket, byte[] data)
        {
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
            if (NetUtils.IsSocketConnected(Socket))
            {
                NetUtils.CloseSocket(Socket);
                Socket = null;
            }
        }

        protected string TransformMessage(string message, bool appendNewLine)
        {
            return appendNewLine ? String.Concat(message, Environment.NewLine) : message;
        }

        protected byte[] ConvertMessageToData(string message, bool appendNewLine)
        {
            var messageToSend = TransformMessage(message, appendNewLine);
            return Encoding.GetBytes(messageToSend);
        }

        public override string ToString()
        {
            if (Socket == null)
            {
                return "Not listening...";
            }

            return $"{Socket.LocalEndPoint}";
        }
    }
}
