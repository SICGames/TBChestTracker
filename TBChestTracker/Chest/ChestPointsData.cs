using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestPoints : INotifyPropertyChanged
    {

        private string _chesttype = "";
        private int _level = 5;
        private int _pointValue = 0;

        public string ChestType
        {
            get => _chesttype;
            set
            {
                _chesttype = value;
                OnPropertyChanged(nameof(ChestType));
            }
        }
        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
            }
        }
        public int PointValue
        {
            get => _pointValue;
            set
            {
                _pointValue = value;
                OnPropertyChanged(nameof(PointValue));
            }
        }

        public ChestPoints() 
        { 
        
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
