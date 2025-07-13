using System;
using System.Net.Sockets;

namespace Mtf.Network
{
    public static class SocketConfigurator
    {
        public static void SetTimeout(Socket socket, int value = 0)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
        }

        public static void SetBufferSize(Socket socket, int bufferSize = Constants.MaxBufferSize)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, bufferSize);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, bufferSize);
        }
    }
}
