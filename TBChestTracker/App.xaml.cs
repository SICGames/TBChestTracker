
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TBChestTracker.Localization;


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
            MainWindow mainwnd = new MainWindow();
            StartPageWindow startPageWindow = new StartPageWindow();
            
            startPageWindow.MainWindow = mainwnd;

            SplashScreen splashScreen = new SplashScreen();

            splashScreen.onSplashScreenComplete += SplashScreen_onSplashScreenComplete;
            splashScreen.mainWindow = mainwnd;
            splashScreen.startPageWindow = startPageWindow;
            
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
            }

            //-- configure crashbox
            CrashBox crashBox = new CrashBox();

            splashScreen.Show();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("I AM PROCESS KILLER! MUAHAHAHA!");
        }

        private void SplashScreen_onSplashScreenComplete(object sender, EventArgs e)
        {
            var splash = (SplashScreen)sender;
            var startpage = splash.startPageWindow;
            startpage.Show();

            splash.Close(); 
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            
            Console.WriteLine("I AM EXITING!!!");
        }
    }
}
