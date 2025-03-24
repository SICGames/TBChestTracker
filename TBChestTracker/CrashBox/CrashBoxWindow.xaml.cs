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
using com.HellStormGames.Diagnostics;

namespace TBChestTracker.Dialogs
{
    /// <summary>
    /// Interaction logic for CrashBoxWindow.xaml
    /// </summary>
    public partial class CrashBoxWindow : Window
    {
        bool viewingReport = false;
        readonly Exception ExceptionObject;
        public CrashBoxWindow(Exception exception)
        {
            ExceptionObject = exception;
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Loggio.Fatal(ExceptionObject, "Application Crash", $"Crash has occured. Reason: {ExceptionObject.Message}. More information in log file.");
        }

        private void ViewCrashReportBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //-- make sure we save everything
            Loggio.Shutdown();
            Application.Current.Shutdown();
        }
    }
}
