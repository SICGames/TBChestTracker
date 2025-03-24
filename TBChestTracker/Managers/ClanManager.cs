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
        private ClanDatabaseManager _databasemanager = null;
        public ClanDatabaseManager ClanDatabaseManager => _databasemanager;
        private ClanChestManager _chestmanager = null;
        public ClanChestManager ClanChestManager => _chestmanager;
        private ClanChestSettings _clanchestsettings = null;
        public ClanChestSettings ClanChestSettings => _clanchestsettings;
        public ClanSettings ClanSettings;
        public ChestDataManager ChestDataManager;

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
        public void UpdateOcrProfile(string profilename, AOIRect value)
        {
            ClanSettings?.OcrProfileManager?.UpdateOcrProfile(profilename, value);
        }
        public string GetCurrentOcrProfileName()
        {
            return ClanSettings?.OcrProfileManager?.CurrentOcrProfileName;

        }
        public AOIRect GetCurrentOcrProfile()
        {
            return ClanSettings?.OcrProfileManager?.GetCurrentOcrProfile();
        }
        public ClanManager()
        {
            Instance ??= this;

            _databasemanager ??= new ClanDatabaseManager();
            _chestmanager ??= new ClanChestManager();
            _clanchestsettings ??= new ClanChestSettings();
            ClanSettings ??= new ClanSettings();
            
        }
        public void AddOcrProfile(string profilename, AOIRect roi)
        {
            ClanSettings?.OcrProfileManager?.AddProfile(profilename, roi);
        }
        public void RemoveOcrProfile(string profilename)
        {
            ClanSettings?.OcrProfileManager?.RemoveProfile(profilename);
        }

        public void SetCurrentOcrProfile(string profilename)
        {
            ClanSettings?.OcrProfileManager?.SetCurrentOcrProfile(profilename);
        }

        public AOIRect GetOcrProfile(string  profilename)
        {
            return ClanSettings?.OcrProfileManager?.GetProfile(profilename);
        }

        public void UnloadClan()
        {
            ChestDataManager?.Dispose();
            CurrentProjectDirectory = String.Empty;
            ClanSettings = null;
            _clanchestsettings?.Clear();
            _clanchestsettings = null;
            _chestmanager?.Dispose();
            _chestmanager = null;
            _databasemanager?.Dispose();
            _databasemanager = null;
            Instance = null;
        }
        public void Destroy()
        {
            UnloadClan();
        }
    }
}
