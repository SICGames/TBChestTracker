using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
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
            }

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
