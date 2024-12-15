using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for BuildingChestsWindow.xaml
    /// </summary>
    public partial class BuildingChestsWindow : Window, INotifyPropertyChanged
    {

        public BuildingChestsWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;    
                OnPropertyChanged(nameof(Status));
            }
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
    
            Progress<BuildingChestsProgress> progress = new Progress<BuildingChestsProgress>();
            progress.ProgressChanged += (s,o) => 
            {
                Status = o.Status;
                Progress = o.Progress;
            };
            var clanfolder = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";
            var cacheFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\\cache";
            DirectoryInfo di = new DirectoryInfo(cacheFolder);
            if (di.Exists)
            {
                var files = di.GetFiles("*.txt");
                var filepaths = files.Select(f => f.FullName).ToArray();
                ClanManager.Instance.ClanChestManager.BuildChests(filepaths, progress);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
