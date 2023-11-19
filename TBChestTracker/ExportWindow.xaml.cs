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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
     
        
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
            var file = ((FilePicker)sender).File;
            if (!String.IsNullOrEmpty(file))
            {
                exportBtn.IsEnabled = true;
            }
        }

        private async void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedExportIndex = ExportOptions.SelectedIndex;

            var file = FilePicker01.File;
            var sortOption = (SortType)SortOptions.SelectedIndex;
            var chest_points_value = ChestPointsValue.Text;
            int chest_points = 0;
            if(!Int32.TryParse(chest_points_value, out  chest_points))
            {
                MessageBox.Show("Chest Points Correction Value must be only numbers.");
                return;
            }
            
            await ClanManager.Instance.ClanChestManager.ExportDataAsync(file, chest_points, sortOption);

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
