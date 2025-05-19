using Mtf.Network.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

namespace Mtf.Network.Services
{
    public static class ProcessUtils
    {
        public static void KillProcesses(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            KillProcesses(processes);
            FreeProcessArrayResources(processes);
        }

        public static void KillProcesses(params Process[] processes)
        {
            for (var i = 0; i < processes.Length; i++)
            {
                KillProcessIfNecessary(processes[i]);
            }
        }

        public static void KillProcessIfNecessary(Process process)
        {
            if ((process != null) && (!process.HasExited))
            {
                process.Kill();
            }
        }

        public static void FreeProcessArrayResources(params Process[] processes)
        {
            foreach (var process in processes)
            {
                process.Close();
            }
        }

        public static Process RunProgramOrFile(string filename, string arguments)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Starts an application on a remote machine using PsExec.
        /// This method handles constructing the PsExec command with authentication details.
        /// </summary>
        /// <param name="processPath">The path or name of the process to start on the remote machine (e.g., "calc" or "C:\Windows\System32\notepad.exe").</param>
        /// <param name="computerName">The hostname or IP address of the remote machine (e.g., "192.168.0.246").</param>
        /// <param name="username">The username for authentication on the remote machine. This can be a local account or a domain account.</param>
        /// <param name="password">The password for the specified username.</param>
        /// <param name="domain">The domain name for domain user authentication. If using a local user account on a workgroup machine, leave this parameter as null.</param>
        /// <param name="psExecFolderPath">The local folder path where the PsExec.exe executable is located (e.g., "C:\PsExec").</param>
        /// <param name="encoding">The encoding of the standard output and standard error.</param>
        /// <param name="outputDataReceived">An event handler to capture standard output from the remote process. If set, standard output will be redirected.</param>
        /// <param name="errorDataReceived">An event handler to capture standard error from the remote process. If set, standard error will be redirected.</param>
        /// <returns>Returns the started Process object representing the PsExec execution.</returns>
        /// <remarks>
        /// <para>
        /// **Important Considerations for PsExec:**
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term>User Rights:</term>
        /// <description>The provided username on the remote machine must have the "Log on as a batch job" permission assigned. This is configured in the target machine's Local Security Policy (Security Settings &gt; Local Policies &gt; User Rights Assignment).</description>
        /// </item>
        /// <item>
        /// <term>GUI Applications:</term>
        /// <description>Starting GUI applications (like Notepad) remotely via PsExec will cause them to run in a non-interactive session. They will not be visible on the remote desktop unless you are logged into the same session as the PsExec execution. You may need to use tools like `taskkill` via PsExec to terminate them.</description>
        /// </item>
        /// <item>
        /// <term>Local Elevation:</term>
        /// <description>If `outputDataReceived` and `errorDataReceived` are both null, this method attempts to run PsExec with elevated privileges on the local machine (via a UAC prompt). If you are redirecting output/error, your calling application must already be running with sufficient privileges.</description>
        /// </item>
        /// <item>
        /// <term>Firewall:</term>
        /// <description>Ensure that Windows Firewall on the remote machine is configured to allow inbound remote administration (File and Printer Sharing exceptions, or specific ports used by PsExec's RPC communication).</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static Process StartApplicationWithPsExec(string processPath, string computerName = "localhost",
            string username = null, string password = null, string domain = null,
            string psExecFolderPath = @"C:\PsExec", Encoding encoding = null,
            DataReceivedEventHandler outputDataReceived = null, DataReceivedEventHandler errorDataReceived = null)
        {
            var args = new StringBuilder();
            args.Append($@"\\{computerName} ");

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                if (!String.IsNullOrEmpty(domain))
                {
                    args.Append($"-u {domain}\\{username} ");
                }
                else
                {
                    args.Append($"-u {username} ");
                }

                args.Append($"-p {password} ");
            }

            args.Append(processPath);

            var useShellExecute = outputDataReceived == null && errorDataReceived == null;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    //FileName = "cmd",
                    //Arguments = String.Concat("/k", " ", Path.Combine(psExecFolderPath, "PsExec.exe"), " ", args.ToString()),
                    FileName = Path.Combine(psExecFolderPath, "PsExec.exe"),
                    Arguments = args.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = useShellExecute,
                    RedirectStandardOutput = !useShellExecute && outputDataReceived != null,
                    RedirectStandardError = !useShellExecute && errorDataReceived != null,
                    Verb = useShellExecute ? "runas" : String.Empty,
                    StandardOutputEncoding = encoding,
                    StandardErrorEncoding = encoding
                }
            };
            if (outputDataReceived != null)
            {
                process.OutputDataReceived += outputDataReceived;
            }
            if (errorDataReceived != null)
            {
                process.ErrorDataReceived += errorDataReceived;
            }
            process.Start();
            if (outputDataReceived != null)
            {
                process.BeginOutputReadLine();
            }
            if (errorDataReceived != null)
            {
                process.BeginErrorReadLine();
            }
            return process;
        }

        public static void ExecuteCommand(string command, IProcessResultParser processResultParser)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/C {command}",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                process.ErrorDataReceived += processResultParser.ErrorDataReceived;
                process.OutputDataReceived += processResultParser.OutputDataReceived;
                process.Start();
                process.WaitForExit();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }
        }
    }
}
