using Mtf.Network.Interfaces;
using System;
using System.Diagnostics;

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
