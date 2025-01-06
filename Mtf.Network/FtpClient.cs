using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network
{
    /// <summary>
    /// RFC 959 - https://tools.ietf.org/html/rfc959
    /// </summary>
    public class FtpClient : Disposable
    {
        private readonly string ftpServer;
        private readonly ushort port;
        private string remoteFilePath = String.Empty;
        private Socket socket, dataSocket;
        private IPEndPoint localFtpDataEndPoint;

        private CancellationTokenSource dataReceiveCancellationTokenSource;

        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred; // change base to client and remove this
        public event EventHandler<MessageEventArgs> MessageSent; // too

        /// <summary>
        /// Initializes a new instance of the FtpClient class with the specified server, username, and password.
        /// </summary>
        /// <param name="ftpServer">The FTP server address, e.g., "ftp://example.com".</param>
        public FtpClient(string ftpServer, ushort port = 21)
        {
            this.ftpServer = ftpServer;
            this.port = port;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Connects to the FTP server.
        /// </summary>
        public void Connect()
        {
            if (NetUtils.IsSocketConnected(socket))
            {
                throw new InvalidOperationException("Already connected to the FTP server.");
            }

            CancellationTokenSource = new CancellationTokenSource();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddresses = Dns.GetHostAddresses(ftpServer);
            if (ipAddresses.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }

            var serverEndpoint = new IPEndPoint(ipAddresses[0], port);
            socket.Connect(serverEndpoint);
            _ = ReceiveAsync(CancellationTokenSource.Token);
        }

        /// <summary>
        /// Disconnects from the FTP server.
        /// </summary>
        public void Disconnect()
        {
            if (NetUtils.IsSocketConnected(socket))
            {
                NetUtils.CloseSocket(socket);
                socket = null;
            }

            if (dataReceiveCancellationTokenSource != null)
            {
                dataReceiveCancellationTokenSource.Cancel();
                dataReceiveCancellationTokenSource.Dispose();
                dataReceiveCancellationTokenSource = null;
            }

            if (NetUtils.IsSocketConnected(dataSocket))
            {
                NetUtils.CloseSocket(dataSocket);
                socket = null;
            }
        }

        /// <summary>
        /// Sends an FTP account command with the specified account name.
        /// </summary>
        /// <param name="account">Account name to use for FTP.</param>
        /// <returns>The task representing the process.</returns>
        public Task Account(string account) => Send($"ACCT {account}\r\n");

        /// <summary>
        /// Sends an FTP allocate command to reserve the specified storage size.
        /// </summary>
        /// <param name="size">Amount of storage to allocate, in bytes.</param>
        /// <returns>The task representing the process.</returns>
        public Task AllocateStorage(ulong size)
        {
            return Send($"ALLO {size}\r\n");
        }

        /// <summary>
        /// Appends data to a specified file on the server.
        /// </summary>
        /// <param name="file">File path on the server to append data to.</param>
        /// <returns>The task representing the process.</returns>
        public Task AppendTo(string file)
        {
            return Send($"APPE {file}\r\n");
        }

        /// <summary>
        /// Changes the current working directory on the server.
        /// </summary>
        /// <param name="path">The path of the target directory on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task ChangeWorkingDirectory(string path)
        {
            var result = Send($"CWD {path}\r\n");
            ChangePath(path);
            return result;
        }

        private void ChangePath(string path)
        {
            remoteFilePath = path == ".." ?
                remoteFilePath.Substring(0, remoteFilePath.LastIndexOf('/')) :
                remoteFilePath += remoteFilePath.EndsWith("/", StringComparison.Ordinal) ? path : $"/{path}";
        }

        /// <summary>
        /// Deletes the specified file from the server.
        /// </summary>
        /// <param name="file">File path on the server to delete.</param>
        /// <returns>The task representing the process.</returns>
        public Task DeleteFile(string file)
        {
            return Send($"DELE {file}\r\n");
        }

        /// <summary>
        /// Sends a help request to the server, optionally with a specific command to get help for.
        /// </summary>
        /// <param name="param">Command to get help for. If null, returns general help.</param>
        /// <returns>The task representing the process.</returns>
        public Task Help(string param)
        {
            return Send(String.IsNullOrEmpty(param) ? "HELP\r\n" : $"HELP {param}\r\n");
        }

        /// <summary>
        /// Lists files and directories at a specified path on the server.
        /// </summary>
        /// <param name="path">Directory path to list. Optional.</param>
        /// <returns>The task representing the process.</returns>
        public Task List(string path)
        {
            return Send($"LIST {path}\r\n");
        }

        /// <summary>
        /// Gets the last modification date of a specified file.
        /// </summary>
        /// <param name="file">File path on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task GetModificationDate(string file)
        {
            return Send($"MDTM  {file}\r\n");
        }

        /// <summary>
        /// Creates a new directory on the server.
        /// </summary>
        /// <param name="directory">Path of the new directory.</param>
        /// <returns>The task representing the process.</returns>
        public Task MakeDirectory(string directory)
        {
            return Send($"MKD {directory}\r\n");
        }

        /// <summary>
        /// Sets the transfer mode (stream, block, or compressed) for data transfers.
        /// </summary>
        /// <param name="transferMode">Transfer mode to set.</param>
        /// <returns>The task representing the process.</returns>
        public Task SetTransferMode(FtpTransferMode transferMode)
        {
            return Send($"MODE {transferMode.ToString()[0]}\r\n");
        }

        /// <summary>
        /// Lists file names in the specified directory with an optional filter.
        /// </summary>
        /// <param name="filter">File filter, such as "*.txt". If null, lists all files.</param>
        /// <returns>The task representing the process.</returns>
        public Task ListNames(string filter)
        {
            return Send(String.IsNullOrEmpty(filter) ? "NLST\r\n" : $"NLST {filter}\r\n");
        }

        /// <summary>
        /// Provides the password for FTP login.
        /// </summary>
        /// <param name="password">User password.</param>
        /// <returns>The task representing the process.</returns>
        public Task Password(string password)
        {
            return Send($"PASS {password}\r\n");
        }

        /// <summary>
        /// Specifies the address and port for data connections.
        /// </summary>
        /// <param name="h1h2h3h4p1p2">Host and port in h1,h2,h3,h4,p1,p2 format.</param>
        /// <returns>The task representing the process.</returns>
        public async Task Port(string h1h2h3h4p1p2 = null)
        {
            if (!String.IsNullOrEmpty(h1h2h3h4p1p2))
            {
                await Send($"PORT {h1h2h3h4p1p2}\r\n").ConfigureAwait(false);
            }

            var externalIpAddress = await NetUtils.GetExternalIpAddressAsync().ConfigureAwait(false)
                ?? throw new InvalidOperationException("Unable to get external IP address.");

            localFtpDataEndPoint = new IPEndPoint(externalIpAddress, NetUtils.GetFreePort());
            await Send($"PORT {NetUtils.FormatIPEndPoint(localFtpDataEndPoint)}\r\n").ConfigureAwait(false);
            InitializeDataSocket();

            dataReceiveCancellationTokenSource = new CancellationTokenSource();
            _ = ReceiveDataAsync(dataSocket, dataReceiveCancellationTokenSource.Token);
        }

        /// <summary>
        /// Resumes a download from a specified byte offset.
        /// </summary>
        /// <param name="byteOffset">Byte offset to resume from.</param>
        /// <returns>The task representing the process.</returns>
        public Task ContinueDownload(ulong byteOffset)
        {
            return Send($"REST {byteOffset}\r\n");
        }

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        /// <param name="file">File path to download.</param>
        /// <returns>The task representing the process.</returns>
        public Task Download(string file)
        {
            return Send($"RETR {file}\r\n");
        }

        /// <summary>
        /// Removes a directory on the server.
        /// </summary>
        /// <param name="directory">Directory path to remove.</param>
        /// <returns>The task representing the process.</returns>
        public Task RemoveDirectory(string directory)
        {
            return Send($"RMD {directory}\r\n");
        }

        /// <summary>
        /// Specifies the current file to be renamed.
        /// </summary>
        /// <param name="oldFile">Current name of the file on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task RenameFile(string oldFile)
        {
            return Send($"RNFR {oldFile}\r\n");
        }

        /// <summary>
        /// Renames the file specified by RenameFile to a new name.
        /// </summary>
        /// <param name="newFile">New name for the file.</param>
        /// <returns>The task representing the process.</returns>
        public Task RenameTo(string newFile)
        {
            return Send($"RNTO {newFile}\r\n");
        }

        /// <summary>
        /// Executes a SITE-specific command on the server.
        /// </summary>
        /// <param name="command">Command to execute on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task ShellExecute(string command)
        {
            return Send($"SITE {command}\r\n");
        }

        /// <summary>
        /// Gets the size of a specified file on the server.
        /// </summary>
        /// <param name="file">File path on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task GetSize(string file)
        {
            return Send($"SIZE {file}\r\n");
        }

        /// <summary>
        /// Requests server status information.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task Status()
        {
            return Send("STAT\r\n");
        }

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        /// <param name="file">File path to upload.</param>
        /// <returns>The task representing the process.</returns>
        public Task Store(string file)
        {
            return Send($"STOR {file}\r\n");
        }

        /// <summary>
        /// Creates and stores a new file with a unique name.
        /// </summary>
        /// <param name="file">Base name of the file to create.</param>
        /// <returns>The task representing the process.</returns>
        public Task CreateNewFile(string file)
        {
            return Send($"STOU {file}\r\n");
        }

        /// <summary>
        /// Sets the file structure for file transfers.
        /// </summary>
        /// <param name="fileStructure">The file structure (e.g., file, record, page).</param>
        /// <returns>The task representing the process.</returns>
        public Task SetFileStructure(FtpFileStructure fileStructure)
        {
            return Send($"STRU {fileStructure.ToString()[0]}\r\n");
        }

        /// <summary>
        /// Sets the transfer type (ASCII or binary) for data transfers.
        /// </summary>
        /// <param name="transferType">Transfer type to set.</param>
        /// <returns>The task representing the process.</returns>
        public Task SetTransferType(FtpTransferType transferType)
        {
            return Send($"TYPE {transferType.ToString()[0]}\r\n");
        }

        /// <summary>
        /// Sends the username to authenticate the user on the server.
        /// </summary>
        /// <param name="username">Username to authenticate with.</param>
        /// <returns>The task representing the process.</returns>
        public Task User(string username)
        {
            return Send($"USER {username}\r\n");
        }

        /// <summary>
        /// Aborts the current file transfer.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task Abort()
        {
            return Send("ABOR\r\n");
        }

        /// <summary>
        /// Sends the CDUP command to navigate to the parent directory.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task ChangeToParentDirectory()
        {
            var result = Send("CDUP\r\n");
            ChangePath("..");
            return result;
        }

        /// <summary>
        /// Sends the PASV command to enter passive mode.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task PassiveMode()
        {
            return Send("PASV\r\n");
        }

        /// <summary>
        /// Sends the REIN command to reinitialize the FTP connection.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task Reinitialize()
        {
            return Send("REIN\r\n");
        }

        /// <summary>
        /// Closes the connection to the FTP server.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task Quit()
        {
            return Send("QUIT\r\n");
        }

        /// <summary>
        /// Prints the current working directory on the server.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task PrintWorkingDirectory()
        {
            return Send("PWD\r\n");
        }

        /// <summary>
        /// Requests information about the server operating system.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task SystemInfo()
        {
            return Send("SYST\r\n");
        }

        /// <summary>
        /// Sends a no-operation command to keep the connection alive.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task NoOperation()
        {
            return Send("NOOP\r\n");
        }

        /// <summary>
        /// Prints the current working directory using an extended command.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task ExtendedPrintWorkingDirectory()
        {
            return Send("XPWD\r\n");
        }

        /// <summary>
        /// Changes the current working directory using an extended command.
        /// </summary>
        /// <param name="path">The path of the new directory.</param>
        /// <returns>The task representing the process.</returns>
        public Task ExtendedChangeWorkingDirectory(string path)
        {
            var result = Send($"XCWD {path}\r\n");
            ChangePath(path);
            return result;
        }

        /// <summary>
        /// Retrieves detailed directory information using the MLSD command.
        /// </summary>
        /// <returns>The task representing the process.</returns>
        public Task GetFolderInfo()
        {
            return Send("MLSD\r\n");
        }

        /// <summary>
        /// Retrieves detailed file information for a specified file using the MLST command.
        /// </summary>
        /// <param name="file">File path on the server.</param>
        /// <returns>The task representing the process.</returns>
        public Task GetFileInfo(string file)
        {
            return Send($"MLST {file}\r\n");
        }

        protected override void DisposeManagedResources()
        {
            Disconnect();
        }

        public async Task Send(string command)
        {
            if (!NetUtils.IsSocketConnected(socket))
            {
                throw new InvalidOperationException("Not connected to the FTP server.");
            }

            var commandBytes = Encoding.GetBytes(command);
            using (var sendArgs = new SocketAsyncEventArgs { RemoteEndPoint = socket.RemoteEndPoint })
            {
                sendArgs.SetBuffer(commandBytes, 0, commandBytes.Length);

                var sendTaskCompletion = new TaskCompletionSource<bool>();
                sendArgs.Completed += (s, e) => sendTaskCompletion.TrySetResult(true);

                if (!socket.SendAsync(sendArgs))
                {
                    _ = sendTaskCompletion.TrySetResult(true);
                }

                _ = await sendTaskCompletion.Task.ConfigureAwait(false);
                MessageSent?.Invoke(this, new MessageEventArgs(command));
            }
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            var responseBuffer = new byte[8192];
            using (var receiveArgs = new SocketAsyncEventArgs())
            {
                receiveArgs.SetBuffer(responseBuffer, 0, responseBuffer.Length);

                var receiveTaskCompletion = new TaskCompletionSource<int>();
                receiveArgs.Completed += (s, e) =>
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        _ = receiveTaskCompletion.TrySetResult(e.BytesTransferred);
                    }
                    else
                    {
                        var socketException = new SocketException((int)e.SocketError);
                        OnErrorOccurred(new ExceptionEventArgs(socketException));
                        _ = receiveTaskCompletion.TrySetException(socketException);
                    }
                };

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!socket.ReceiveAsync(receiveArgs))
                        {
                            _ = receiveTaskCompletion.TrySetResult(receiveArgs.BytesTransferred);
                        }

                        var bytesRead = await receiveTaskCompletion.Task.ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var response = Encoding.GetString(responseBuffer, 0, bytesRead);
                        MessageReceived?.Invoke(this, new MessageEventArgs(response));
                        receiveTaskCompletion = new TaskCompletionSource<int>();
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new ExceptionEventArgs(ex));
                }
            }
        }

        private void InitializeDataSocket()
        {
            dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //dataSocket.Bind(localFtpDataEndPoint);
            dataSocket.Bind(new IPEndPoint(IPAddress.Any, localFtpDataEndPoint.Port));
            dataSocket.Listen(1);
        }

        private async Task ReceiveDataAsync(Socket dataSocket, CancellationToken token)
        {
            var buffer = new byte[8192];
            using (var receiveArgs = new SocketAsyncEventArgs())
            {
                receiveArgs.SetBuffer(buffer, 0, buffer.Length);

                var receiveTaskCompletion = new TaskCompletionSource<int>();
                receiveArgs.Completed += (s, e) =>
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        _ = receiveTaskCompletion.TrySetResult(e.BytesTransferred);
                    }
                    else
                    {
                        var socketException = new SocketException((int)e.SocketError);
                        OnErrorOccurred(new ExceptionEventArgs(socketException));
                        _ = receiveTaskCompletion.TrySetException(socketException);
                    }
                };

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!dataSocket.ReceiveAsync(receiveArgs))
                        {
                            _ = receiveTaskCompletion.TrySetResult(receiveArgs.BytesTransferred);
                        }

                        var bytesRead = await receiveTaskCompletion.Task.ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        DataArrived?.Invoke(this, new DataArrivedEventArgs(dataSocket, data));

                        receiveTaskCompletion = new TaskCompletionSource<int>();
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new ExceptionEventArgs(ex));
                }
            }
        }

        protected void OnErrorOccurred(ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
    }
}
