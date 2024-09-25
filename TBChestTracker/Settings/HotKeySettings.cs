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
        private string _StartAutomationKeys;
        private string _StopAutomationKeys;

        public string StartAutomationKeys {
            get
            {
               return String.IsNullOrEmpty(_StartAutomationKeys) == true ? "F9" : _StartAutomationKeys;
            }
            set
            {
                _StartAutomationKeys = value;
            }
        }
        public string StopAutomationKeys
        { 
            get
            {
                return String.IsNullOrEmpty(_StopAutomationKeys) == true ? "F10" : _StopAutomationKeys;
            }
            set
            {
                _StopAutomationKeys = value;
            }
        }

        public HotKeySettings() 
        { 
        }

    }
}
