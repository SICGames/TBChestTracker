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
    /// Interaction logic for NewRequirementWindow.xaml
    /// </summary>
    public partial class NewRequirementWindow : Window
    {
        public NewRequirementWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ClanSpecifiedRequirements clanSpecifiedRequirements = new ClanSpecifiedRequirements();
            clanSpecifiedRequirements.ChestType = ChestTypeRequirement.Text;
            clanSpecifiedRequirements.ChestLevel = Int32.Parse(ChestLevelRequirement.Text);
            clanSpecifiedRequirements.AmountPerDay = Int32.Parse(ChestPerDayRequirementText.Text);
            clanSpecifiedRequirements.ChestOperator = ChestOperatorRequirement.Text;    
            ClanManager.Instance.ClanChestSettings.ClanRequirements.ClanSpecifiedRequirements.Add(clanSpecifiedRequirements);
            this.DialogResult = true;
            this.Close();
        }

    }
}
