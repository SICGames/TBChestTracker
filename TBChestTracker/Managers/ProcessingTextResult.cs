using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public enum ProcessingStatus
    {
        OK = 0,
        INDEX_OUT_OF_RANGE = 1,
        TEMP_CHEST_NULL = 2,
        UNKNOWN_ERROR = 4,
        CLANMATE_ERROR = 5
    }

    [System.Serializable]
    public class ProcessingTextResult
    {
        public ProcessingStatus Status {  get; set; }
        public string Message { get; set; }
        public List<ChestData> ChestData { get; set; }
    }
}
