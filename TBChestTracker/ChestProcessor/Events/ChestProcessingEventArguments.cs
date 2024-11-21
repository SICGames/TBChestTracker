using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestProcessingEventArguments : EventArgs
    {
        public bool isCompleted {  get; set; }
        public bool hasError { get; set; }
        public Chest Chest { get; set; }
        public string Owner { get; set; }
        public long Timestamp { get; set; }

        public ChestProcessingEventArguments(string owner, Chest chest, long timestamp, bool isCompleted, bool hasError)
        {
            Owner = owner;
            Timestamp = timestamp;
            this.isCompleted = isCompleted;
            this.hasError = hasError;
            Chest = chest;
        }   
    }
}
