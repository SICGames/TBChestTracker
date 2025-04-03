using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [Serializable()]
    public class CrashException
    {
        public string ExceptionType { get; set; }
        public string Message {  get; set; }
        public string StackTrace { get; set; }
    }
}
