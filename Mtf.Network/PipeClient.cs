using Mtf.Network.EventArgs;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    public class PipeClient : IDisposable
    {
        private int disposed;
        private CancellationTokenSource cancellationTokenSource;
        private NamedPipeClientStream namedPipeClientStream;

        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        public event EventHandler<MessageEventArgs> MessageReceived;

        public string ServerName { get; set; } = ".";

        public string PipeName { get; set; } = String.Empty;

        public PipeOptions PipeOptions { get; set; } = PipeOptions.Asynchronous;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public PipeDirection PipeDirection { get; set; } = PipeDirection.InOut;

        ~PipeClient()
        {
            Dispose(false);
        }

        public async Task ConnectAsync()
        {
            if (namedPipeClientStream != null && namedPipeClientStream.IsConnected)
            {
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            namedPipeClientStream = new NamedPipeClientStream(ServerName, PipeName, PipeDirection, PipeOptions);

            try
            {
                await namedPipeClientStream.ConnectAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (namedPipeClientStream != null)
            {
                try
                {
                    namedPipeClientStream.Close();
                    namedPipeClientStream.Dispose();
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
                }
                finally
                {
                    namedPipeClientStream = null;
                }
            }
        }

        /// <summary>
        /// Call ConnectAsync prior this function call. This function sends a message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">CancellationToken to stop he process.</param>
        /// <returns>The task representing the process.</returns>
        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = Encoding.GetBytes(message);
                await namedPipeClientStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                await namedPipeClientStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
            }
        }

        /// <summary>
        /// Call ConnectAsync prior this function call. This function receives a message from the server.
        /// </summary>
        /// <param name="bufferSize">Buffer to be used for message receive.</param>
        /// <param name="cancellationToken">CancellationToken to stop he process.</param>
        /// <returns>The received message.</returns>
        public async Task<string> ReceiveAsync(int bufferSize = 1024, CancellationToken cancellationToken = default)
        {
            try
            {
                var buffer = new byte[bufferSize];
                var bytesRead = await namedPipeClientStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                return Encoding.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
                return String.Empty;
            }
        }

        /// <summary>
        /// Call ConnectAsync prior this function call. It will listen for incoming messages.
        /// </summary>
        /// <returns>The task representing this action.</returns>
        public void StartListening(int bufferSize = 1024)
        {
            StopListening();
            _ = Task.Run(() => ListenAsync(bufferSize), cancellationTokenSource.Token);
        }

        public void StopListening()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async Task ListenAsync(int bufferSize)
        {
            try
            {
                var buffer = new byte[bufferSize];
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var bytesRead = await namedPipeClientStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token).ConfigureAwait(false);
                    if (bytesRead > 0)
                    {
                        var message = Encoding.GetString(buffer, 0, bytesRead);
                        MessageReceived?.Invoke(this, new MessageEventArgs(message));
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            if (disposing)
            {
                StopListening();
                Disconnect();
            }
        }
    }
}
