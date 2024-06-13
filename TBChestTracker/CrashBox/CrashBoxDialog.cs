using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Dialogs;

namespace TBChestTracker
{
    public class CrashBoxDialog
    {
        public static void Show(Exception e)
        {
            CrashBoxWindow crashBoxWindow = new CrashBoxWindow();
            crashBoxWindow.exception = e;
            crashBoxWindow.ShowDialog();
        }
    }
}
