using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    /*
      New as of 11/20/2024
    */
    public class ChestProcessor : IDisposable
    {
        private ChestProcessingState pPreviousChestProcessingState = ChestProcessingState.IDLE;
        private ChestProcessingState pChestProcessingState = ChestProcessingState.IDLE;
        public ChestProcessingState ChestProcessingState 
        { 
            get 
            { 
                return pChestProcessingState; 
            } 
        }

        public EventHandler<ChestProcessingStateChangedEventArguments> ChestProcessingStateChanged;
        public EventHandler<ChestProcessingEventArguments> ChestProcessed;


        public void ChangeProcessingState(ChestProcessingState state)
        {
            pChestProcessingState = state;
        }

        public void Dispose()
        {

        }
    }
}
