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
        public bool isFinished { get; set; }

        public BuildingChestsProgress(string status, double progress, double total = 0, double current = 0, bool isFinished = false)
        {
            Status = status;
            Total = total;
            Current = current;
            Progress = progress == -1 ? current / total * 100.0 : progress;
            this.isFinished = isFinished;
        }
    }
}
