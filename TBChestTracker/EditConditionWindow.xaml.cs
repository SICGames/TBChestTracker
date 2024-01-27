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
    /// Interaction logic for NewConditionWindow.xaml
    /// </summary>
    public partial class EditConditionWindow : Window
    {
        ChestConditions ChestConditions { get; set; }
        public EditConditionWindow()
        {
            InitializeComponent();
            ChestConditions = new ChestConditions();
            this.DataContext = ChestConditions; 
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(var condition in ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions)
            {
                if (condition.ChestType.Equals(ChestConditions.ChestType, StringComparison.OrdinalIgnoreCase))
                {
                    condition.ChestType = ChestTypeCondition.Text;
                    condition.level = Int32.Parse(ChestLevelCondition.Text);
                }
            }
            this.Close();
        }
        public void LoadChestCondition(string chesttype,  Int32 level)
        {
            ChestConditions.ChestType = chesttype;
            ChestConditions.level = level;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ChestConditions.ChestType.Equals("Common", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 0;
            else if (ChestConditions.ChestType.Equals("Rare", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 1;
            else if (ChestConditions.ChestType.Equals("Epic", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 2;
            else if (ChestConditions.ChestType.Equals("Citadel", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 3;
            else if (ChestConditions.ChestType.Equals("Arena", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 4;
            else if (ChestConditions.ChestType.Equals("Arena", StringComparison.OrdinalIgnoreCase))
                ChestTypeCondition.SelectedIndex = 5;

            if (ChestConditions.level == 5)
                ChestLevelCondition.SelectedIndex = 0;
            else if (ChestConditions.level == 10)
                ChestLevelCondition.SelectedIndex = 1;
            else if (ChestConditions.level == 15)
                ChestLevelCondition.SelectedIndex = 2;
            else if (ChestConditions.level == 20)
                ChestLevelCondition.SelectedIndex = 3;
            else if (ChestConditions.level == 25)
                ChestLevelCondition.SelectedIndex = 4;
            else if (ChestConditions.level == 35)
                ChestLevelCondition.SelectedIndex = 5;
        }
    }
}
