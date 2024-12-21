using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ConsoleInterlopEventArgs : EventArgs
    {
        public string? Data { get; set; }
        public bool? isCompleted { get; set; }
        public ConsoleInterlopEventArgs(string? data, bool? completed)
        {
            this.Data = data;
            this.isCompleted = completed;
        }
    }
}
