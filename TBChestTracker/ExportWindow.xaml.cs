using System;
using System.Collections.Generic;
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
using CsvHelper;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Diagnostics;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {

        int extensionFilterIndex = 0;
        public ExportSettings exportSettings {  get; set; }    
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string ExportSettingsFile { get; set; }

        private bool useTotalCountMethod = true;
        public bool UseTotalCountMethod
        {
            get => useTotalCountMethod;
            set
            {
                useTotalCountMethod = value;
                OnPropertyChanged(nameof(UseTotalCountMethod));
            }
        }
        private bool useSpecificCountMethod = true;
        public bool UseSpecificCountMethod
        {
            get => useSpecificCountMethod;
            set
            {
                useSpecificCountMethod = value;
                OnPropertyChanged(nameof(UseSpecificCountMethod));
            }
        }

        public ExportWindow()
        {
            InitializeComponent();
            exportBtn.IsEnabled = false;
        }

        private void FilePicker01_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FilePicker_FileAccepted(object sender, RoutedEventArgs e)
        {
            var filepicker = (FilePicker)sender;    
            var file = filepicker.File;
            if (!String.IsNullOrEmpty(file))
            {
                exportBtn.IsEnabled = true;
                extensionFilterIndex = filepicker.ExtensionFilterIndex;
            }
        }

       

        private async void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            
            var file = FilePicker01.File;
            var sortOption = (SortType)SortOptions.SelectedIndex;
            
            var saveExportSettings = (bool)SaveExportSettingsCheckBox.IsChecked;
            if(saveExportSettings)
            {
                SaveExportSettings();
            }

            var export = new ClanChestExporter(file,exportSettings);
            bool result = export.Export();
            if(result)
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool failedToParseDates = false;

            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";

            FilePicker01.DefaultFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath}";

            var GameChests = ApplicationManager.Instance.Chests;
            var previousChestType = String.Empty;
            HeadersComboBox.Items.Clear();
            foreach (var gamechest in GameChests)
            {
                if (gamechest.ChestType != previousChestType)
                {
                    HeadersComboBox.Items.Add(gamechest.ChestType);
                    previousChestType = gamechest.ChestType;
                }
            }

            HeadersComboBox.SelectedIndex = 0;
            var clanRootFolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}";
            ExportSettingsFile = $@"{clanRootFolder}\exportSettings.db";

            exportSettings = new ExportSettings();

            if (System.IO.File.Exists(ExportSettingsFile))
            {
                LoadExportSettings();
            }

            var dailyclanchestdata = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions != ChestOptions.UseConditions ? ClanManager.Instance.ClanChestManager.ClanChestDailyData : ClanManager.Instance.ClanChestManager.FilterClanChestByConditions();
            var firstDate = dailyclanchestdata.First().Key;
            var lastDate = dailyclanchestdata.Last().Key;

            var FirstDateTimeObject = new DateTime();
            var LastDateTimeObject = new DateTime();

            if (DateTime.TryParse(firstDate, out FirstDateTimeObject) == false)
            {
                failedToParseDates = true;
            }
            if (DateTime.TryParse(lastDate, out LastDateTimeObject) == false)
            {
                failedToParseDates = true;
            }

            if (failedToParseDates == false)
            {
                var pastDateTimeObject = FirstDateTimeObject.AddDays(-1);
                var futureDateTimeObject = LastDateTimeObject.AddDays(1);

                var beginningOfTime = pastDateTimeObject.AddYears(-1000);
                var endingOfTime = futureDateTimeObject.AddYears(1000);

                DateRangeToPicker.BlackoutDates.Add(new CalendarDateRange(beginningOfTime, pastDateTimeObject));
                DateRangeToPicker.BlackoutDates.Add(new CalendarDateRange(futureDateTimeObject, endingOfTime));

                DateRangeFromPicker.BlackoutDates.Add(new CalendarDateRange(beginningOfTime, pastDateTimeObject));
                DateRangeFromPicker.BlackoutDates.Add(new CalendarDateRange(futureDateTimeObject, endingOfTime));

                exportSettings.DateRangeTo = LastDateTimeObject;
                exportSettings.DateRangeFrom = FirstDateTimeObject;
            }

            this.DataContext = exportSettings;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void AddHeaderItemBtn_Click(object sender, RoutedEventArgs e)
        {
            if(exportSettings.ExtraHeaders.Contains(HeadersComboBox.SelectedValue.ToString()))
            {
                if(MessageBox.Show($"Unable to add same extra header. Must be unique. Can't have two or more '{HeadersComboBox.SelectedValue.ToString()}'") == MessageBoxResult.OK)
                {
                    return;
                }
            }

            exportSettings.ExtraHeaders.Add(HeadersComboBox.SelectedValue.ToString());
        }

        private void RemoveHeaderItemBtn_Click(object sender, RoutedEventArgs e)
        {
            exportSettings.ExtraHeaders.Remove(HeadersComboBox.SelectedValue.ToString());  
        }

        private void ClearHeaderItemsBtn_Click(object sender, RoutedEventArgs e)
        {
            exportSettings.ExtraHeaders.Clear();
        }

        private void MoveUpItemBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = HeadersListView.SelectedIndex;
                if (selectedIndex > 0)
                {
                    exportSettings.ExtraHeaders.Move(selectedIndex, selectedIndex - 1);
                }
            }
            catch 
            { 

            }
        }

        private void MoveDownItemBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = HeadersListView.SelectedIndex;
                if ((selectedIndex < HeadersListView.Items.Count - 1))
                {
                    exportSettings.ExtraHeaders.Move(selectedIndex, selectedIndex + 1);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void DateRangeOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = ((DateRangeEnum)DateRangeOptions.SelectedValue);


            if (c.ToString() == "Custom")
            {
                if (CustomDateRangeGrid != null)
                {
                    CustomDateRangeGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (CustomDateRangeGrid != null)
                {
                    CustomDateRangeGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void LoadExportSettings()
        {
            using (StreamReader sr = File.OpenText(ExportSettingsFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                exportSettings = (ExportSettings)serializer.Deserialize(sr, typeof(ExportSettings));
                sr.Close();
            }

            //--- start to build everything from settings.

        }
        public void SaveExportSettings()
        {
            using(StreamWriter sw = File.CreateText(ExportSettingsFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, exportSettings);
                sw.Close();
            }
        }

    }
}
