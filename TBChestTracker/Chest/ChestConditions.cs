using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestConditions : INotifyPropertyChanged
    {
        private string _chestType;
        private string _chestName;

        private string _level = String.Empty;
      
        public string ChestName
        {
            get
            {
                return _chestName;
            }
            set
            {
                _chestName = value;
                OnPropertyChanged(nameof(ChestName));
            }
        }
        public string ChestType
        {
            get 
            {
                return _chestType;
            }
            set
            {
                _chestType = value;
                OnPropertyChanged(nameof(ChestType));   
            }
        }

        public string level {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                OnPropertyChanged(nameof(level));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
