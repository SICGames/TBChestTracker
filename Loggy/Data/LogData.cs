using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{
    [System.Serializable]
    public class LogData
    {
        public string DateTime { get; set; }
        public LoggioEventType LogType { get; set; }
        public string Message { get; set; }
        public LogData(string datetime, LoggioEventType logtype, string message) 
        { 
            this.DateTime = datetime;
            this.LogType = logtype;
            this.Message = message;
        }
    }
}
