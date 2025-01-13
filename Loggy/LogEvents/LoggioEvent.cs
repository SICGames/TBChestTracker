using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public class LoggioEvent
    {
        public string Message { get; }
        public DateTimeOffset DateTimeOffset { get; }
        public string Tag { get; }
        public LoggioEventType LogType { get; }
        public Exception Exception { get; }

        public LoggioEvent(DateTimeOffset datetimeoffset, LoggioEventType logtype, string tag, string message, Exception exception) 
        { 
            this.DateTimeOffset = datetimeoffset;
            this.Message = message;
            this.Tag = tag;
            this.LogType = logtype;
            this.Tag = tag;
            this.Exception = exception;
        }
    }
}
