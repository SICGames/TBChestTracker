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
        private ObservableCollection<ChestPoints> _ChestPoints;
        public ObservableCollection<ChestPoints> ChestPoints 
        {
            get => _ChestPoints;
            set
            {
                _ChestPoints = value;
                OnPropertyChanged(nameof(ChestPoints));
            }
        }

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
