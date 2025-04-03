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
            CrashBoxWindow crashBoxWindow = new CrashBoxWindow(ex);
            crashBoxWindow.ShowDialog();
        }
    }
}
