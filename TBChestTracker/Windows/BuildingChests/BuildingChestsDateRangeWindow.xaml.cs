using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using com.HellStormGames.Logging;
using com.HellStormGames.Diagnostics;

namespace TBChestTracker.Windows.BuildingChests
{
    /// <summary>
    /// Interaction logic for BuildingChestsDateRangeWindow.xaml
    /// </summary>
    public partial class BuildingChestsDateRangeWindow : Window
    {
        public string[] files { get; set; }
        Dictionary<string, string> dateDict = new Dictionary<string, string>();

        //public List<string> dates = new List<string>();
        public BuildingChestsDateRangeWindow()
        {
            InitializeComponent();
        }

        private string[] GetChestFiles()
        {
            var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
            var chestFolder = $"{clanfolder}\\Chests\\Data";
            var di = new DirectoryInfo(chestFolder);
            if (di.Exists)
            {
                return di.EnumerateFiles("chests_*.txt", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();    
            }
            return null;
        }
        private void BuildBlackoutDates()
        {
            files = GetChestFiles();
            if (files?.Length > 0)
            {
                foreach (var file in files)
                {
                    var pattern = @"(\d+-\d+-\d+)";
                    Regex r = new Regex(pattern, RegexOptions.Singleline);
                    var m = r.Match(file);
                    if (m.Success)
                    {
                        dateDict.Add(m.Value, file);
                    }
                }
                DateTime StartDateobj = new DateTime();
                DateTime endDate = new DateTime();

                var firstDate = dateDict.Keys.First();
                var lastDate = dateDict.Keys.Last();

                DateTime.TryParse(firstDate, CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out StartDateobj);
                DateTime.TryParse(lastDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out endDate);

                var pastDateTimeObject = StartDateobj.AddDays(-1);
                var futureDateTimeObject = endDate.AddDays(1);
                
                var beginningOfTime = StartDateobj.AddYears(-1000);
                var endingOfTime = endDate.AddYears(1000);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StartDatePicker.BlackoutDates.Add(new CalendarDateRange(beginningOfTime, pastDateTimeObject));
                    StartDatePicker.BlackoutDates.Add(new CalendarDateRange(futureDateTimeObject, endingOfTime));

                    EndDatePicker.BlackoutDates.Add(new CalendarDateRange(beginningOfTime, pastDateTimeObject));
                    EndDatePicker.BlackoutDates.Add(new CalendarDateRange(futureDateTimeObject, endingOfTime));

                    StartDatePicker.SelectedDate = StartDateobj;
                    EndDatePicker.SelectedDate = endDate;
                }));

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                BuildBlackoutDates();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dateDict.Clear();
            dateDict = null;
            files = null;
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> filesToBuild = new List<string>();

            var pickedStartDate = StartDatePicker.SelectedDate.Value;
            var pickedEndDate = EndDatePicker.SelectedDate.Value;
            var diff = pickedEndDate - pickedStartDate;
            var days = diff.TotalDays;
            var startDateStr = pickedStartDate.ToString(@"yyyy-MM-dd");
            var endDateStr = pickedEndDate.ToString(@"yyyy-MM-dd");

            if (days == 0)
            {
                var dayDate = pickedStartDate.AddDays(0);
                var dayDateStr = dayDate.ToString(@"yyyy-MM-dd");
                try
                {
                    var file = dateDict[dayDateStr];
                    if (file != null)
                    {
                        filesToBuild.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    Loggio.Error(ex, "Building Chests Date Range", "Issue occurred.");
                }
            }
            else
            {
                for (var day = 0; day <= days; day++)
                {
                    var dayDate = pickedStartDate.AddDays(day);
                    var dayDateStr = dayDate.ToString(@"yyyy-MM-dd");
                    try
                    {
                        var file = dateDict[dayDateStr];
                        if (file != null)
                        {
                            filesToBuild.Add(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            //-- we now send to BuilderWindow.

            BuildingChestsWindow buildingChestsWindow = new BuildingChestsWindow(SettingsManager.Instance.Settings.AutomationSettings);
            buildingChestsWindow.ChestFiles = filesToBuild.ToArray();

            buildingChestsWindow.Show();
            this.Close();
        }
    }
}
