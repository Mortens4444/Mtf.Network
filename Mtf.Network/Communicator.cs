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

        public Communicator(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, ushort listenerPortOfServer)
        {
            ListenerPortOfServer = listenerPortOfServer;
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
        }

        public AddressFamily AddressFamily { get; private set; }

        public SocketType SocketType { get; private set; }

        public ProtocolType ProtocolType { get; private set; }

        public Socket Socket { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public ushort ListenerPortOfServer { get; set; }

        public bool Send(string message)
        {
            return Send(Socket, Encoding.GetBytes(message));
        }

        public bool Send(byte[] bytes)
        {
            return Send(Socket, bytes);
        }

        public bool Send(Socket socket, string message)
        {
            return Send(socket, Encoding.GetBytes(message));
        }

        public static bool Send(Socket socket, byte[] bytes)
        {
            var success = false;
            if (socket?.Connected ?? false)
            {
                success = socket.Send(bytes, SocketFlags.None) == bytes?.Length;
            }
            return success;
        }

        protected void OnDataArrived(DataArrivedEventArgs e)
        {
            DataArrived?.Invoke(this, e);
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
            }
        }
    }
}
