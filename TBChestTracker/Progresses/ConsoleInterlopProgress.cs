using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ConsoleInterlopProgress
    {
        public string Data { get; set; }
        public double Percent { get; set; }
        public ConsoleInterlopProgress(string data, double percent)
        {
            this.Data = data;
            this.Percent = percent;
        }
    }
}
