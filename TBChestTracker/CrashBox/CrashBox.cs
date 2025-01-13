using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TBChestTracker
{

    //--- Handles Uncaught Exceptions, displays window and has user submit bug.
    //--- most preferred method is to use HTTPS post method. 
    //--- Since users are more cautious about privacy, allow submit anonymously. 
    //--- Wouldn't be a bad thing to collect OS, hardware but again, would need to require users agree to privacy policy agreement when launching application. 
    public class CrashBox
    {
        public CrashBox() 
        {
            //-- catches every exception across application domain
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //--- catches exceptions tossed by any function using async operations. 
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }
        private void ShowCrashBoxDialog(Exception exception)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                CrashBoxDialog.Show(exception);
            }));
            thread.Start();
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
            ShowCrashBoxDialog(e.ExceptionObject as Exception);
        }
    }
}
