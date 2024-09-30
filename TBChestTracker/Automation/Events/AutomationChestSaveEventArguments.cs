using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Automation
{
    public class AutomationChestSaveEventArguments : EventArgs
    {
        public bool isSaving {  get; set; }
        public bool isSaved { get; set; }

        public AutomationChestSaveEventArguments(bool issaving, bool issaved) 
        { 
            this.isSaving = issaving;
            this.isSaved = issaved;
        }
    }
}
