using CefSharp;
using System;
using System.Collections.Generic;
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

namespace TBChestTracker.Windows.WebViewWindow
{
    /// <summary>
    /// Interaction logic for WebViewWindow.xaml
    /// </summary>
    public partial class WebViewWindow : Window
    {
        private ChromiumWebBrowser Browser = null;
        readonly Uri uri;
        public WebViewWindow(Uri uri)
        {
            InitializeComponent();
            this.uri = uri;
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
            catch (IOException ex)
            {
                //-- couldn't create.
            }
        }

        private void InitBrowser()
        {
            CefSettings settings = new CefSettings();

            //--- CefSharp Cache, UserDir will be stored in ProgramData
            var cache_dir = $@"{AppContext.Instance.LocalApplicationPath}Cache";
            var user_dir = $@"{AppContext.Instance.LocalApplicationPath}User";
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
            Browser = new ChromiumWebBrowser(uri.ToString());
            WebViewControl.Children.Add(Browser);
        }

        private void DestroyBrowser()
        {
            Browser.Dispose();
            WebViewControl.Children.Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            InitBrowser();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyBrowser();
        }
    }
}
