using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ViewModels
{
    public class ClanmatesRepairProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public ClanmatesRepairProgress() { }
        public ClanmatesRepairProgress(string msg, double progress)
        {
            Message = msg;
            Progress = progress;
            ProgressStr = $"{Progress}%";
        }

        private string _Message = String.Empty;
        public string Message
        {
            get => _Message;
            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private string _progressStr = String.Empty;
        public string ProgressStr
        {
            get => _progressStr;
            set
            {
                _progressStr = value;
                OnPropertyChanged(nameof(ProgressStr));
            }
        }
    }
}
