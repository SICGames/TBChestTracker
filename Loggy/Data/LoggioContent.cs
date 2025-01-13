using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace com.HellStormGames.Diagnostics.Data
{
    /// <summary>
    /// LoggioContent is meant to be displayed friendly inside a ListView container.
    /// Not meant to be written to file nor console.
    /// </summary>
    public class LoggioContent : INotifyPropertyChanged
    {
        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private LoggioEventType logtype = LoggioEventType.INFO;
        public LoggioEventType LogType
        {
            get => LogType;
            set
            {
                switch(logtype)
                {
                    case LoggioEventType.INFO:
                        Icon = "infoIcon.png";
                        break;
                    case LoggioEventType.WARNING: 
                        Icon = "warningIcon.png";
                        break;
                    case LoggioEventType.ERROR:
                        Icon = "errorIcon.png";
                        break;
                }
            }
        }
        public string Date { get; private set; }
        public string Time { get; private set; }
        public string Icon { get; private set; }
        public string Tag { get; private set; }
        public string Message { get; private set; }
        public string DetailedMessage { get; private set; }
    }
}
