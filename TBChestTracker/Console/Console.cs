using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Logging
{
    public class Console : INotifyPropertyChanged
    {
        private string _message = String.Empty;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message)); 
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private static Console _instance = null;
        public static Console Instance
        {
            get
            {
                if(_instance == null )
                    _instance = new Console();
                return _instance;
            }
        }

        public static void Write(string message)
        {
            _instance.Message += $"{message}\n";
        }
        public static void Clear()
        {
            _instance.Message = String.Empty;
        }
        public static void Destroy()
        {
            _instance.Message = String.Empty;
            _instance = null;
        }
    }
}
