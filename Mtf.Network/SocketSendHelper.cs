using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public static class SocketSendHelper
    {
        private static Task<bool> SendInternalAsync(Socket socket, byte[] data)
        {
            var tcs = new TaskCompletionSource<bool>();

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(data, 0, data.Length);

            void CompletedHandler(object s, SocketAsyncEventArgs e)
            {
                args.Completed -= CompletedHandler;
                args.Dispose();
                var ok = e.SocketError == SocketError.Success && e.BytesTransferred == data.Length;
                tcs.SetResult(ok);
            }

            args.Completed += CompletedHandler;

            if (!socket.SendAsync(args))
            {
                CompletedHandler(socket, args);
            }

            return tcs.Task;
        }

        public static bool Send(Socket socket, byte[] bytes, Socket rawSocket, bool appendNewLine, bool encryptData, Encoding encoding, MultiCipherEncryptionHandler encryptionHandler)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Data to send cannot be null.");
            }

            if (socket?.Connected != true)
            {
                return false;
            }

            if (encryptData && encryptionHandler != null)
            {
                bytes = encryptionHandler.Transform(bytes, true);
            }

            var success = socket.Send(bytes, SocketFlags.None) == bytes?.Length;
            if (success && appendNewLine)
            {
                var newlineBytes = encoding.GetBytes(Environment.NewLine);
                success &= socket.Send(newlineBytes, SocketFlags.None) == newlineBytes.Length;
            }

            return success;
        }

        public static Task<bool> SendAsync(Socket socket, byte[] bytes, bool appendNewLine, Encoding encoding, MultiCipherEncryptionHandler encryptionHandler)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Data to send cannot be null.");
            }

            var tcs = new TaskCompletionSource<bool>();

            if (socket?.Connected != true)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            if (encryptionHandler != null)
            {
                bytes = encryptionHandler.Transform(bytes, true);
            }

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);

            void CompletedHandler(object s, SocketAsyncEventArgs e)
            {
                args.Completed -= CompletedHandler;
                args.Dispose();

                if (e.SocketError == SocketError.Success && e.BytesTransferred == bytes.Length)
                {
                    if (appendNewLine)
                    {
                        var newLineBytes = encoding.GetBytes(Environment.NewLine);
                        SendInternalAsync(socket, newLineBytes).ContinueWith(t =>
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
            }

            args.Completed += CompletedHandler;

            if (!socket.SendAsync(args))
            {
                CompletedHandler(socket, args);
            }

            return tcs.Task;
        }
    }
}
