using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class BuildingChestsProgress
    {
        public string Status {  get; set; }
        public double Total {  get; set; }
        public double Current { get; set; }
        public double Progress { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public string CurrentFile { get; set; }
        public BuildingChestsProgress(string status, double total, double current, TimeSpan timeRemaining, string currentFile)
        {
            Status = status;
            Total = total;
            Current = current;
            TimeRemaining = timeRemaining;
            CurrentFile = currentFile;
            Progress = total / current * 100.0;
        }
    }
}
