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
            ClanManager.Instance.ClanChestManager.SaveData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reasonOfCrash = exception;
            var crashDate = DateTime.Now.ToString(@"d");
            var crashTime = DateTime.Now.ToString(@"t");
            if(!Directory.Exists($@"{AppContext.Instance.CommonAppFolder}\Logs"))
            {
                Directory.CreateDirectory($@"{AppContext.Instance.CommonAppFolder}\Logs");
            }

            var crashLogFile = $@"{AppContext.Instance.CommonAppFolder}\Logs\crash.log";

            using (var sw = File.AppendText(crashLogFile))
            {
                var crashMessage = $"Crash Log {crashDate} - {crashTime}:\n {reasonOfCrash.Message} \n {reasonOfCrash.StackTrace.ToString()}\n";
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
