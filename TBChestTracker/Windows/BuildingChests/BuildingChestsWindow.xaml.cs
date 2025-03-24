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
        IChestDataBuilder builder;
        AutomationSettings AutomationSettings;

        public BuildingChestsWindow(AutomationSettings automationSettings)
        {
            InitializeComponent();
            this.DataContext = this;
            AutomationSettings = automationSettings;
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

        private String[] _ChestFiles;
        public String[] ChestFiles
        {
            get => _ChestFiles;
            set => _ChestFiles = value;
        }

        private void UpdateUI(string status, double progress)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Status = status;
                Progress = progress;

            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async Task BuildTempChests(IProgress<BuildingChestsProgress> progress)
        {
            if (ChestFiles.Count() == 0)
            {
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var chestsFolder = $"{clanfolder}\\Chests\\Temp";
                var p = new BuildingChestsProgress($"Oh no, doesn't seem to be any chests files within folder '{chestsFolder}'. So, we can not continue building chests.", -1, 0, 0, false, true);
                progress.Report(p);
                await Task.Delay(100);
                return;
            }
            await Build(progress, true);
        }

        private async Task BuildChests(IProgress<BuildingChestsProgress> progress)
        {
            if (ChestFiles.Count() == 0)
            {
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var chestsFolder = $"{clanfolder}\\Chests\\Data";
                var p = new BuildingChestsProgress($"Oh no, doesn't seem to be any chests files within folder '{chestsFolder}'. So, we can not continue building chests.", -1, 0, 0, false, true);
                progress.Report(p);
                await Task.Delay(100);
                return;
            }
            await Build(progress);
        }

        private async Task Build(IProgress<BuildingChestsProgress> progress, bool hasTempFiles = false)
        {
            var p1 = new BuildingChestsProgress("Preparing...", -1, 0, 0, false);
            progress.Report(p1);
            await Task.Delay(100);
            if (hasTempFiles)
            {
                builder = new ChestDataBuildConfiguration(ChestFiles, progress, hasTempFiles).CreateBuilder();
            }
            else
            {
                builder = new ChestDataBuildConfiguration(ChestFiles, progress, hasTempFiles).CreateBuilder();
            }
            await builder.Build();
        }

        private Task BeginBuildingChestsTask(IProgress<BuildingChestsProgress> progress)
        {
            return Task.Run(async () =>
            {
                if (AutomationSettings != null)
                {
                    if (AutomationSettings.BuildChestsAfterStoppingAutomation)
                    {
                        await BuildTempChests(progress);
                    }
                    else
                    {
                        await BuildChests(progress);
                    }
                }
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Progress<BuildingChestsProgress> progress = new Progress<BuildingChestsProgress>();
            AppContext.Instance.IsBuildingChests = true;
            //-- check to see if we need to create Data folder
            var DataFolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Data";
            var DataFolderDi = new DirectoryInfo(DataFolder);
            if(DataFolderDi.Exists == false)
            {
                DataFolderDi.Create();
            }

            progress.ProgressChanged += (s, o) =>
            {
                UpdateUI(o.Status, o.Progress);
                if(o.isFinished)
                {
                    if (SettingsManager.Instance.Settings.AutomationSettings.AutomaticallyCloseChestBuildingDialogAfterFinished)
                    {
                        this.Close();
                    }
                    else
                    {
                        PanelVisible = Visibility.Visible;
                    }
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
            
            builder?.Dispose();
            ChestFiles = null;
            AppContext.Instance.HasBuiltChests = true;
            AppContext.Instance.IsBuildingChests = false;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
