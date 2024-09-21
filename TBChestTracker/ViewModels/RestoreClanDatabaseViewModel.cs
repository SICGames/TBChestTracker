using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ViewModels
{
    public class RestoreClanDatabaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private ObservableCollection<ClanDatabaseBackupItem> backupItems = new ObservableCollection<ClanDatabaseBackupItem>();
        public ObservableCollection<ClanDatabaseBackupItem> BackupItems 
        {
            get => backupItems;
            set
            {
                backupItems = value;
                OnPropertyChanged(nameof(BackupItems));
            }
        }
        public void Add(string date, string time, string file)
        {
            backupItems.Add(new ClanDatabaseBackupItem(date, time, file));  
        }

        public void Dispose()
        {
            BackupItems.Clear();
            BackupItems = null;
        }
    }
}
