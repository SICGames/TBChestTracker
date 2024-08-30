using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class GeneralClanSettings :INotifyPropertyChanged
    {
        private ChestOptions _ChestOptions;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ChestOptions ChestOptions
        {
            get { return _ChestOptions; }
            set
            {
                _ChestOptions = value;
                OnPropertyChanged(nameof(ChestOptions));
            }
        }
    }
}
