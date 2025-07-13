using Mtf.Cryptography.AsymmetricCiphers;
using Mtf.Cryptography.Interfaces;
using Mtf.Cryptography.KeyGenerators;
using Mtf.Cryptography.SymmetricCiphers;
using Mtf.MessageBoxes;
using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Models;
using Mtf.Network.Services;
using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mtf.Network.Test
{
    public partial class MainForm : Form
    {
        private Server server;
        private Client client;
        private Client client2;
        private FtpClient ftpClient;
        private TelnetClient telnetClient;
        private SmtpClient smtpClient;
        private Pop3Client pop3Client;
        private VncServer vncServer;
        private VncClient vncClient;

        public MainForm()
        {
            InitializeComponent();
            cbFtpCommands.SelectedIndex = 0;
            cbSmtpCommands.SelectedIndex = 0;
            cbPop3Commands.SelectedIndex = 0;
        }

        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (server == null)
                {
                    if (!File.Exists("key.xml"))
                    {
                        RsaKeyGenerator.GenerateKeyFiles("key.xml", "public_key.xml", 2048, true);
                    }
                    var ciphers = chkUseEncrypt.Checked ? new ICipher[] { new CaesarCipher(1), new RsaCipher("key.xml", true) } : Array.Empty<ICipher>();
                    server = new Server(listenerPort: (ushort)nudServerListeningPort.Value, ciphers: ciphers);
                    server.ErrorOccurred += Server_ErrorOccurred;
                    server.DataArrived += DataArrivedEventHandler;
                    server.Start();
                    lblServer.Text = server.ToString();
                    btnStopServer.Enabled = true;
                    btnStartServer.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void Server_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            ErrorBox.Show(e.Exception);
        }

        private async void DataArrivedEventHandler(object sender, DataArrivedEventArgs e)
        {
            string text = null;
            Invoke((Action)(() =>
            {
                rtbServerReceivedMessages.AppendText($"{server?.Encoding.GetString(e.Data)}");
                text = rtbSendToClient.Text;
            }));

            await SendtoClient(e.Socket, text);

            Invoke((Action)(() =>
            {
                rtbSendToClient.Text = String.Empty;
            }));
        }

        private async Task SendtoClient(Socket socket, string text)
        {
            if (server != null)
            {
                await Task.Delay(50);
                server.SendMessageToClient(socket, text);
            }
        }

        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                server.Stop();
                server.Dispose();
                server = null;
                btnStopServer.Enabled = true;
                btnStartServer.Enabled = true;
            }
        }

        private async void BtnSendToClient_Click(object sender, EventArgs e)
        {
            await SendToAllClient();
        }

        private async Task SendToAllClient()
        {
            if (server != null)
            {
                await Task.Delay(50);
                server.SendMessageToAllClients(rtbSendToClient.Text);
                rtbSendToClient.Text = String.Empty;
            }
        }

        private void BtnSendToServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (client == null)
                {
                    var ciphers = chkClientUseEnrypt.Checked ? new ICipher[] { new CaesarCipher(1), new RsaCipher("key.xml", true) } : Array.Empty<ICipher>();
                    client = new Client(tbServerAddress.Text, (ushort)nudServerPort.Value, ciphers: ciphers);
                    client.DataArrived += ClientDataArrivedEventHandler;
                }
                client.Connect();
                client.Send(rtbClientSend.Text);
                rtbClientSend.Text = String.Empty;
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }


        private void BtnSendToServer2_Click(object sender, EventArgs e)
        {
            try
            {
                if (client2 == null)
                {
                    var ciphers = chkClientUseEnrypt2.Checked ? new ICipher[] { new CaesarCipher(1), new RsaCipher("key.xml", true) } : Array.Empty<ICipher>();
                    client2 = new Client(tbServerAddress.Text, (ushort)nudServerPort2.Value, ciphers: ciphers);
                    client2.DataArrived += ClientDataArrivedEventHandler2;
                }
                client2.Connect();
                client2.Send(rtbClientSend2.Text);
                rtbClientSend2.Text = String.Empty;
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void ClientDataArrivedEventHandler(object sender, DataArrivedEventArgs e)
        {
            Invoke((Action)(() =>
            {
                rtbClientReceived.AppendText($"{client?.Encoding.GetString(e.Data)}");
            }));
        }

        private void ClientDataArrivedEventHandler2(object sender, DataArrivedEventArgs e)
        {
            Invoke((Action)(() =>
            {
                rtbClientReceived2.AppendText($"{client2?.Encoding.GetString(e.Data)}");
            }));
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

        private void FtpClient_DataArrived(object sender, DataArrivedEventArgs e)
        {
            Invoke((Action)(() => { rtbFtpCommunication.AppendText(String.Concat("Data received: ", ftpClient.Encoding.GetString(e.Data), Environment.NewLine)); }));
        }

        private void FtpClient_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            ErrorBox.Show(e.Exception);
        }

        private void FtpClient_MessageSent(object sender, MessageEventArgs e)
        {
            Invoke((Action)(() => { rtbFtpCommunication.AppendText(String.Concat("Message sent: ", e.Message, Environment.NewLine)); }));
        }

        private void FtpClient_MessageReceived(object sender, MessageEventArgs e)
        {
            Invoke((Action)(() => { rtbFtpCommunication.AppendText(String.Concat("Message received: ", e.Message, Environment.NewLine)); }));
        }

        private async void BtnSendFtpCommand_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void BtnTelnetConnect_Click(object sender, EventArgs e)
        {
            telnetClient = new TelnetClient(tbTelnetHost.Text, (ushort)nudTelnetPort.Value);
            Connect(telnetClient, rtbTelnetCommunication);
        }

        private void BtnSendToTelnetServer_Click(object sender, EventArgs e)
        {
            try
            {
                telnetClient?.Send(tbTelnetCommand.Text, true);
                tbTelnetCommand.Text = String.Empty;
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void BtnSmtpConnect_Click(object sender, EventArgs e)
        {
            smtpClient = new SmtpClient(tbSmtpHost.Text, (ushort)nudSmtpPort.Value);
            Connect(smtpClient, rtbSmtpCommunication);
        }

        private void TbSendToSmtpServer_Click(object sender, EventArgs e)
        {
            try
            {
                string[] credentials;
                var commandParameter = rtbSmtpParams.Text;
                switch (cbSmtpCommands.Text)
                {
                    case "HELO":
                        smtpClient.Helo(tbSmtpHost.Text);
                        break;
                    case "EHLO":
                        smtpClient.Ehlo(tbSmtpHost.Text);
                        break;
                    case "STARTTLS":
                        smtpClient.StartTls();
                        break;
                    case "DATA":
                        smtpClient.Data();
                        break;
                    case "RSET":
                        smtpClient.Reset();
                        break;
                    case "TURN":
                        smtpClient.Turn();
                        break;
                    case "EXPN":
                        smtpClient.Expanse(rtbSmtpParams.Text);
                        break;
                    case "NOOP":
                        smtpClient.NoOperation();
                        break;
                    case "AUTH PLAIN":
                        credentials = rtbSmtpParams.Text.Split(';');
                        smtpClient.Authenticate(SmtpAuthenticationMechanism.Plain, credentials[0], credentials[1]);
                        break;
                    case "AUTH LOGIN":
                        credentials = rtbSmtpParams.Text.Split(';');
                        smtpClient.Authenticate(SmtpAuthenticationMechanism.Login, credentials[0], credentials[1]);
                        break;
                    case "AUTH CRAM-MD5":
                        credentials = rtbSmtpParams.Text.Split(';');
                        smtpClient.Authenticate(SmtpAuthenticationMechanism.CramMd5, credentials[0], credentials[1]);
                        break;
                    case "AUTH DIGEST-MD5":
                        credentials = rtbSmtpParams.Text.Split(';');
                        smtpClient.Authenticate(SmtpAuthenticationMechanism.DigestMd5, credentials[0], credentials[1]);
                        break;
                    case "AUTH XOAUTH2":
                        credentials = rtbSmtpParams.Text.Split(';');
                        smtpClient.Authenticate(SmtpAuthenticationMechanism.XoAuth2, credentials[0], credentials[1]);
                        break;
                    case "MAIL FROM":
                        smtpClient.MailFrom(rtbSmtpParams.Text);
                        break;
                    case "SEND FROM":
                        smtpClient.SendFrom(rtbSmtpParams.Text);
                        break;
                    case "SOML FROM":
                        smtpClient.SendOrMailFrom(rtbSmtpParams.Text);
                        break;
                    case "SAML FROM":
                        smtpClient.SamlFrom(rtbSmtpParams.Text);
                        break;
                    case "SIZE":
                        smtpClient.CheckMessageSize(Convert.ToUInt64(rtbSmtpParams.Text));
                        break;
                    case "VRFY":
                        smtpClient.Verify(rtbSmtpParams.Text);
                        break;
                    case "QUIT":
                        smtpClient.Quit();
                        break;
                    default:
                        ErrorBox.Show("Unknown SMTP command", $"Cannot recognize command: {cbSmtpCommands.Text}");
                        break;
                }
                rtbSmtpParams.Text = String.Empty;
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void BtnPop3Connect_Click(object sender, EventArgs e)
        {
            pop3Client = new Pop3Client(tbPop3Host.Text, (ushort)nudPop3Port.Value);
            Connect(pop3Client, rtbPop3Communication);
        }

        private void BtnSendToPop3Server_Click(object sender, EventArgs e)
        {
            try
            {
                string[] credentials;
                var commandParameter = rtbPop3Param.Text;
                switch (cbPop3Commands.Text)
                {
                    case "USER":
                        pop3Client.User(rtbPop3Param.Text);
                        break;
                    case "PASS":
                        pop3Client.Pass(rtbPop3Param.Text);
                        break;
                    case "LIST":
                        pop3Client.List(rtbPop3Param.Text);
                        break;
                    case "RETR":
                        pop3Client.Retrieve(rtbPop3Param.Text);
                        break;
                    case "RSET":
                        pop3Client.Reset();
                        break;
                    case "DELE":
                        pop3Client.Delete(rtbPop3Param.Text);
                        break;
                    case "NOOP":
                        pop3Client.NoOperation();
                        break;
                    case "TOP":
                        var parameters = rtbPop3Param.Text.Split(';');
                        pop3Client.Top(parameters[0], Convert.ToUInt16(parameters[1]));
                        break;
                    case "APOP":
                        credentials = rtbPop3Param.Text.Split(';');
                        pop3Client.Apop(credentials[0], credentials[1]);
                        break;
                    case "STAT":
                        pop3Client.GetStatus();
                        break;
                    case "UIDL":
                        pop3Client.Uidl(rtbPop3Param.Text);
                        break;
                    case "QUIT":
                        pop3Client.Quit();
                        break;
                    default:
                        ErrorBox.Show("Unknown POP3 command", $"Cannot recognize command: {cbPop3Commands.Text}");
                        break;
                }
                rtbPop3Param.Text = String.Empty;
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void BtnHttpSend_Click(object sender, EventArgs e)
        {
            try
            {
                var httpClient = new HttpClient(new Uri(tbHttpHost.Text));
                Connect(httpClient, rtbHttpCommunication);
                httpClient.Send(new HttpPacket(new Uri(tbHttpHost.Text)));
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void Connect(Client client, RichTextBox communication)
        {
            try
            {
                client.DataArrived += (object sender, DataArrivedEventArgs e) =>
                {
                    Invoke((Action)(() =>
                    {
                        communication.AppendText(String.Concat("Data received: ", client.Encoding.GetString(e.Data), Environment.NewLine));
                    }));
                };
                client.MessageSent += (object sender, MessageEventArgs e) =>
                {
                    Invoke((Action)(() =>
                    {
                        communication.AppendText(String.Concat("Message sent: ", e.Message, Environment.NewLine));
                    }));
                };
                client.ErrorOccurred += (object sender, ExceptionEventArgs e) => { ErrorBox.Show(e.Exception); };
                client.Connect();
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private async void BtnDiscover_Click(object sender, EventArgs e)
        {
            lvUPnPDevices.Items.Clear();
            var upnpClient = new UpnpClient();
            upnpClient.DeviceDiscovered += DeviceDiscoveredHandler;
            upnpClient.Connect();
            await upnpClient.SendDiscoveryMessage();
        }

        private void DeviceDiscoveredHandler(object sender, DeviceDiscoveredEventArgs args)
        {
            if (args.Device.Manufacturer?.Equals("Pelco", StringComparison.OrdinalIgnoreCase) == true)
            {
                BeginInvoke((Action)(() => { lvUPnPDevices.Items.Add(new ListViewItem(args.Device.IPAddress)); }));
            }
        }

        private void BtnSendRequest_Click(object sender, EventArgs e)
        {
            try
            {
                rtbResult.Text = SoapClient.SendRequest(new Uri(tbSoapUri.Text), tbFunctionUrn.Text, tbServiceId.Text, tbResultTagName.Text);
            }
            catch (Exception ex)
            {
                rtbResult.Text = ex.ToString();
            }
        }

        const string sunellCameraIp = "192.168.0.201";
        const string sunellCameraUsername = "admin";
        const string sunellCameraPassword = "Tibi2025";
        const int cameraId = 1;
        const int streamId = 1;
        private Client pageClient;

        private void BtnSunellCameraTest_Click(object sender, EventArgs e)
        {
            try
            {
                pageClient = new Client(sunellCameraIp, 30001);
                pageClient.DataArrived += DataArrived;
                pageClient.Connect();

                //pageClient.Send(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x80, 0x02, 0x20, 0x00, 0x82, 0x22, 0x00, 0x00, 0x02, 0x04, 0x05, 0xb4, 0x01, 0x03, 0x03, 0x02, 0x01, 0x01, 0x04, 0x02 });

                var auth_1 = new byte[] { 0xff, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x12, 0x5c, 0x5c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xde, 0x00, 0x00, 0x00, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var auth_2 = new byte[48 - sunellCameraIp.Length];
                var auth_3 = new byte[] { 0x75, 0x31 };
                var auth_4 = new byte[32 - sunellCameraUsername.Length];
                var auth_5 = new byte[32 - sunellCameraPassword.Length];

                var msg = AppendByteArrays(auth_1, Encoding.ASCII.GetBytes(sunellCameraIp), auth_2, auth_3, Encoding.ASCII.GetBytes(sunellCameraUsername), auth_4, Encoding.ASCII.GetBytes(sunellCameraPassword), auth_5);
                pageClient.Send(msg);

                auth_1 = new byte[] { 0xff, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x01, 0x99, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xbc };
                auth_2 = Encoding.ASCII.GetBytes(String.Format("<StreamURIRequestInfo UserName=\"{0}\" Password=\"{1}\" CameraID=\"{2}\" StreamID=\"{3}\" RtspTransferProtocol=\"2\">", sunellCameraUsername, sunellCameraPassword, cameraId, streamId));
                auth_3 = new byte[] { 0x0a, 0x09 };
                auth_4 = Encoding.ASCII.GetBytes(String.Format("<Addr IP=\"{0}\" Port=\"554\" IPProtoVar=\"1\" />", sunellCameraIp));
                auth_5 = new byte[] { 0x0a };
                var auth_6 = Encoding.ASCII.GetBytes("</StreamURIRequestInfo>");
                var auth_7 = new byte[] { 0x0a, 0x00 };

                msg = AppendByteArrays(auth_1, auth_2, auth_3, auth_4, auth_5, auth_6, auth_7);
                pageClient.Send(msg);
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        public static byte[] AppendByteArrays(params byte[][] arrays)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (byte[] array in arrays)
                {
                    ms.Write(array, 0, array.Length);
                }
                return ms.ToArray();
            }
        }

        private void DataArrived(object sender, DataArrivedEventArgs e)
        {
            var message = Encoding.ASCII.GetString(e.Data);
            InfoBox.Show("Data arrived to client", message);
        }

        private void BtnStartCaptureImage_Click(object sender, EventArgs e)
        {
            try
            {
                var vcc = new VideoCaptureClient(tbImageProviderIpAddress.Text, (ushort)nudImageProviderPort.Value);
                vcc.BufferSize = 409600;
                vcc.FrameArrived += Vcc_FrameArrived;
                vcc.Start(20000);
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        Image lastImage;

        private void Vcc_FrameArrived(object sender, FrameArrivedEventArgs e)
        {
            Invoke((Action)(() =>
            {
                lastImage?.Dispose();
                pbVideoCaptureImage.Image = e.Frame;
                lastImage = e.Frame;
            }));
        }

        private void BtnStartVncServer_Click(object sender, EventArgs e)
        {
            vncServer = new VncServer(new ScreenInfoProvider());
            vncServer.Start();

            lblVncServerStatus.Text = vncServer.ToString();
            tbVncClientIpAddress.Text = vncServer.CommandServerIpAddress;
            nudVncClientIpAddress.Value = vncServer.CommandServer.ListenerPortOfServer;
        }

        private void BtnStartVncClient_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(tbVncClientIpAddress.Text))
            {
                vncClient = new VncClient(tbVncClientIpAddress.Text, (ushort)nudVncClientIpAddress.Value);
                vncClient.FrameArrived += VncClient_FrameArrived;
                vncClient.Start();
            }
        }

        private void VncClient_FrameArrived(object sender, FrameArrivedEventArgs e)
        {
            Invoke((Action)(() =>
            {
                pbVncImages.Image = (Image)e.Frame.Clone();
                e.Frame.Dispose();
            }));
        }

        private void BtnStartAppWithPsExec_Click(object sender, EventArgs e)
        {
            try
            {
                var output = new StringBuilder();

                //var process = ProcessUtils.StartApplicationWithPsExec("calc");
                var process = ProcessUtils.StartApplicationWithPsExec("systeminfo", "192.168.0.246", "Sup_dev_env", "pass", encoding: Encoding.GetEncoding(852),
                    outputDataReceived: (_, d) => { output.AppendLine(d.Data); }, errorDataReceived: (_, d) => { output.AppendLine(d.Data); });

                process.WaitForExit();
                rtbOutput.Text = output.ToString();
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }
    }
}
