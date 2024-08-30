using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Logging
{
    public class LogData
    {
        private LogType _logType = LogType.INFO;
        private string _logTypeIcon = string.Empty;
        public LogType logType
        {
            get => _logType;
            set
            {
                switch (_logType)
                {
                    case LogType.INFO:
                        logTypeIcon = "Images/infoIcon.png";
                        break;
                    case LogType.WARNING:
                        logTypeIcon = "Images/warningIcon.png";
                        break;
                    case LogType.ERROR:
                        logTypeIcon = "Images/errorIcon.png";
                        break;
                }
                _logType = value;
            }
        }
        public String logTypeIcon
        {
            get => _logTypeIcon;
            set => _logTypeIcon = value;
        }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Tag { get; set; } 
        public string Message { get; set; }

        public LogData(string message, string tag, LogType type)
        {
            this.logType = logType;
            this.Tag = tag;
            Date = DateTime.Now.ToString("d");
            Time = DateTime.Now.ToString("t");
            this.Message = message;
        }
    }
}