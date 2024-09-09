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

            if (chestsettings.GeneralClanSettings.ChestOptions != ChestOptions.UsePoints)
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
                            //-- fix Runic or anything that has source: Lvl in it.
                            var name = chest.Name;
                            var source = chest.Source;
                            if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                            {
                                var levelStartPos = 0;
                                levelStartPos = source.IndexOf(TBChestTracker.Resources.Strings.lvl);

                                int level = 0;
                                //-- level in en-US is 0 position.
                                //-- level in es-ES is 11 position.
                                //-- crypt in en-US is 9 position 
                                //-- crypt in es-ES is 0 position.

                                //-- we can check direction of level position.
                                //-- if more than 1 then we know we should go backwares. If 0 then we know to go forwards.
                                var levelFullLength = 0;
                                var ChestType = String.Empty;

                                if (levelStartPos > -1)
                                {
                                    var levelStr = source.Substring(levelStartPos).ToLower();
                                    var levelNumberStr = levelStr.Substring(levelStr.IndexOf(" ") + 1);

                                    //-- using a quantifer to check if there is an additional space after the level number. If user is spanish, no space after level number.
                                    levelNumberStr = levelNumberStr.IndexOf(" ") > 0 ? levelNumberStr.Substring(0, levelNumberStr.IndexOf(" ")) : levelNumberStr;
                                    var levelFullStr = String.Empty;
                                    if (source.Contains(TBChestTracker.Resources.Strings.Level))
                                    {
                                        levelFullStr = $"{TBChestTracker.Resources.Strings.Level} {levelNumberStr}";
                                    }
                                    else if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                                    {
                                        levelFullStr = $"{TBChestTracker.Resources.Strings.lvl} {levelNumberStr}";
                                    }

                                    levelFullLength = levelFullStr.Length; //-- 'level|nivel 10' should equal to 8 characters in length.

                                    var levelArray = levelNumberStr.Split('-');

                                    if (levelArray.Count() == 1)
                                    {
                                        if (Int32.TryParse(levelArray[0], out level) == false)
                                        {
                                            //-- couldn't extract level.
                                        }
                                    }
                                    else if (levelArray.Count() > 1)
                                    {
                                        if (Int32.TryParse(levelArray[0], out level) == false)
                                        {
                                            //-- couldn't extract level.

                                        }
                                    }
                                    if (level == 0)
                                    {
                                        level = 5;
                                    }

                                    //-- now we make sure levelStartPos == 0 or more than 0.
                                    var direction = levelStartPos == 0 ? "forwards" : "backwards";
                                    if (direction == "forwards")
                                    {
                                        ChestType = source.Substring(levelFullLength + 1);
                                    }
                                    else
                                    {
                                        //-- Cripta de nivel 10 = 18 characters long.
                                        //-- Cripta de = 10 characters long
                                        //-- nivel 10 =  8 characters long.

                                        ChestType = source.Substring(0, source.Length - levelFullLength);
                                        ChestType = ChestType.Trim(); //-- remove any whitespaces at the end 
                                    }
                                    if (ChestType.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
                                    {
                                        ChestType = ChestType.Insert(0, $"{TBChestTracker.Resources.Strings.Common} ");
                                    }

                                    chest.Source = ChestType;
                                    chest.Level = level;

                                }
                                chestErrors++;
                            }
                            if (chestsettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                            {


                                foreach (var chestpointvalue in chestpointsvalues)
                                {
                                    var chestname = chest.Name;
                                    var chesttype = chest.Type;

                                    if (chesttype.ToLower().Contains(chestpointvalue.ChestType.ToLower()))
                                    {
                                        if (chestpointvalue.ChestName.Equals("(Any)"))
                                        {
                                            if (!chestpointvalue.Level.Equals("(Any)"))
                                            {
                                                var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                if (chest.Level == chestlevel)
                                                {
                                                    total_chest_points += chestpointvalue.PointValue;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                total_chest_points += chestpointvalue.PointValue;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (chestname.ToLower().Contains(chestpointvalue.ChestName.ToLower()))
                                            {
                                                if (!chestpointvalue.Level.Equals("(Any)"))
                                                {
                                                    var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                    if (chest.Level == chestlevel)
                                                    {
                                                        total_chest_points += chestpointvalue.PointValue;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    total_chest_points += chestpointvalue.PointValue;
                                                    break;
                                                }
                                            }
                                        }
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
