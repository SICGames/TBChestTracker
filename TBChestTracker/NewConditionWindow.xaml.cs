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
    /// Interaction logic for NewConditionWindow.xaml
    /// </summary>
    public partial class NewConditionWindow : Window
    {
        public NewConditionWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestConditions chestcondition = new ChestConditions();

            chestcondition.ChestType = ChestTypeCondition.Text;
            chestcondition.Comparator = ComparatorCondition.Text;
            chestcondition.level = Int32.Parse(ChestLevelCondition.Text);
            ClanChestSettings.ChestRequirements.ChestConditions.Add(chestcondition);
            this.Close();
        }
    }
}
