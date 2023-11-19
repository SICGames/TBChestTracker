using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public enum ChestConditionType
    {
        COMMON = 0,
        RARE = 1,
        EPIC  = 2,
        CITADEL = 3,
        AREA  = 4,
        STORY = 5
    }
    public enum ChestConditionComparator
    {
        GREATER = 0,
        LESS = 1,
    }

    public class ChestConditions : INotifyPropertyChanged
    {
        private string _chestType = "Common";
        private string _chestComparator = "greater than";
        private int _level = 5;

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
        public string Comparator {
            get
            {
                return _chestComparator;
            }
            set
            {
                _chestComparator = value;
                OnPropertyChanged(nameof(Comparator));
            }
        }
        public int level {
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
