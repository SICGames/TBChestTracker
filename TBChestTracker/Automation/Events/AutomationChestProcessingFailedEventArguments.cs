using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationChestProcessingFailedEventArguments : EventArgs
    {
        public string ErrorMessage { get; set; }
        public int Code { get; set; }
        public AutomationChestProcessingFailedEventArguments(string errorMessage, int code)
        {
            ErrorMessage = errorMessage;
            Code = code;
        }       
    }
}
