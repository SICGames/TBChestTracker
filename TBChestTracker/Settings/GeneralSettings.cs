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
        private string defaultClanRootFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TotalBattleChestTracker\\";
        private string _ClanRootFolder;
        public string ClanRootFolder
        {
            get
            {
                if(String.IsNullOrEmpty(_ClanRootFolder)) 
                    ClanRootFolder = defaultClanRootFolder;
                return _ClanRootFolder;
            }
            set
            {
                _ClanRootFolder = value;
                OnPropertyChanged(nameof(ClanRootFolder));
            }
        }
        private string defaultTessDataFolder = $@"{AppContext.AppFolder}TessData";
        private string _TessDataFolder;
        public string TessDataFolder
        {
            get
            {
                if(string.IsNullOrEmpty(_TessDataFolder))
                {
                    TessDataFolder = defaultTessDataFolder;
                }
                return _TessDataFolder;
            }
            set
            {
                _TessDataFolder = value;
                OnPropertyChanged(nameof(TessDataFolder));
            }
        }

        private string defaultLanguages = "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus+pol+por+pus+ukr+ger";
        private string _languages;

        public string Languages
        {
            get
            {
                if(string.IsNullOrEmpty(_languages))
                {
                    _languages = defaultLanguages;
                }
                return _languages;
            }
            set
            {
                _languages = value;
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
