﻿using System;
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
    public partial class ValidateChestDataWindow : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource _cancellationTokenSource;
        private string _statusMessage = "Please wait while validating chest data...";

        public event PropertyChangedEventHandler PropertyChanged;
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

        //--- should be a task
        private void BeginValidatingChestData(CancellationToken token)
        {
            //--- get chest data 
            //--- check if Use Chest Point System is used
            //--- If not, nothing to do
            //--- if so, check to see if chests add up to value obtained 
            //--- if not, replace point value
            //--- alert or change status to completed and close
            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var isRepairCompleted = false;
            var chestpointErrors = 0;

            if (!chestsettings.ChestPointsSettings.UseChestPoints)
            {
                EndValidatingChestData("There's nothing to do.",true, false);
                return;
            }

            var chestdata = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
            if(token.IsCancellationRequested)
            {
                return;
            }

            foreach (var dates in chestdata.Keys)
            {
                
                var data = chestdata[dates];
                foreach(var _data in data)
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
                            chestpointErrors++;
                        }
                    }
                }
            }
            isRepairCompleted = true;
            if(chestpointErrors > 0)
            {
                EndValidatingChestData($"Chest Data Points has been successfully fixed.", true, true);
                return;
            }
            else
            {
                EndValidatingChestData($"Chest Data Points looks fine.", true, false);
            }
        }
        private void EndValidatingChestData(string message, bool isCompleted, bool isRepaired)
        {
            //--- we wrap things up by placing checkmark icon and hide progress bar
            StatusMessage = message;
            if(isRepaired)
                ClanManager.Instance.ClanChestManager.SaveData();

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
