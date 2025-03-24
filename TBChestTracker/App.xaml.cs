
using System;
using System.Collections.Generic;
using System.Windows;
using TBChestTracker.Localization;

using com.HellStormGames.Diagnostics;
using com.HellStormGames.Diagnostics.Logging;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppContext appContext = new AppContext();

            var logsFolder = $"{AppContext.Instance.LocalApplicationPath}Logs\\";
            var logFile = $"loggio_{DateTimeOffset.Now.ToString(@"yyyy-MM-dd")}.log";
            var logfilepath = logsFolder + logFile;
#if DEBUG
            Loggio.Logger = new LoggioConfiguration().SubscribeTo.DebugOutput()
                .SubscribeTo.File(logfilepath, LoggioEventType.VERBOSE).CreateLogger();
#else
            Loggio.Logger = new LoggioConfiguration().SubscribeTo.File(logfilepath, LoggioEventType.VERBOSE).CreateLogger();
#endif
          
            
            Dictionary<string,string> argumentsDictionary = new Dictionary<string,string>();
            if(e.Args.Length > 0)
            {
                string[] args = Environment.GetCommandLineArgs();
                for (var x = 1; x < args.Length; x++)
                {
                    argumentsDictionary.Add(args[x], args[x]);
                }
                if(argumentsDictionary.ContainsKey("--ocr_wizard_debug"))
                {
                    appContext.DebugOCRWizardEnabled = true;
                }
                else if(argumentsDictionary.ContainsKey("--save_ocr_images"))
                {
                    appContext.SaveOCRImages = true;
                }
                else if(argumentsDictionary.ContainsKey("--delete_tessdata"))
                {
                    appContext.bDeleteTessData = true;
                }
                else if(argumentsDictionary.ContainsKey("--force-us-locale"))
                {
                    //-- Force to English
                    LocalizationManager.Set("en-US");
                }
                else if(argumentsDictionary.ContainsKey("--debug"))
                {
                    AppContext.ShowDebugMenu(true);
                }
            }

#if DEBUG
            AppContext.ShowDebugMenu(true);
#endif

            //-- configure crashbox
            CrashBox crashBox = new CrashBox();

            MainWindow mainwnd = new MainWindow();
            StartPageWindow startPageWindow = new StartPageWindow();
            startPageWindow.MainWindow = mainwnd;

            SplashScreen splashScreen = new SplashScreen();
            splashScreen.onSplashScreenComplete += SplashScreen_onSplashScreenComplete;
            splashScreen.mainWindow = mainwnd;
            splashScreen.startPageWindow = startPageWindow;

            Loggio.Info($"Total Battle Chest Tracker ({AppContext.Instance.AppVersionString})  is starting...");
            splashScreen.Show();
        }
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            
        }

        private void SplashScreen_onSplashScreenComplete(object sender, EventArgs e)
        {
            var splash = (SplashScreen)sender;
            var startpage = splash.startPageWindow;

            startpage.Show();

            splash.Close(); 
            Loggio.Info($"Splash Screen closing..."); 
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Loggio.Info("Total Battle Chest Tracker is exiting...");
            Loggio.Shutdown();
        }
    }
}
