using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ConsoleInterlopErrorEventArgs : EventArgs
    {
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
        public ConsoleInterlopErrorEventArgs(string errorMessage, int? errorCode)
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }   
    }
}
