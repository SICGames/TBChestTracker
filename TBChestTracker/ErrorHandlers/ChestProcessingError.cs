using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestProcessingError
    {
        public string Message { get; set; }
        public ChestProcessingError(string message)
        {
            Message = message;
        }
    }
}
