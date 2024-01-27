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
    }
}
