using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using TBChestTracker.Managers;
using System.Diagnostics;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanChestDatabaseUpdaterWindow.xaml
    /// </summary>
    public partial class ClanChestDatabaseUpdaterWindow : Window, INotifyPropertyChanged
    {


        private string pStatus = "";
        public string Status
        {
            get => pStatus; set
            {
                pStatus = value;
                OnPropertyChanged(nameof(Status));  
            }
        }
        private double previousProgress = 0.0;

        private double pProgress = 0.0;
        public double Progress
        {
            get => pProgress;
            set
            {
                pProgress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        private bool bIsCompleted = false;

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

        private Dictionary<string, IList<ClanChestData>> tmp_clanchestdata;
        private Progress<UpgradeProgressStatus> UpgradeStatus { get; set; }

        private DateKeys DateKeys { get; set; }
        private void UpdateProgressStatus(string status, double progress, bool isCompleted)
        {
            Status = status;
            Progress = progress;
            bIsCompleted = isCompleted;
            previousProgress = Progress;
            if(bIsCompleted)
            {
                PanelVisible = Visibility.Visible;
            }
            else
            {
                PanelVisible = Visibility.Hidden;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public ClanChestDatabaseUpdaterWindow()
        {
            InitializeComponent();
            tmp_clanchestdata = new Dictionary<string, IList<ClanChestData>>();
            this.DataContext = this;
        }

        private async Task<bool> LoadDatabase(string databasefilename, IProgress<UpgradeProgressStatus> progress)
        {
            var tprogress = new UpgradeProgressStatus("Loading Older Version Of Clan Chest Database...", 0.0, false);
            progress.Report(tprogress);

            bool result = JsonHelper.TryLoad(databasefilename, out tmp_clanchestdata);
            return result;
        }

        private async Task DetectDateFormatAndConvert(IProgress<UpgradeProgressStatus> progress)
        {
            var dateparser = new DateParser();
            if(tmp_clanchestdata == null || tmp_clanchestdata.Keys == null) { return; }
            double dateCount = tmp_clanchestdata.Keys.Count;
            double currentDateCount = 0;
          
            foreach (var tmp_date in tmp_clanchestdata.Keys.ToList()) 
            {
                bool isValid = dateparser.IsDateFormatValid(tmp_date);
                if(isValid)
                {
                    if(dateparser.GetDateKeys() != null)
                    {
                        DateKeys = dateparser.GetDateKeys();

                        DateTime date = new DateTime(dateparser.Year, dateparser.Month,dateparser.Day).ToUniversalTime();
                        string forcedDateString = date.ToString(AppContext.Instance.ForcedDateFormat);
                        
                        try
                        {
                            tmp_clanchestdata.UpdateKey(tmp_date, forcedDateString);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed to convert date.");
                        }
                        double p = currentDateCount / dateCount * 100.0;
                        p = Math.Round(p, 0);
                        var tprogress = new UpgradeProgressStatus($"Converting Date Format of '{tmp_date}' to '{forcedDateString}", p, false);
                        progress.Report(tprogress);
                    }
                }

                currentDateCount += 1.0;
                await Task.Delay(100);
            }
        }
        private async Task<bool> UpgradeDatabase(IProgress<UpgradeProgressStatus> progress)
        {
            var db = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
            var dbfolder = $@"{ClanManager.Instance.CurrentProjectDirectory}{db.ClanFolderPath}";
            var dbFile = $@"{dbfolder}{db.ClanChestDatabaseFile}";
            var tprogress = new UpgradeProgressStatus($"Saving upgraded clanchest.db file...", 95, false);
            progress.Report(tprogress);
            await Task.Delay(250);
            ClanManager.Instance.ClanChestManager.Database.Version = 3;
            ClanManager.Instance.ClanChestManager.Database.ClanChestData = tmp_clanchestdata;
            ClanManager.Instance.ClanChestManager.Save();
            return true;
        }
        private async void BeginUpgrade(IProgress<UpgradeProgressStatus> progress)
        {
            var db = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
            var dbfolder = $@"{ClanManager.Instance.CurrentProjectDirectory}";
            var dbFile = $@"{dbfolder}{db.ClanChestDatabaseFile}";

            var result = await LoadDatabase(dbFile, progress);
            if (result == false)
            {
                var tprogress0 = new UpgradeProgressStatus($"Failed to load {dbFile}...", 0, false);
                progress.Report(tprogress0);
                await Task.Delay(100);
                return;
            }
            await DetectDateFormatAndConvert(progress);
            var r = await UpgradeDatabase(progress);
            if (r == false)
            {
                var tprogress1 = new UpgradeProgressStatus($"Failed to upgrade clan chest database to version 3. Make sure your system date format matches date format inside clanchest.db file.", 0, false);
                progress.Report(tprogress1);
                await Task.Delay(100);
                return;
            }
            var tprogress = new UpgradeProgressStatus($"Clan Chest Database been upgraded to newer version...", 100, true);
            progress.Report(tprogress);
            await Task.Delay(100);

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var progress = new Progress<UpgradeProgressStatus>((x) =>
            {
                UpdateProgressStatus(x.Status, x.Progress, x.IsCompleted);
            });
            BeginUpgrade(progress);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //-- patched object not referenced 
            if (tmp_clanchestdata != null)
            {
                tmp_clanchestdata.Clear();
                tmp_clanchestdata = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
