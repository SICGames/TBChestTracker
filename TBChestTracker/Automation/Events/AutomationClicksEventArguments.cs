using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationClicksEventArguments : EventArgs
    {
        public bool ProcessedClicks {  get; set; }
        public int CurrentClick {  get; set; }
        public int MaxClicks { get; set; }
        public AutomationClicksEventArguments(bool processedClicks, int currentClick, int maxClicks)
        {
            ProcessedClicks = processedClicks;
            CurrentClick = currentClick;
            MaxClicks = maxClicks;
            
        }
    }
}
