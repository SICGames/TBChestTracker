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
        public static void Show(Exception e)
        {
            try
            {
                CrashBoxWindow crashBoxWindow = new CrashBoxWindow();
                crashBoxWindow.exception = e;
                crashBoxWindow.Show();
            }
            catch (Exception ex)
            {
                //-- don't do anything. 
                Loggio.Error(ex, "Crash", "A crash occured.");
            }
        }
    }
}
