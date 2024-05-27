using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ValidateChestDataWindow.xaml
    /// </summary>
    /// 

    public partial class ValidateChestDataWindow : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource _cancellationTokenSource;
        private string _statusMessage = "Please wait while validating chest data...";

        public event PropertyChangedEventHandler PropertyChanged;

        private int chestErrors = 0;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public ValidateChestDataWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private bool ValidateChestDataChestTypes(ref Dictionary<string, List<ClanChestData>> chestdata)
        {
            foreach (var dates in chestdata.Keys)
            {
                var data = chestdata[dates];
                foreach (var _data in data)
                {
                    var chests = _data.chests;
                    if(chests != null)
                    {
                        foreach(var chest in chests)
                        {
                            var chesttype = chest.Type;
                            var chestSource = chest.Source;
                            var chestname = chest.Name;

                            if (chestSource.ToLower().Contains("jormungandr"))
                            {
                                if (chesttype == ChestType.OTHER)
                                {
                                    chest.Type = ChestType.JORMUNGANDR;
                                    chestErrors++;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }


        //--- should be a task
        private void BeginValidatingChestData(CancellationToken token)
        {
            //--- get chest data 
            //--- itlerate through chests and correct any mistakes.
            //--- if ChestType.OTHER is not suppose to be ChestType.OTHER, needs to be corrected.
            //--- check if Use Chest Point System is used
            //--- If not, nothing to do
            //--- if so, check to see if chests add up to value obtained 
            //--- if not, replace point value
            //--- alert or change status to completed and close
            var chestsettings = ClanManager.Instance.ClanChestSettings;

            var chestdata = ClanManager.Instance.ClanChestManager.ClanChestDailyData;

            if (!chestsettings.ChestPointsSettings.UseChestPoints)
            {
                EndValidatingChestData("There's nothing to do.", true, false);
                return;
            }
            if (token.IsCancellationRequested)
            {
                return;
            }

            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;

            //--- Validate ChestType Data.
            StatusMessage = "Validating Chest data types...";
            var chestTypeRepaired = ValidateChestDataChestTypes(ref chestdata);
            var numClanmates = ClanManager.Instance.ClanmateManager.Database.NumClanmates;
            StatusMessage = "Validating other chest data...";

            foreach (var dates in chestdata.Keys)
            {
                var data = chestdata[dates];

                /*
                 -- to implement in future. 
                 -- Goal is to remove duplicates from clan chest data. 
                var cleaned_data = data.Distinct(new ClanChestComparer()).ToList();
                if (data.Count() != cleaned_data.Count())
                {
                    chestErrors++;
                    data = cleaned_data;
                }
                */
                foreach (var _data in data)
                {
                    var clanmate_points = _data.Points;
                    var chests = _data.chests;

                    var total_chest_points = 0;

                    if (chests != null)
                    {
                        foreach (var chest in chests)
                        {
                            foreach (var chestpointvalue in chestpointsvalues)
                            {
                                var chestname = chest.Name;
                                if (chestpointvalue.ChestType.ToLower() == "custom")
                                {
                                    if (chestname.ToLower().Equals(chestpointvalue.ChestName.ToLower()))
                                    {
                                        total_chest_points += chestpointvalue.PointValue;
                                        break;
                                    }
                                }
                                if (chest.Type.ToString().ToLower() == chestpointvalue.ChestType.ToLower())
                                {
                                    if (chest.Level == chestpointvalue.Level)
                                    {
                                        total_chest_points += chestpointvalue.PointValue;
                                        break;
                                    }
                                }
                            }
                        }
                        if (clanmate_points != total_chest_points)
                        {
                            _data.Points = total_chest_points;
                            chestErrors++;
                        }
                    }
                }
            }

            //--- now we fix duplicates inside clanchests.db file
            if (chestErrors > 0)
            {
                EndValidatingChestData($"Chest Data has been successfully fixed.", true, true);
                return;
            }
            else
            {
                EndValidatingChestData($"Chest Data looks fine.", true, false);
            }
        }


        private void EndValidatingChestData(string message, bool isCompleted, bool isRepaired)
        {
            //--- we wrap things up by placing checkmark icon and hide progress bar
            StatusMessage = message;
            if (isRepaired)
            {
                ClanManager.Instance.ClanChestManager.CreateBackup();
                ClanManager.Instance.ClanChestManager.SaveData();
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task checkIntegrityTask = Task.Run(()=> BeginValidatingChestData(_cancellationTokenSource.Token));
            checkIntegrityTask.Wait();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
