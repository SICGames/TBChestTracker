using CefSharp.DevTools.FedCm;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Windows.WebViewWindow;

namespace TBChestTracker.Web
{
    //-- Not WebView known in Win3 UI or Android
    
    public class WebView
    {
        public WebView() 
        { 
        }
        
        public static void Show(String title, String Uri)
        {
            ShowWindow(title, Uri);
        }
        public static void Show(String Title, String Uri, int width, int height)
        {
            ShowWindow(Title, Uri, width, height);
        }
        public static void Show(String Title,String Uri, int width, int height, bool topmost)
        {
            ShowWindow(Title, Uri, width, height, topmost);
        }
        public static void Show(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar)
        {
            ShowWindow(title, Uri,width, height, topmost, showInTaskbar);
        }

        public static void Show(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar, bool isMaximized)
        {
            ShowWindow(title, Uri, width, height, topmost, showInTaskbar, isMaximized);
        }

        public static void Show(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar, bool isMaximized, bool canResize)
        {
            ShowWindow(title, Uri, width, height, topmost, showInTaskbar, isMaximized, canResize);
        }
        private static void  ShowWindow(String title, String Uri)
        {
            CreateWindow(title, Uri);
        }
        private static void ShowWindow(String title, String Uri, int width, int height) 
        {
            CreateWindow(title, Uri, width, height);
        }
        
        private static void ShowWindow(String title, String Uri, int width, int height, bool topmost)
        {
            CreateWindow(title, Uri, width, height, topmost);
        }
        private static void ShowWindow(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar)
        {
            CreateWindow(title, Uri, width, height, topmost, showInTaskbar);
        }
        private static void ShowWindow(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar, bool isMaximized)
        {
            CreateWindow(title, Uri, width, height, topmost, showInTaskbar, isMaximized);
        }
        private static void ShowWindow(String title, String Uri, int width, int height, bool topmost, bool showInTaskbar, bool isMaximized, bool canResize)
        {
            CreateWindow(title, Uri, width, height, topmost, showInTaskbar, isMaximized, canResize);
        }

        private static void CreateWindow(String title, String Uri, int width = 300, int height = 480, bool? topmost = true, bool? showinTaskbar = true, bool? isMaximized = false, bool? canResize = true)
        {
            //-- maybe delegate this incase 
            if(String.IsNullOrEmpty(Uri)) 
            {
                throw new ArgumentNullException(nameof(Uri));   
            }

            WebViewWindow webViewWindow = new WebViewWindow(new Uri(Uri, UriKind.RelativeOrAbsolute));
            webViewWindow.Title = title;
            webViewWindow.Width = width;
            webViewWindow.Height = height;
            webViewWindow.Topmost = topmost.Value;
            webViewWindow.ShowInTaskbar = showinTaskbar.Value;
            webViewWindow.WindowState = isMaximized.HasValue ? 
                                        (isMaximized.Value == true ? 
                                        System.Windows.WindowState.Maximized : System.Windows.WindowState.Normal) : System.Windows.WindowState.Normal;
            if(canResize.HasValue)
            {
                bool ableToResize = canResize.Value;
                webViewWindow.ResizeMode = ableToResize == true ? System.Windows.ResizeMode.CanResize : System.Windows.ResizeMode.NoResize;
            }
            webViewWindow.Show();
        }
    }
}
