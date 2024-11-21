using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestProcessingStateChangedEventArguments : EventArgs
    {
        public ChestProcessingState OldState { get; set; }
        public ChestProcessingState NewState { get; set; }

        public ChestProcessingStateChangedEventArguments(ChestProcessingState oldState, ChestProcessingState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
