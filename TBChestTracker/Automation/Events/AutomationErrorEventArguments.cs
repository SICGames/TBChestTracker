using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationErrorEventArguments : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public AutomationErrorEventArguments(string errorMessage) 
        { 
            this.ErrorMessage = errorMessage;
        }
    }
}
