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
        private bool? _bIsFirstRun;
        public bool IsFirstRun
        {
            get => _bIsFirstRun.GetValueOrDefault(true);
            set => _bIsFirstRun = value;
        }
        private string? _ClanRootFolder;
        public string ClanRootFolder
        {
            get
            {
                return String.IsNullOrEmpty(_ClanRootFolder) == true ? $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TotalBattleChestTracker\\" : _ClanRootFolder;
            }
            set
            {
                _ClanRootFolder = value;
                OnPropertyChanged(nameof(ClanRootFolder));
            }
        }
       
        private string? _uiLanguage;
        public string UILanguage
        {
            get => String.IsNullOrEmpty(_uiLanguage) == true ? "English" : _uiLanguage;
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
