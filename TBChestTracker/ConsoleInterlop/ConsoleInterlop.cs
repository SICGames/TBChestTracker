using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker
{
    public class ConsoleInterop
    {
        public string? Application { get; private set; }
        public string? Arguments { get; private set; }
        public string? WorkingDirectoryPath { get; private set; }
        private Process? pProcess;
        private CancellationTokenSource? CancelTokenSource;

        public event EventHandler<ConsoleInterlopEventArgs>? DataReceived;
        protected virtual void onDataReceived(ConsoleInterlopEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public ConsoleInterop(string? application, string? arguments = "", string? workingDirectoryPath = "")
        {
            if (String.IsNullOrEmpty(application))
            {
                throw new ArgumentNullException(nameof(application));
            }
            this.Application = application;
            this.Arguments = arguments;
            this.WorkingDirectoryPath = workingDirectoryPath;

        }
        private Task<bool> ExecuteProcessTask(bool waitForExit, bool runAsAdmin = false)
        {
            try
            {
                pProcess = new Process();
                pProcess.StartInfo.FileName = Application;
                pProcess.StartInfo.Arguments = Arguments;
                //pProcess.StartInfo.WorkingDirectory = WorkingDirectoryPath;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.EnableRaisingEvents = true;
                if (runAsAdmin)
                {
                    pProcess.StartInfo.Verb = "runas";
                }
                pProcess.Start();
                pProcess.OutputDataReceived += PProcess_OutputDataReceived;

                pProcess.BeginOutputReadLine();
                if (waitForExit)
                {
                    pProcess.WaitForExit();
                }
            
            }
            catch (Exception ex)
            {
                if(MessageBox.Show($"Failed to start {Application}. Reason: {ex.Message}") == MessageBoxResult.OK)
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }
        private Task KillTask()
        {
            return Task.Run(() =>
            {
                if (pProcess != null)
                {
                    if (!pProcess.HasExited)
                    {
                        pProcess.Kill();
                    }
                }
            });
        }
        private void PProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
            bool isCompleted = false;
            if (data != null)
            {
                isCompleted = false;
            }
            else
            {
                isCompleted = true;
            }

            var args = new ConsoleInterlopEventArgs(data, isCompleted);
            onDataReceived(args);
        }

        public async Task<bool> Run(CancellationToken? token, bool bWaitForExit = true)
        {
            try
            {
                bool result = await ExecuteProcessTask(bWaitForExit);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                await Kill();
            }
            return false;
        }

        public async Task Kill()
        {
            await KillTask();
        }
    }
}
