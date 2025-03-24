using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Dialogs;
using com.HellStormGames.Diagnostics;

namespace TBChestTracker
{
    public class CrashBoxDialog
    {
        public static void Show(Exception ex)
        {
            
            try
            {
                CrashBoxWindow crashBoxWindow = new CrashBoxWindow(ex);
                crashBoxWindow.ShowDialog();
            }
            catch (Exception e)
            {
                Loggio.Fatal(e, "Application Crash", $"Attempt to catch issue raised exception inside CrashBox.");
                Loggio.Shutdown();
            }
        }
    }
}
