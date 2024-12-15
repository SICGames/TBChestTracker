using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public enum ChestProcessingState
    {
        IDLE = 0,
        PROCESSING = 1,
        COMPLETED = 2,
        NO_PROCESSOR_ATTACHED
    }
}
