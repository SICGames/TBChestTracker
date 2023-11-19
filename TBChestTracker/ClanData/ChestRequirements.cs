using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestRequirements : INotifyPropertyChanged
    {
        private bool pUseNoChestConditions = true;
        private bool pUseChestConditions = false;
        public bool useNoChestConditions
        {
            get
            {
                return pUseNoChestConditions;
            }
            set
            {
                pUseNoChestConditions = value;
                OnPropertyChanged(nameof(useNoChestConditions));   
            }
        }
        public bool useChestConditions
        {
            get
            {
                return pUseChestConditions;
            }
            set
            {
                pUseChestConditions = value;
                OnPropertyChanged(nameof(useChestConditions));
            }
        }

        public ObservableCollection<ChestConditions> ChestConditions { get; set; }
        public ChestRequirements() 
        { 
            if(ChestConditions == null)
                ChestConditions = new ObservableCollection<ChestConditions>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
