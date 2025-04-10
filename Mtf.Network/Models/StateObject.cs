using System;
using System.Net.Sockets;

namespace Mtf.Network.Models
{
    public class StateObject
    {
        public Socket Socket { get; set; }

        public byte[] Buffer { get; set; } = new byte[Constants.MaxBufferSize];

        public void ReadFromSocket(AsyncCallback asyncCallback)
        {
            _ = Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, asyncCallback, this);
        }
    }
}

