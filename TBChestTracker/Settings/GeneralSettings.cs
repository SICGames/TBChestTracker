using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
                return String.IsNullOrEmpty(_ClanRootFolder) == true ? 
                    $@"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\SICGames\TotalBattleChestTracker\"
                    : _ClanRootFolder;
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
        private int? _monitorindex;
        public int MonitorIndex
        {
            get => _monitorindex.GetValueOrDefault(-1);
            set
            {
                _monitorindex = value;
                OnPropertyChanged(nameof(MonitorIndex));
            }
        }

        private bool? _showOcrLanguageSelection;
        public bool ShowOcrLanguageSelection
        {
            get => _showOcrLanguageSelection.GetValueOrDefault(true);
            set
            {
                _showOcrLanguageSelection = value;
                OnPropertyChanged(nameof(ShowOcrLanguageSelection));
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
