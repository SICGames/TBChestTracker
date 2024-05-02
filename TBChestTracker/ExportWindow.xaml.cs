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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {

        int extensionFilterIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
            this.DataContext = this;
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

        private Task CreateTextFileAsync(string filename, List<ChestCountData> chestdata, int chest_points, int countmethod)
        {
            return Task.Run(() => ClanManager.Instance.ClanChestManager.ExportData(filename, chestdata, chest_points, countmethod));
        }
        private void CreateCVSFile(string file, List<ChestCountData> chestdata)
        {
            //-- CSV file.
            using (var writer = new StreamWriter(file))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var headers = new List<String>();
                    headers.Add("Clanmate");
                    if (chestdata[0].ChestTypes.Count > 0)
                    {
                        foreach (var types in chestdata[0].ChestTypes)
                        {
                            headers.Add(types.Chest.ToString());
                        }
                    }

                    headers.Add("Total");

                    foreach (var heading in headers)
                    {
                        csv.WriteField(heading);
                    }
                    csv.NextRecord();

                    foreach (var item in chestdata)
                    {
                        csv.WriteField(item.Clanmate);

                        if (item.ChestTypes.Count > 0)
                        {
                            foreach (var type in item.ChestTypes)
                            {
                                csv.WriteField(type.Total);
                            }
                        }

                        csv.WriteField(item.Count);
                        csv.NextRecord();
                        
                    }
                }
            }
        }
        private Task CreateCSVFileAysnc(string file, List<ChestCountData> chestdata)
        {
            return Task.Run(() => CreateCVSFile(file, chestdata));
        }
        private async void exportBtn_Click(object sender, RoutedEventArgs e)
        {

            
            var file = FilePicker01.File;
            var sortOption = (SortType)SortOptions.SelectedIndex;
            var chest_points_value = ChestPointsValue.Text;
            var selectedExportIndex = ExportOptions.SelectedIndex;
            int chest_points = 0;
            var selectedFileExtension = FilePicker01.ExtensionFilterIndex;

            if(!Int32.TryParse(chest_points_value, out  chest_points))
            {
                MessageBox.Show("Chest Points Correction Value must be only numbers.");
                return;
            }

            var count_method = 0;
            if (ShowTotal.IsChecked == true)
                count_method = 0;
            else if(ShowIndividual.IsChecked == true) 
                count_method = 1;

            var clansettings = ClanManager.Instance.ClanChestSettings;
            List<ChestCountData> chestcountdata = null;
            if (clansettings.ClanRequirements.UseSpecifiedClanRequirements)
            {
                chestcountdata = await ClanManager.Instance.ClanChestManager.BuildSpecificChestCountDataAsync(sortOption);
            }
            else if(clansettings.ChestPointsSettings.UseChestPoints)
            {
                chestcountdata = await ClanManager.Instance.ClanChestManager.BuildChestPointsDataAsync(sortOption);
            }
            else if(!clansettings.ClanRequirements.UseSpecifiedClanRequirements && !clansettings.ChestPointsSettings.UseChestPoints)
            {
                chestcountdata = await ClanManager.Instance.ClanChestManager.BuildAllChestCountDataAsync(sortOption);
            }

            if (selectedFileExtension == 0)
            {
                await CreateTextFileAsync(file, chestcountdata, chest_points, count_method);
            }
            if(selectedFileExtension == 2)
            {
                await CreateCSVFileAysnc(file, chestcountdata);
            }
            switch (selectedExportIndex)
            {
                case 0: //-- Do nothing.
                    {
                    }
                    break;
                case 1: //-- Archive
                    {
                        //-- Clan Statistic Window will allow ability view archieved chest reports.

                    }
                    break;
                case 2: //-- Delete
                    {
                        //-- delete and rebuild.
                        var clanchestfile = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
                        try
                        {
                            System.IO.File.Delete(clanchestfile);
                            ClanManager.Instance.ClanChestManager.ClearData();
                            ClanManager.Instance.ClanChestManager.BuildData();
                        }
                        catch(Exception ex)
                        {
                            //-- log error.
                            com.HellStormGames.Logging.Loggy.Write($"{ex.Message}", com.HellStormGames.Logging.LogType.ERROR);
                        }
                    }
                    break;
            }

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FilePicker01.DefaultFolder = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath;
            var useChestPoints = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.UseChestPoints;
            var useSpecificChests = ClanManager.Instance.ClanChestSettings.ChestRequirements.useChestConditions || ClanManager.Instance.ClanChestSettings.ClanRequirements.UseSpecifiedClanRequirements;
            if(useChestPoints)
            {
                UseTotalCountMethod = true;
                UseSpecificCountMethod = false;
            }
            else if(useSpecificChests)
            {
                UseTotalCountMethod = false;
                UseSpecificCountMethod = true;
            }
            else
            {
                UseTotalCountMethod = true;
                UseSpecificCountMethod = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
