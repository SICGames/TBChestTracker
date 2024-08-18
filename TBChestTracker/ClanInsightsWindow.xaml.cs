using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using CefSharp;
using CefSharp.Core;
using CefSharp.Wpf;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanInsightsWindow.xaml
    /// </summary>
    public partial class ClanInsightsWindow : Window
    {

        public ChromiumWebBrowser Browser { get; private set; }
        public ClanInsightsWindow()
        {
            InitializeComponent();
        }

        private bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);    
        }
        private void CreateDirectory(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch(IOException ex)
            {
                //-- couldn't create.
            }
        }

        private void InitBrowser()
        {
            CefSettings settings = new CefSettings();

            //--- CefSharp Cache, UserDir will be stored in ProgramData
            var cache_dir = $@"{GlobalDeclarations.CommonAppFolder}Cache";
            var user_dir = $@"{GlobalDeclarations.CommonAppFolder}User";
            if (!DirectoryExists(cache_dir))
            {
                CreateDirectory(cache_dir);
            }
            if (!DirectoryExists(user_dir))
            {
                CreateDirectory(user_dir);
            }

            settings.CachePath = cache_dir;
            if (Cef.IsInitialized == false)
            {
                Cef.Initialize(settings);
            }

            var node_server = "http://127.0.0.1:8888/";
            
            Browser = new ChromiumWebBrowser(node_server);
            BROWSER_CONTAINER.Children.Add(Browser);
        }

        private void DestroyBrowser()
        {
            Browser.Dispose();
            BROWSER_CONTAINER.Children.Clear();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitBrowser();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyBrowser();
        }
    }
}
