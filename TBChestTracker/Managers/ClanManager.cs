using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace TBChestTracker.Managers
{
    public class ClanManager
    {
        public static ClanManager Instance {get; private set;}
        
        private ClanmateManager _clanmatemanager = null;
        public ClanmateManager ClanmateManager => _clanmatemanager;
        private ClanDatabaseManager _databasemanager = null;
        public ClanDatabaseManager ClanDatabaseManager => _databasemanager;
        private ClanChestManager _chestmanager = null;
        public ClanChestManager ClanChestManager => _chestmanager;
        private ClanChestSettings _clansettings = null;
        public ClanChestSettings ClanChestSettings => _clansettings;

        private string currentProjectDirectory = String.Empty;
        public string CurrentProjectDirectory
        {
            get => currentProjectDirectory;
            private set => currentProjectDirectory = value;
        }
        public void SetProjectDirectory(string path)
        {
            CurrentProjectDirectory = path;
        }


        public ClanManager()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            if (_clanmatemanager == null)
                _clanmatemanager = new ClanmateManager();
            if(_databasemanager == null) 
                _databasemanager = new ClanDatabaseManager();
            if(_chestmanager == null)
                _chestmanager= new ClanChestManager();
            if(_clansettings == null)
                _clansettings= new ClanChestSettings();
        }
        public void Destroy()
        {
            _clansettings.Clear();
            _chestmanager.Dispose();
            //_chestmanager.ClearData();
            _databasemanager = null;
            _clanmatemanager.Database.Clanmates.Clear();
            _clanmatemanager.Database.Clanmates = null;
            _clanmatemanager = null;
            _databasemanager = null;
        }
    }
}
