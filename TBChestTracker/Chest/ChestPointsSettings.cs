using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestPointsSettings : INotifyPropertyChanged
    {
        private bool _dontUseChestPoints = true;
        private bool _useChestPoints = false;

        public bool DontUseChestPoints
        {
            get => _dontUseChestPoints;
            set
            {
                _dontUseChestPoints = value;
                OnPropertyChanged(nameof(DontUseChestPoints));  
            }
        }
        public bool UseChestPoints
        {
            get { return _useChestPoints; }
            set
            {
                _useChestPoints = value;
                OnPropertyChanged(nameof(UseChestPoints));
            }
        }

        public ObservableCollection<ChestPoints> ChestPoints { get; set; }

        public ChestPointsSettings() 
        { 
            if(ChestPoints == null)
            {
                ChestPoints = new ObservableCollection<ChestPoints>();
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
