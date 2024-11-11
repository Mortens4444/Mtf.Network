using Mtf.Network.EventArg;

namespace Mtf.Network.Test
{
    public partial class MainForm : Form
    {
        private Server? server;
        private Client client;

        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                server = new Server(listenerPort: (ushort)nudServerListeningPort.Value, dataArrivedEventHandler: DataArrivedEventHandler);
                server.Start();
            }
        }

        private void DataArrivedEventHandler(object? sender, DataArrivedEventArgs e)
        {
            Invoke(() =>
            {
                rtbServerReceivedMessages.Text += $"{server?.Encoding.GetString(e.Data)}";
                server?.Send(e.Socket, rtbSendToClient.Text);
                rtbSendToClient.Text = String.Empty;
            });
        }

        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                server.Stop();
                server.Dispose();
                server = null;
            }
        }

        private void BtnSendToClient_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                server.Send(rtbSendToClient.Text);
                rtbSendToClient.Text = String.Empty;
            }
        }

        private void BtnSendToServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (client == null)
                {
                    client = new Client(tbServerAddress.Text, (ushort)nudServerPort.Value, dataArrivedHandler: ClientDataArrivedEventHandler);
                }
                client.Connect();
                client.Send(rtbClientSend.Text);
                rtbClientSend.Text = String.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void ClientDataArrivedEventHandler(object? sender, DataArrivedEventArgs e)
        {
            Invoke(() =>
            {
                rtbClientReceived.Text += $"{client?.Encoding.GetString(e.Data)}";
            });
        }
    }
}
