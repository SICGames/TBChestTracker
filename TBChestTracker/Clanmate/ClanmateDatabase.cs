using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ClanmatesDatabase : INotifyPropertyChanged
    {
        private int _NumClanmates = 0;

        public int NumClanmates
        {
            get => _NumClanmates;
            set
            {
                _NumClanmates = value;
                OnPropertyChanged(nameof(NumClanmates));    
            }
        }

        public int Version { get; set; }

        private ObservableCollection<Clanmate> _clanmates = null;
        public ObservableCollection<Clanmate> Clanmates
        {
            get => _clanmates;
            set
            {
                _clanmates = value;
                OnPropertyChanged(nameof(Clanmates));   
            }
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
