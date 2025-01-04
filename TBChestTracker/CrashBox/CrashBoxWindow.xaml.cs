using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TBChestTracker.Managers;
using CefSharp;
using CefSharp.Wpf;

namespace TBChestTracker.Dialogs
{
    /// <summary>
    /// Interaction logic for CrashBoxWindow.xaml
    /// </summary>
    public partial class CrashBoxWindow : Window
    {
        public Exception exception { get; set; }
        bool viewingReport = false;
        public CrashBoxWindow()
        {
            InitializeComponent();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //-- make sure we save everything
            ClanManager.Instance.ClanChestManager.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(exception == null)
            {
                //-- somehow this is odd. And shouldn't happen. 
                exception = new Exception("CrashBox exception was null. Odd.");
            }
            var reasonOfCrash = exception;
            var crashDate = DateTime.Now;
            var crashTime = DateTime.Now;

            var crashDateStr = DateTime.Now.ToString(@"d");
            var crashTimeStr = DateTime.Now.ToString(@"t");

            if(!Directory.Exists($@"{AppContext.Instance.CommonAppFolder}\Logs"))
            {
                Directory.CreateDirectory($@"{AppContext.Instance.CommonAppFolder}\Logs");
            }

            var crashLogFile = $@"{AppContext.Instance.CommonAppFolder}\Logs\crash_{crashDate.ToString(@"yyyy-MM-dd")}_{crashTime.ToString(@"HHHH:mm:ss")}.log";

            using (var sw = new StreamWriter(crashLogFile, true))
            {
                var crashMessage = $"Crash Log {crashDateStr} - {crashTimeStr}:\n {reasonOfCrash.Message} \n {reasonOfCrash.StackTrace.ToString()}\n";
                sw.WriteLine(crashMessage);
                sw.Close();
            }
        }

        private void ViewCrashReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewingReport == false)
            {
                System.Diagnostics.Process.Start($@"{AppContext.Instance.CommonAppFolder}\Logs\crash.log");
                viewingReport = true;
            }
            this.Close();
        }
    }
}
