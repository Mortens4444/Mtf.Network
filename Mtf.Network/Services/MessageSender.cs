using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mtf.Network.Services
{
    public static class MessageSender
    {
        public static bool Send(Socket socket, byte[] bytes)
        {
            int sentBytes = 0;
            if (socket.Connected)
            {
                sentBytes = socket.Send(bytes, bytes.Length, SocketFlags.None);
            }
            return sentBytes == bytes.Length;
        }

        public static Task<bool> SendAsync(Socket socket, byte[] bytes)
        {
            if (!socket.Connected)
                return Task.FromResult(false);

            return Task.Factory.FromAsync(
                (callback, state) => socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, callback, state),
                socket.EndSend,
                null
            ).ContinueWith(t => t.Result == bytes.Length);
        }
        //public static async Task<bool> SendAsync(Socket socket, byte[] bytes)
        //{
        //    int sentBytes = 0;
        //    if (socket.Connected)
        //    {
        //        sentBytes = await socket.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        //    }
        //    return sentBytes == bytes.Length;
        //}
    }
}
