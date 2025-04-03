using com.HellStormGames.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TBChestTracker.Crashhandling;

namespace TBChestTracker
{

    //--- Handles Uncaught Exceptions, displays window and has user submit bug.
    //--- most preferred method is to use HTTPS post method. 
    //--- Since users are more cautious about privacy, allow submit anonymously. 
    //--- Wouldn't be a bad thing to collect OS, hardware but again, would need to require users agree to privacy policy agreement when launching application. 
    public class CrashBox
    {
        bool showned = false;
        public CrashBox() 
        {
            //-- catches every exception across application domain
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //--- catches exceptions tossed by any function using async operations. 
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            
        }
        private void ShowCrashBoxDialog(Exception ex)
        {
            if (!showned)
            {
                try
                {
                    Loggio.Shutdown();

                    var logspath = $"{AppContext.Instance.LocalApplicationPath}Logs";
                    var crashFile = $"{logspath}\\Crash.log";

                    var cr = new CrashReport(crashFile);

                    var stacktrace = ex.StackTrace;
                    cr.CrashException.StackTrace = stacktrace.ToString();
                    cr.CrashException.Message = ex.Message;
                    cr.CrashException.ExceptionType = ex.GetType().Name;

                    cr.Generate();

                    //-- startup new Crash Box Procecss.
                    var args = $"--app_name \"TotalBattle Chest Tracker\"";
                    var crashboxApp = $"CrashBox.exe";
                    var p = Process.Start(crashboxApp, args);
                   
                    showned = true;
                }
                catch (Exception ex2)
                {
                    throw new Exception(ex2.Message);
                }
            }
        }
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowCrashBoxDialog(e.Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowCrashBoxDialog(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            ShowCrashBoxDialog(ex);
        }
    }
}
