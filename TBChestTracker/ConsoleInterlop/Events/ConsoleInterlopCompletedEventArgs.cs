using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ConsoleInterlopCompletedEventArgs : EventArgs
    {
        public bool? isCompleted {  get; set; }
        public ConsoleInterlopCompletedEventArgs(bool? isCompleted)
        {
            this.isCompleted = isCompleted;
        }
    }
}
