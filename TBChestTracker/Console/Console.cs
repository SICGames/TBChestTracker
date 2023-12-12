using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace com.HellStormGames.Logging
{
    public class Console : INotifyPropertyChanged
    {
        private ObservableCollection<LogData> _logData = null;
        public ObservableCollection<LogData> LogData
        {
            get
            {
                return _logData;
            }
            set
            {
                _logData = value;
                onPropertyChanged(nameof(LogData));
            }
        }
        
        private static Console _instance = null;
        public static Console Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Console();
                    if(_instance.LogData == null) 
                        _instance.LogData = new ObservableCollection<LogData>();    
                }
                return _instance;
            }
        }

        public static void Write(string message)
        {
            Write(message, "Common", LogType.INFO);
        }
        public static void Write(string message, string tag)
        {
            Write(message, tag, LogType.INFO);
        }
        public static void Write(string message, LogType type)
        {
            Write(message, "Common", type);
        }
        public static void Write(string message, string Tag, LogType type = LogType.INFO)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _instance.LogData.Add(new Logging.LogData(message, Tag, type));
            }));
        }
        public static void Clear()
        {
            _instance.LogData.Clear();
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