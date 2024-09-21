using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ViewModels
{
    public class ClanDatabaseBackupItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private string _date = String.Empty;
        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }
        private string _time = String.Empty;
        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }
        private string _file = String.Empty;
        public string File
        {
            get => _file;
            set
            {
                _file = value;
            }
        }
        public ClanDatabaseBackupItem(string date, string time, string file)
        {
            Date = date;
            Time = time;
            File = file;
        }   
    }
}
