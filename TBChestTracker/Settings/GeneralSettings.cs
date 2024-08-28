using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class GeneralSettings : INotifyPropertyChanged
    {
        private string _ClanRootFolder;
        public string ClanRootFolder
        {
            get
            {
                return _ClanRootFolder;
            }
            set
            {
                _ClanRootFolder = value;
                OnPropertyChanged(nameof(ClanRootFolder));
            }
        }
        private string _TessDataFolder;
        public string TessDataFolder
        {
            get
            {
                return _TessDataFolder;
            }
            set
            {
                _TessDataFolder = value;
                OnPropertyChanged(nameof(TessDataFolder));
            }
        }
        
        private string _languages;

        public string Languages
        {
            get
            {
                return _languages;
            }
            set
            {
                _languages = value;
            }
        }

        private string _uiLanguage;
        public string UILanguage
        {
            get => _uiLanguage;
            set
            {
                _uiLanguage = value;
                OnPropertyChanged(nameof(UILanguage));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public GeneralSettings() 
        { 

        }

    }
}
