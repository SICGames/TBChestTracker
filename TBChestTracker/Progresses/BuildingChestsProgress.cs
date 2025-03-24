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
        private double? _Total;
        public double Total
        {
            get => _Total.GetValueOrDefault(0.0);
            set => _Total = value;
        }

        public double Current { get; set; }
        private double? _Progress;
        public double Progress
        {
            get => _Progress.GetValueOrDefault(0.0);
            set => _Progress = value;
        }

        public bool isFinished { get; set; }
        public bool hasFailed { get; set; }
        public BuildingChestsProgress(string status, double progress, double total = 0, double current = 0, bool isFinished = false, bool hasFailed = false)
        {
            Status = status;
            Total = total;
            Current = current;
            if (Double.IsNaN(progress) == false)
            {
                Progress = progress == -1 ? current / total * 100.0 : progress;
            }

            this.isFinished = isFinished;
            this.hasFailed = hasFailed;
        }
    }
}
