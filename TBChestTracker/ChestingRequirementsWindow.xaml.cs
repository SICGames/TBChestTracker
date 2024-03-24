using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
            var item = ((ListViewItem)sender).Content as ChestConditions;
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

        private void AddRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            NewRequirementWindow newRequirementWindow = new NewRequirementWindow();
            if(newRequirementWindow.ShowDialog() == true)
            {
               
            }
        }

        private void RemoveRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as ClanSpecifiedRequirements;
            ClanManager.Instance.ClanChestSettings.ClanRequirements.ClanSpecifiedRequirements.Remove(item);
        }

        private void ClearRequirementsButton_Click(object sender, RoutedEventArgs e)
        {
            ClanManager.Instance.ClanChestSettings.ClanRequirements.ClanSpecifiedRequirements.Clear();
        }

        private void RequirementListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as ClanSpecifiedRequirements;

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
            SaveFileDialog sf = new SaveFileDialog();
            sf.RestoreDirectory = true;
            sf.Filter = "Text File|*.txt";
            sf.OverwritePrompt = true;
            if(sf.ShowDialog() == true)
            {
                using (StreamWriter sw = File.CreateText(sf.FileName))
                {
                    var points = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
                    foreach (var point in points)
                    {
                        var line = $"{point.ChestType} {point.Level} - {point.PointValue}";
                        sw.WriteLine(line);
                    }
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

    }
}
