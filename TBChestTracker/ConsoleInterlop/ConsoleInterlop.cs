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
        //-- status codes
        private const int CONSOLE_INTERLOP_MESSAGE = 100;
        private const int CONSOLE_INTERLOP_SUCCESS = 200;
        private const int CONSOLE_INTERLOP_NOTFOUND = 404;
        private const int CONSOLE_INTERLOP_ERROR = 500;

        public string? Application { get; private set; }
        public string? Arguments { get; private set; }
        public string? WorkingDirectoryPath { get; private set; }
        public bool? RunAsAdmin { get; private set; }
        public int? ExitCode { get; private set; }

        private Process? pProcess;
        private CancellationTokenSource? CancelTokenSource;

        public event EventHandler<ConsoleInterlopEventArgs>? DataReceived;
        public event EventHandler<ConsoleInterlopCompletedEventArgs>? Completed;
        public event EventHandler<ConsoleInterlopErrorEventArgs>? Error;

        protected virtual void onDataReceived(ConsoleInterlopEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }
        protected virtual void onCompleted(ConsoleInterlopCompletedEventArgs e)
        {
            Completed?.Invoke(this, e); 
        }
        protected virtual void onError(ConsoleInterlopErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        public ConsoleInterop(string? application, string? arguments = "", string? workingDirectoryPath = "", bool? runAsAdmin = false)
        {
            if (String.IsNullOrEmpty(application))
            {
                throw new ArgumentNullException(nameof(application));
            }
            this.Application = application;
            this.Arguments = arguments;
            this.WorkingDirectoryPath = workingDirectoryPath;
            this.RunAsAdmin = runAsAdmin;
        }
        private Task<bool> ExecuteProcessTask(bool waitForExit)
        {
            try
            {
                pProcess = new Process();
                pProcess.StartInfo.FileName = Application;
                pProcess.StartInfo.Arguments = Arguments;
                if (String.IsNullOrEmpty(WorkingDirectoryPath) == false)
                {
                    pProcess.StartInfo.WorkingDirectory = WorkingDirectoryPath;
                }
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                bool bDebug;
#if DEBUG
                bDebug = true;
#else
                bDebug = false;
#endif

                pProcess.StartInfo.CreateNoWindow = !bDebug;
                pProcess.EnableRaisingEvents = true;
                if (RunAsAdmin == true)
                {
                    pProcess.StartInfo.Verb = "runas";
                }
                pProcess.Start();
                pProcess.OutputDataReceived += PProcess_OutputDataReceived;
                pProcess.Exited += (s, e) =>
                {
                    ExitCode = pProcess.ExitCode;
                };

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
            //--- CODE: 200\tMESSAGE: Doing Something Awesome!\t50%
            //--- 200\tDoing Something Really Fancy Over Here!\t50%
            //--- [CODE][MESSAGE][PERCENT]

            var data = e.Data;
            if (data != null)
            {
                var dataArray = data.Split('\t');
                int status_code;
                Int32.TryParse(dataArray[0], out status_code);
                var msg = dataArray[1];
                var percentStr = dataArray[2];
                
                
                if (status_code == CONSOLE_INTERLOP_MESSAGE)
                {
                    if (percentStr.Contains("%"))
                    {
                       percentStr = percentStr.Substring(0, percentStr.IndexOf("%"));
                    }
                    var percent = Convert.ToDouble(percentStr);
                    //Double.TryParse(percentStr, out percent);

                    var args = new ConsoleInterlopEventArgs(status_code, msg, percent);
                    onDataReceived(args);
                }
                else if(status_code == CONSOLE_INTERLOP_SUCCESS)
                {
                    //-- raise the success 
                    var completeArgs = new ConsoleInterlopCompletedEventArgs(true);
                    onCompleted(completeArgs);
                }
                else if(status_code == CONSOLE_INTERLOP_ERROR || status_code == CONSOLE_INTERLOP_NOTFOUND)
                {
                    var errorArgs = new ConsoleInterlopErrorEventArgs(msg, status_code);
                    onError(errorArgs);
                }
            }
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
