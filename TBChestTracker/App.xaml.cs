using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

            LocalizationManager.Set("fr-FR");


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
                for (var x = 0; x < args.Length; x++)
                {
                    argumentsDictionary.Add(args[x], args[x + 1]);
                }
                if(argumentsDictionary.ContainsKey("--ocr_wizard_debug"))
                {
                    GlobalDeclarations.DebugOCRWizardEnabled = true;
                }
                else if(argumentsDictionary.ContainsKey("--save_ocr_images"))
                {
                    GlobalDeclarations.SaveOCRImages = true;
                }
            }

            //-- configure crashbox
            CrashBox crashBox = new CrashBox();

            splashScreen.Show();
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

        }
    }
}
