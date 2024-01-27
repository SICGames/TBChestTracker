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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {

        int extensionFilterIndex = 0;

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
                    foreach (var types in chestdata[0].ChestTypes)
                    {
                        headers.Add(types.Chest.ToString());
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
                        
                        foreach (var type in item.ChestTypes)
                        {
                            csv.WriteField(type.Total);
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

            var chestcountdata = await ClanManager.Instance.ClanChestManager.BuildChestCountDataAsync(sortOption);

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
                        }
                    }
                    break;
            }

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FilePicker01.DefaultFolder = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
