using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    /// Interaction logic for ChestingRequirementsWindow.xaml
    /// </summary>
    public partial class ChestingRequirementsWindow : Window
    {
        public ChestingRequirementsWindow()
        {
            InitializeComponent();
            
            this.DataContext = ClanManager.Instance.ClanChestSettings;
        }

        private void AddConditionBtn_Click(object sender, RoutedEventArgs e)
        {
            NewConditionWindow newConditionWindow = new NewConditionWindow();
            if(newConditionWindow.ShowDialog() == true)
            {

            }
        }
        private void RemoveConditionBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = ChestConditionsListView.SelectedItem as ChestConditions;
            
            ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions.Remove(item);
        }

        private void ClearAllConditionsBtn_Click(object sender, RoutedEventArgs e)
        {
            ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions.Clear();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            //-- make changes.
            var clanchestsettings = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanSettingsFile;
            ClanManager.Instance.ClanChestSettings.SaveSettings(clanchestsettings);
            this.Close();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as ChestConditions;
            EditConditionWindow editConditionWindow = new EditConditionWindow();
            editConditionWindow.LoadChestCondition(item.ChestType, item.level);
            if(editConditionWindow.ShowDialog() == true)
            {

            }
        }
      
        private void AddChestPointButton_Click(object sender, RoutedEventArgs e)
        {
            NewChestPointWindow newChestPointWindow = new NewChestPointWindow();
            if(newChestPointWindow.ShowDialog() == true)
            {

            }
        }

        private void RemoveChestPointButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ChestPointsListView.SelectedItem as ChestPoints;
            ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints.Remove(item);
        }

        private void ClearChestPointsButton_Click(object sender, RoutedEventArgs e)
        {
            ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints.Clear();
        }

        private void ExportChestPointsToFile_Click(object sender, RoutedEventArgs e)
        {

            //-- one file is text file and other file is db file to allow users to import if sharing same point values

            SaveFileDialog sf = new SaveFileDialog();
            sf.RestoreDirectory = true;
            sf.Filter = "Text File|*.txt";
            sf.OverwritePrompt = true;
            if(sf.ShowDialog() == true)
            {
                var filename = sf.FileName;
                using (StreamWriter sw = File.CreateText(filename))
                {
                    var points = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
                    foreach (var point in points)
                    {
                        var line = String.IsNullOrEmpty(point.ChestName) == true ? 
                            $"{point.ChestType} {point.Level} - {point.PointValue}" : $"{point.ChestName} {point.Level} - {point.PointValue}";

                        sw.WriteLine(line);
                    }
                    sw.Close();
                }

                var dbFilename = filename.Substring(0,filename.LastIndexOf("."));
                dbFilename += $".cpd";
                using(var sw =  File.CreateText(dbFilename))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    serializer.Serialize(sw, ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints);
                    sw.Close();
                }
            }
        }

        private void EditChestPoints_Click(object sender, RoutedEventArgs e)
        {
            var item = ChestPointsListView.SelectedItem as ChestPoints;
            EditChestPointValueWindow editChestPointValueWindow = new EditChestPointValueWindow();
            editChestPointValueWindow.LoadChestPoints(item, ChestPointsListView.SelectedIndex);
            if(editChestPointValueWindow.ShowDialog() == true)
            {

            }

        }

        private void ChestPointsListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as ChestPoints;
            if (item == null) return;
            EditChestPointValueWindow editChestPointValueWindow = new EditChestPointValueWindow();
            editChestPointValueWindow.LoadChestPoints(item, ChestPointsListView.SelectedIndex);
            if (editChestPointValueWindow.ShowDialog()== true)
            {

            }

        }

        private void ImportChestPointsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Chest Points Data file|*.cpd|All Files|*.*";
            ofd.RestoreDirectory = true;
            if(ofd.ShowDialog() == true)
            {
                using(var sr = File.OpenText(ofd.FileName))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    var chestpoints = new ObservableCollection<ChestPoints>();
                    
                    chestpoints = (ObservableCollection<ChestPoints>)serializer.Deserialize(sr, typeof(ObservableCollection<ChestPoints>));
                    ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints = chestpoints;
                    sr.Close();
                    ICollectionView collectionView = CollectionViewSource.GetDefaultView(ChestPointsListView.ItemsSource);
                    collectionView.Refresh();
                }
            }
        }
    }
}
