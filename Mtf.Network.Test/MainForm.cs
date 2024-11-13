using MessageBoxes;
using Mtf.Network.Enums;
using Mtf.Network.EventArg;

namespace Mtf.Network.Test
{
    public partial class MainForm : Form
    {
        private Server? server;
        private Client client;
        private FtpClient ftpClient;

        public MainForm()
        {
            InitializeComponent();
            cbFtpCommands.SelectedIndex = 0;
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

        private void BtnFtpAuthenticate_Click(object sender, EventArgs e)
        {
            try
            {
                ftpClient = new FtpClient(tbFtpHost.Text.Replace("ftp://", String.Empty), (ushort)nudFtpPort.Value);
                ftpClient.DataArrived += FtpClient_DataArrived;
                ftpClient.ErrorOccurred += FtpClient_ErrorOccurred;
                ftpClient.MessageSent += FtpClient_MessageSent;
                ftpClient.MessageReceived += FtpClient_MessageReceived;
                ftpClient.Connect();
                ftpClient.User(tbFtpUser.Text);
                ftpClient.Password(pbFtpPassword.Password);
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void FtpClient_DataArrived(object? sender, DataArrivedEventArgs e)
        {
            Invoke(() => { rtbFtpCommunication.Text += String.Concat("Data received: ", ftpClient.Encoding.GetString(e.Data), Environment.NewLine); });
        }

        private void FtpClient_ErrorOccurred(object? sender, ExceptionEventArgs e)
        {
            ErrorBox.Show(e.Exception);
        }

        private void FtpClient_MessageSent(object? sender, MessageEventArgs e)
        {
            Invoke(() => { rtbFtpCommunication.Text += String.Concat("Message sent: ", e.Message, Environment.NewLine); });
        }

        private void FtpClient_MessageReceived(object? sender, MessageEventArgs e)
        {
            Invoke(() => { rtbFtpCommunication.Text += String.Concat("Message received: ", e.Message, Environment.NewLine); });
        }

        private async void BtnSendFtpCommand_Click(object sender, EventArgs e)
        {
            var commandParameter = tbFtpCommandParameter.Text;
            switch (cbFtpCommands.Text)
            {
                case "ChangePath":
                    await ftpClient.ChangeWorkingDirectory(commandParameter);
                    break;
                case "DeleteFile":
                    await ftpClient.DeleteFile(commandParameter);
                    break;
                case "Help":
                    await ftpClient.Help(commandParameter);
                    break;
                case "List":
                    await ftpClient.List(commandParameter);
                    break;
                case "GetModificationDate":
                    await ftpClient.GetModificationDate(commandParameter);
                    break;
                case "MakeDirectory":
                    await ftpClient.MakeDirectory(commandParameter);
                    break;
                case "SetTransferMode":
                    if (Enum.TryParse(commandParameter, out FtpTransferMode transferMode))
                        await ftpClient.SetTransferMode(transferMode);
                    break;
                case "Port":
                    await ftpClient.Port(commandParameter);
                    break;
                case "ContinueDownload":
                    if (ulong.TryParse(commandParameter, out ulong byteOffset))
                        await ftpClient.ContinueDownload(byteOffset);
                    break;
                case "Download":
                    await ftpClient.Download(commandParameter);
                    break;
                case "RemoveDirectory":
                    await ftpClient.RemoveDirectory(commandParameter);
                    break;
                case "RenameFile":
                    await ftpClient.RenameFile(commandParameter);
                    break;
                case "RenameTo":
                    await ftpClient.RenameTo(commandParameter);
                    break;
                case "ShellExecute":
                    await ftpClient.ShellExecute(commandParameter);
                    break;
                case "GetSize":
                    await ftpClient.GetSize(commandParameter);
                    break;
                case "Status":
                    await ftpClient.Status();
                    break;
                case "Store":
                    await ftpClient.Store(commandParameter);
                    break;
                case "CreateNewFile":
                    await ftpClient.CreateNewFile(commandParameter);
                    break;
                case "SetFileStructure":
                    if (Enum.TryParse(commandParameter, out FtpFileStructure fileStructure))
                        await ftpClient.SetFileStructure(fileStructure);
                    break;
                case "SetTransferType":
                    if (Enum.TryParse(commandParameter, out FtpTransferType transferType))
                        await ftpClient.SetTransferType(transferType);
                    break;
                case "Abort":
                    await ftpClient.Abort();
                    break;
                case "ChangeToParentDirectory":
                    await ftpClient.ChangeToParentDirectory();
                    break;
                case "PassiveMode":
                    await ftpClient.PassiveMode();
                    break;
                case "Reinitialize":
                    await ftpClient.Reinitialize();
                    break;
                case "Quit":
                    await ftpClient.Quit();
                    break;
                case "PrintWorkingDirectory":
                    await ftpClient.PrintWorkingDirectory();
                    break;
                case "SystemInfo":
                    await ftpClient.SystemInfo();
                    break;
                case "NoOperation":
                    await ftpClient.NoOperation();
                    break;
                case "ExtendedPrintWorkingDirectory":
                    await ftpClient.ExtendedPrintWorkingDirectory();
                    break;
                case "ExtendedChangeWorkingDirectory":
                    await ftpClient.ExtendedChangeWorkingDirectory(commandParameter);
                    break;
                case "GetFolderInfo":
                    await ftpClient.GetFolderInfo();
                    break;
                case "GetFileInfo":
                    await ftpClient.GetFileInfo(commandParameter);
                    break;
                case "User":
                    await ftpClient.User(commandParameter);
                    break;
                case "Password":
                    await ftpClient.Password(commandParameter);
                    break;
                default:
                    ErrorBox.Show("Unknown FTP command", $"Cannot recognize command: {cbFtpCommands.Text}");
                    break;
            }
        }
    }
}
