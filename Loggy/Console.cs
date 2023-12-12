using com.HellStormGames;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Logging
{
    public class Console : INotifyPropertyChanged
    {
        public List<LogData> LogData { get; set; }  
        
        private static Console _instance = null;
        public static Console Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Console();
                    _instance.LogData = new List<LogData>();
                }
                return _instance;
            }
        }

        public static void Write(string message)
        {
            Write(message);
        }
        public static void Write(string message, string tag)
        {
            Write(message, tag);
        }
        public static void Write(string message, LogType type)
        {
            Write(message, "Common", type);
        }
        public static void Write(string message, string Tag, LogType type = LogType.INFO)
        {
            _instance.LogData.Add(new Logging.LogData(message, Tag, type));
            //_instance.Message += $"{message}\n";
        }
        public static void Clear()
        {
            _instance.LogData.Clear();
            //_instance.Message = String.Empty;
        }
        public static void Destroy()
        {
            _instance.LogData.Clear();
            _instance.LogData = null;
            _instance = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}