using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [Serializable]
    public enum ClanQuota
    {
        DAILY,
        WEEKLY
    } 
    [Serializable]
    public class ClanDatabase : INotifyPropertyChanged
    {
        public string Clanname { get; set; }
        public string ClanDatabaseFolder { get; set; }
        public string DefaultClanFolderPath
        {
            get
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TotalBattleChestTracker\\";
            }
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
        public string ClanChestDatabaseBackupFile
        {
            get
            {
                return $"{ClanDatabaseBackupFolderPath}\\clanchests_backup.db";
            }
        }
        public string ClanChestDatabaseExportFolderPath { get; set; }
        public string ClanChestReportFolderPath { get; set; }
        public string ClanDatabaseBackupFolderPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
