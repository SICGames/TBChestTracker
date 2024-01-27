using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanRequirements : INotifyPropertyChanged
    {
        private bool bUseNoClanRequirements = true;
        private bool bUseSpecifiedClanRequirements = false;

        public bool UseNoClanRequirements
        {
            get => bUseNoClanRequirements;
            set
            {
                bUseNoClanRequirements = value;
                OnPropertyChanged(nameof(UseNoClanRequirements));
            }
        }
        public bool UseSpecifiedClanRequirements
        {
            get
            {
                return bUseSpecifiedClanRequirements;
            }
            set
            {
                bUseSpecifiedClanRequirements = value;
                OnPropertyChanged(nameof(UseSpecifiedClanRequirements));
            }
        }

        public ObservableCollection<ClanSpecifiedRequirements> ClanSpecifiedRequirements { get; set; }
        #region OnPropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ClanRequirements() 
        { 
            if(ClanSpecifiedRequirements == null) 
                ClanSpecifiedRequirements = new ObservableCollection<ClanSpecifiedRequirements>();  
        }

    }
}
