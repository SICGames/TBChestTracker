using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
   
    [Serializable]
    public class ClanDatabase : INotifyPropertyChanged
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
        
        public string ClanmateDatabaseFile 
        {
            get
            {
                return $"{ClanDatabaseFolder}\\clanmates.db";
            }
        }

        public string ClanChestDatabaseFile 
        {
            get
            {
                return $"{ClanDatabaseFolder}\\clanchests.db";
            }
        }
        public string ClanChestRequirementsFile
        {
            get
            {
                return $"{ClanDatabaseFolder}\\chestrequirements.db";
            }
        }
        public string ClanSettingsFile
        {
            get => $"{ClanDatabaseFolder}\\clansettings.db";
        }
        public string ClanChestDatabaseBackupFile
        {
            get
            {
                return $"{ClanDatabaseBackupFolderPath}\\clanchests_backup.db";
            }
        }
        public string ClanChestDatabaseExportFolderPath { get; set; }
        public string ClanDatabaseBackupFolderPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
