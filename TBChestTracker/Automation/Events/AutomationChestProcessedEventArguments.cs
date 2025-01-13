using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationChestProcessedEventArguments : EventArgs
    {
        public ClanChestProcessResult ProcessResult { get; set; }
        public AutomationChestProcessedEventArguments(ClanChestProcessResult Result) 
        { 
            ProcessResult = Result;
        }
    }
}
