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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
     
        public MainWindow mainwindow { get; set; }
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
            await mainwindow.ClanChestManager.ExportDataAsync(file, sortOption);

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
                        var clanchestfile = ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
                        try
                        {
                            System.IO.File.Delete(clanchestfile);
                            mainwindow.ClanChestManager.ClearData();
                            mainwindow.ClanChestManager.BuildData();
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
    }
}
