using System;
using System.Net.Sockets;

namespace Mtf.Network.EventArg
{
    public class DataArrivedEventArgs : EventArgs
    {
        public Socket Socket { get; }

        public byte[] Data { get; }

        public DataArrivedEventArgs(Socket socket, byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Socket = socket;
        }
    }
}
