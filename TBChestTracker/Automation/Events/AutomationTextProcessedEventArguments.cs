using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Helpers;

namespace TBChestTracker.Automation
{
    public class AutomationTextProcessedEventArguments : EventArgs
    {
        public TessResult TessResult { get; set; }
        public AutomationTextProcessedEventArguments(TessResult tessResult)
        {
            TessResult = tessResult;
        }
    }
}
