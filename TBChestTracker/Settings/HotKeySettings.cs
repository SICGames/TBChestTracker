using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TBChestTracker
{
    [System.Serializable]
    public class HotKeySettings
    {
        public string StartAutomationKeys { get; set; }
        public string StopAutomationKeys { get; set; }
        public HotKeySettings() 
        { 
        }

    }
}
