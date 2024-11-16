using Mtf.Network.EventArg;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Mtf.Network
{
    public class PipeServer : Disposable
    {
        private StreamWriter writer;
        private CancellationTokenSource cancellationTokenSource;

        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        public string PipeName { get; set; } = String.Empty;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public PipeDirection PipeDirection { get; set; } = PipeDirection.InOut;

        public PipeTransmissionMode PipeTransmissionMode { get; set; } = PipeTransmissionMode.Byte;

        public PipeOptions PipeOptions { get; set; } = PipeOptions.Asynchronous;

        public void Start()
        {
            if (String.IsNullOrWhiteSpace(PipeName))
            {
                throw new InvalidOperationException("PipeName must be set before starting the server.");
            }

            Stop();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            _ = Task.Run(() => RunServerAsync(token), token);
        }

        public void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        public async void SendAsnyc(string message)
        {
            if (writer == null)
            {
                throw new InvalidOperationException("Pipe server is not started.");
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await writer.WriteLineAsync(message).ConfigureAwait(false);
        }

        private async Task RunServerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection, 1, PipeTransmissionMode, PipeOptions))
                {
                    try
                    {
                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                        using (var reader = new StreamReader(server, Encoding))
                        {
                            writer = new StreamWriter(server, Encoding) { AutoFlush = true };
                            string message;
                            while ((message = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                            {
                                MessageReceived?.Invoke(this, new MessageEventArgs(message));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, new ExceptionEventArgs(ex));
                    }
                    finally
                    {
                        StopWriter();
                    }
                }
            }
        }

        private void StopWriter()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
                writer.Dispose();
                writer = null;
            }
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            StopWriter();
        }
    }
}
