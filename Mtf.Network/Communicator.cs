using Microsoft.Extensions.Logging;
using Mtf.Cryptography.Interfaces;
using Mtf.Extensions;
using Mtf.Network.EventArg;
using Mtf.Network.Interfaces;
using Mtf.Network.Services;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public partial class Communicator : Disposable, ICommunicator
    {
        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<MessageEventArgs> MessageSent;

        protected Action<ILogger, Communicator, string, Exception> LogErrorAction { get; }
        public MultiCipherEncryptionHandler MultiCipherEncryptionHandler { get; }

        public ILogger Logger { get; set; }
        public AddressFamily AddressFamily { get; private set; }
        public SocketType SocketType { get; private set; }
        public ProtocolType ProtocolType { get; private set; }
        public Socket Socket { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public ushort ListenerPortOfServer { get; set; }

        protected int BufferSize { get; set; } = Constants.MaxBufferSize;

        public Communicator(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, ushort listenerPortOfServer, params ICipher[] ciphers)
        {
            ListenerPortOfServer = listenerPortOfServer;
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            MultiCipherEncryptionHandler = new MultiCipherEncryptionHandler(ciphers);
            LogErrorAction = LoggerMessage.Define<Communicator, string>(LogLevel.Error, new EventId(1, "SerialDevice"), "Error occurred in communicator: {Device}, {EventDetails}");
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

        public Task<bool> SendAsync(byte[] bytes, bool appendNewLine = false)
        {
            return SocketSendHelper.SendAsync(Socket, bytes, appendNewLine, Encoding, MultiCipherEncryptionHandler);
        }

        public bool Send(Socket socket, string message, bool appendNewLine = false)
        {
            return Send(socket, ConvertMessageToData(message, appendNewLine));
        }

        public Task<bool> SendAsync(Socket socket, string message, bool appendNewLine = false)
        {
            return SocketSendHelper.SendAsync(socket, ConvertMessageToData(message, appendNewLine), appendNewLine, Encoding, MultiCipherEncryptionHandler);
        }

        public bool Send(Socket socket, byte[] bytes, bool appendNewLine = false, bool encryptData = true)
        {
            return SocketSendHelper.Send(socket, bytes, socket, appendNewLine, encryptData, Encoding, MultiCipherEncryptionHandler);
        }

        protected void OnDataArrived(Socket socket, byte[] data)
        {
            data = MultiCipherEncryptionHandler.Transform(data, false);
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
            //    foreach (var cipher in ciphers)
            //    {
            //        if (cipher is IAsymmetricCipher asymmetricCipher)
            //        {
            //            SendPublicKey(asymmetricCipher);
            //        }
            //    }
        }

        //private void SendPublicKey(IAsymmetricCipher cipher)
        //{
        //    if (cipher == null)
        //    {
        //        throw new ArgumentNullException(nameof(cipher));
        //    }
        //    Send(Socket, Encoding.GetBytes(RsaCipher.RsaKeyHeader), false, false);
        //    Send(Socket, cipher.PublicKey, true, false);
        //}

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
    }
}
