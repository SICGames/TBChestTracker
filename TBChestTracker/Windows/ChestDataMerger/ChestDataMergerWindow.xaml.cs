using Microsoft.Win32;
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
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ChestDataMerger.xaml
    /// </summary>
    public partial class ChestDataMergerWindow : Window
    {
        private string source_file;
        private string output_file;

        public ChestDataMergerWindow()
        {
            InitializeComponent();
        }

        private void InputFilePicker_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = InputFilePicker.Filters;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if(ofd.ShowDialog() == true)
            {
                var filename = ofd.FileName;
                source_file = filename;
                InputFilePicker.Source = source_file;
            }
        }

        private void OutputFilePicker_Click(object sender, RoutedEventArgs e)
        {
            var savedialog = new SaveFileDialog();
            savedialog.Filter = OutputFilePicker.Filters;
            savedialog.RestoreDirectory = true;

            if (savedialog.ShowDialog() == true)
            {
                var filename = savedialog.FileName;
                output_file = filename;
                OutputFilePicker.Source = output_file;
            }
        }

        private List<ChestsDatabase> LoadChestCollection(string filename = "")
        {
            var ChestCollection = new List<ChestsDatabase>();
            if (ClanManager.Instance.ChestDataManager.Load(filename))
            {
                ChestCollection = ClanManager.Instance.ChestDataManager.GetDatabase();
                return ChestCollection;
            }
            return null;
        }
        private async Task MergeTask()
        {
            var chestdata = LoadChestCollection();
            var input_chestdata = LoadChestCollection(source_file);

        }
        private void MergeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InputFilePicker_Accepted(object sender, RoutedEventArgs e)
        {

        }

        private void OutputFilePicker_Accepted(object sender, RoutedEventArgs e)
        {

        }
    }
}
