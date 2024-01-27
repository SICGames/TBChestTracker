using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    
    public class ClanSpecifiedRequirements : INotifyPropertyChanged
    {
        private string pChestType = "Common";
        private int pChestLevel = 5;
        private int pAmountPerDay = 1;
        private string pChestOperator = "AND";
        public string ChestType
        {
            get
            {
                return this.pChestType;
            }
            set
            {
                this.pChestType = value;
                OnPropertyChanged(nameof(ChestType));
            }
        }
        public int ChestLevel
        {
            get
            {
                return this.pChestLevel;
            }
            set
            {
                this.pChestLevel = value;
                OnPropertyChanged(nameof(ChestLevel));
            }
        }
        public int AmountPerDay
        {
            get
            {
                return this.pAmountPerDay;
            }
            set
            {
                this.pAmountPerDay = value;
                OnPropertyChanged(nameof(AmountPerDay));
            }
        }
        public string ChestOperator
        {
            get
            {
                return this.pChestOperator;
            }
            set
            {
                this.pChestOperator = value;
                OnPropertyChanged(nameof(ChestOperator));
            }
        }
        public ClanSpecifiedRequirements() 
        { 
        }


        #region OnPropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
