using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
   
    [Serializable]
    public class ClanDatabase : INotifyPropertyChanged, IDisposable
    {
        private int? _version;
        public int Version { get; set; }

        public string ClanAbbreviations { get; set; }
        public string Clanname { get; set; }

        private string? _ClanDatabaseFolder;
        public string ClanDatabaseFolder
        {
            get => $"\\db";
        }

        public string ClanFolderPath { get; set; }
        private string _ClanChestRequirementsFile => $"{ClanDatabaseFolder}\\chestrequirements.db";
        public string ClanChestRequirementsFile
        {
            get
            {
                return _ClanChestRequirementsFile;
            }
        }
        private string _ClanSettingsFile => $"{ClanDatabaseFolder}\\clansettings.db";
        public string ClanSettingsFile
        {
            get => _ClanSettingsFile;
        }

        public string ClanChestDatabaseExportFolderPath { get; set; }
        public string ClanDatabaseBackupFolderPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
          Version = 0;
            ClanAbbreviations = String.Empty;
            Clanname = String.Empty;
            _ClanDatabaseFolder = String.Empty;
            ClanFolderPath = String.Empty;
            ClanDatabaseBackupFolderPath = String.Empty;
        }
    }
}
