using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using System;
using System.IO;
using System.Threading;

namespace Mtf.Network
{
    public class VncClient : IDisposable
    {
        private readonly Client client;
        private readonly string serverHost;
        
        private VideoCaptureClient videoCaptureClient;
        private int disposed;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        public VncClient(string serverHost, ushort listenerPort)
        {
            this.serverHost = serverHost;

            client = new Client(serverHost, listenerPort);
            client.DataArrived += Client_DataArrived;
            client.ErrorOccurred += Client_ErrorOccurred;
        }

        private void VideoCaptureClient_FrameArrived(object sender, FrameArrivedEventArgs e)
        {
            FrameArrived?.Invoke(this, e);
        }

        private void Client_DataArrived(object sender, DataArrivedEventArgs e)
        {
            var message = client.Encoding.GetString(e.Data);
            if (message.StartsWith(VncCommand.ScreenRecorderPortResponse))
            {
                var messageParts = message.Split(VncCommand.Separator);
                if (UInt16.TryParse(messageParts[1], out var port))
                {
                    videoCaptureClient = new VideoCaptureClient(serverHost, port);
                    videoCaptureClient.FrameArrived += VideoCaptureClient_FrameArrived;
                    videoCaptureClient.Start();
                }
            }
            else if (message == VncCommand.ScreenSize)
            {

            }
            else if(message == "Unknown command")
            {
                OnErrorOccurred(new InvalidDataException("Server could not recognize the sent command."));
            }
            else
            {
                OnErrorOccurred(new InvalidDataException($"Server sent an unexpected message: {message}"));
            }
        }

        private void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e.Exception));
        }

        public void Start()
        {
            client.Connect();
            client.Send(VncCommand.GetScreenRecorderPort);
        }

        public void Stop()
        {
            videoCaptureClient?.Stop();
            client.Disconnect();
        }

        public void Send(string message)
        {
            client.Send(message);
        }

        protected virtual void OnErrorOccurred(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }

        ~VncClient()
        {
            Dispose(false);
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
                Stop();
                videoCaptureClient?.Dispose();
                client.Dispose();
            }
        }
    }
}
