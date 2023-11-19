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
            if(ClanManager.Instance.ClanChestSettings.ChestRequirements ==  null)
                ClanManager.Instance.ClanChestSettings.InitSettings();

            this.DataContext = ClanManager.Instance.ClanChestSettings.ChestRequirements;
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

        }

        private void ClearAllConditionsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            //-- make changes.
            var clanchestsettings = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestRequirementsFile;
            ClanManager.Instance.ClanChestSettings.SaveSettings(clanchestsettings);
            this.Close();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            var item = ((ListViewItem)sender).Content as ChestConditions;
            EditConditionWindow editConditionWindow = new EditConditionWindow();
            editConditionWindow.LoadChestCondition(item.ChestType, item.Comparator, item.level);
            if(editConditionWindow.ShowDialog() == true)
            {

            }
        }
    }
}
