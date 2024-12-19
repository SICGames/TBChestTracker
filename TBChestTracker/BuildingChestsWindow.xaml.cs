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
        private Visibility pVisibility = Visibility.Hidden;
        public Visibility PanelVisible
        {
            get
            {
                return pVisibility;
            }
            set
            {
                pVisibility = value;
                OnPropertyChanged(nameof(PanelVisible));
            }
        }
        private void UpdateUI(string status, double progress)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Status = status;
                Progress = progress;
                
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private Task BeginBuildingChestsTask(IProgress<BuildingChestsProgress> progress)
        {
            return Task.Run(async() =>
            {
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var cacheFolder = $"{clanfolder}\\cache";
                DirectoryInfo di = new DirectoryInfo(cacheFolder);

                if (di.Exists)
                {
                    var files = di.GetFiles("*.txt");
                    var filepaths = files.Select(f => f.FullName).ToArray();

                    var p = new BuildingChestsProgress("Preparing to build clan chests data...", 0, 0, false);
                    progress.Report(p);

                    await ClanManager.Instance.ClanChestManager.BuildChests(filepaths, progress);
                    
                }
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Progress<BuildingChestsProgress> progress = new Progress<BuildingChestsProgress>();
            progress.ProgressChanged += (s, o) =>
            {
                UpdateUI(o.Status, o.Progress);
                if(o.isFinished)
                {
                    if(SettingsManager.Instance.Settings.AutomationSettings.AutomaticallyCloseChestBuildingDialogAfterFinished)
                    {
                        this.DialogResult = true;
                        this.Close();
                    }
                    
                    PanelVisible = Visibility.Visible;
                }
                else
                {
                    PanelVisible = Visibility.Hidden;
                }
            };
            BeginBuildingChestsTask(progress);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
