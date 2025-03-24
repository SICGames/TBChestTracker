using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class NewConditionWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private List<GameChest> GameChests {  get; set; }
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public NewConditionWindow()
        {
            InitializeComponent();
            GameChests = ApplicationManager.Instance.Chests;
        }
        private void BuildChestTypes(ComboBox cb)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                cb.Items.Clear();
                var previousChestType = String.Empty;

                foreach (var gameChest in GameChests)
                {
                    bool bAlreadyExists = cb.Items.Cast<ComboBoxItem>().Any(cbi => cbi.Content.Equals(gameChest.ChestType));
                    if (bAlreadyExists == false)
                    {
                        ComboBoxItem cbi = new ComboBoxItem();
                        cbi.Content = gameChest.ChestType;
                        cb.Items.Add(cbi);
                        previousChestType = gameChest.ChestType;
                    }
                }
            }));
        }
        private void BuildChestNames(ComboBox cb, string chestType)
        {
            cb.Items.Clear();
            var cbi_p = new ComboBoxItem();
            cbi_p.Content = "(Any)";
            cb.Items.Add(cbi_p);

            foreach (var gameChest in GameChests)
            {
                if (chestType == gameChest.ChestType)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = gameChest.ChestName;
                    cb.Items.Add(cbi);
                }
            }
            cb.SelectedIndex = 0;
        }

        private void BuildLevels(ComboBox cb)
        {

            cb.Items.Clear();
            var cbi_p = new ComboBoxItem();
            cbi_p.Content = "(Any)";
            cb.Items.Add(cbi_p);

            int min, max, amount = 0;
            min = max = amount;

            var ChestTypeSelectedString = ChestTypeBox.Text;

            bool bHasNoLevel = false;

            foreach (var gamechest in GameChests)
            {
                if (gamechest.ChestType == ChestTypeSelectedString)
                {
                    if (gamechest.HasLevel)
                    {
                        min = gamechest.MinLevel;
                        max = gamechest.MaxLevel;
                        amount = gamechest.IncrementPerLevel;
                        break;
                    }
                    else
                    {
                        bHasNoLevel = true;
                    }
                }
            }
            if (!bHasNoLevel)
            {
                for (int i = min; i <= max; i += amount)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = i.ToString();
                    cb.Items.Add(cbi);
                }
            }

            cb.SelectedIndex = 0;
        }
       

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestConditions chestcondition = new ChestConditions();
            chestcondition.ChestType = ChestTypeBox.Text;
            chestcondition.ChestName = ChestNameBox.Text;
            chestcondition.level = ChestLevelCondition.Text;
            ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions.Add(chestcondition);
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildChestTypes(ChestTypeBox);

            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }


        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ChestTypeBox.SelectedItem != null)
                {
                    var content = ((ComboBoxItem)ChestTypeBox.SelectedItem).Content;
                    BuildLevels(ChestLevelCondition);
                    BuildChestNames(ChestNameBox, content.ToString());
                }
            }));
        }
    }
}
