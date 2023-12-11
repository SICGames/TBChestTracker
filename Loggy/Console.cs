using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Logging
{
    public class Console : INotifyPropertyChanged
    {
        private string _message = String.Empty;
        public string Message
        {
            get => this._message; 
            set
            {
                this._message = value;
                onPropertyChanged(nameof(Message));
            }
        }

        private static Console _instance = null;
        public static Console Instance
        {
            get
            {
                if( _instance == null)
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