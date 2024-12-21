using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class UpgradeProgressStatus
    {
        public string Status { get; set; }
        public double Progress { get; set; }
        public bool IsCompleted { get; set; }

        public UpgradeProgressStatus(string status, double progress, bool isComplete) 
        { 
            Status = status;
            Progress = progress;
            IsCompleted = isComplete;
        }
    }
}
