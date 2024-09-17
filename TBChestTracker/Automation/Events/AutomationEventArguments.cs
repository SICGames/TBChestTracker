using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationEventArguments : EventArgs
    {
        public bool isRunning {  get; set; }    
        public bool isCancelled { get; set; }

        public AutomationEventArguments(bool isrunning, bool iscanceled) 
        { 
            this.isRunning = isrunning; 
            this.isCancelled = iscanceled;
        }
    }
}
