using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class EditConditionWindow : Window, INotifyPropertyChanged
    {
        ChestConditions ChestConditions { get; set; }
        private int itemIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        
        private List<GameChest> GameChests { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public EditConditionWindow()
        {
            InitializeComponent();
            ChestConditions = new ChestConditions();
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
            Debug.WriteLine("Building Chest Names!");

            this.Dispatcher.BeginInvoke(new Action(() =>
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
            }));
        }

        private void BuildLevels(ComboBox cb)
        {
            Debug.WriteLine("Building Chest Levels!");
            this.Dispatcher.BeginInvoke(new Action(() =>
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
                else
                {
                    cb.SelectedIndex = 0;
                }


            }));

        }
        private void UpdateChestTypeSelection()
        {
            Debug.WriteLine("Updating Chest Types!");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ChestTypeBox.Items)
                {
                    var comboboxItem = (ComboBoxItem)item;
                    if (comboboxItem.Content.Equals(ChestConditions.ChestType))
                    {
                        comboboxItem.IsSelected = true;
                        break;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        private void UpdateChestNameSelection()
        {
            Debug.WriteLine("Updating Chest Names!");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ChestNameBox.Items)
                {
                    var cbi = (ComboBoxItem)item;
                    if (cbi.Content.Equals(ChestConditions.ChestName))
                    {
                        cbi.IsSelected = true;
                        break;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void UpdateChestLevelSelection()
        {
            Debug.WriteLine("Updating Chest Levels!");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ChestLevelCondition.Items)
                {
                    var cbi = (ComboBoxItem)item;
                    if (cbi.Content.Equals(ChestConditions.level))
                    {
                        cbi.IsSelected = true;
                        break;
                    }

                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void BuildControls()
        {
            BuildChestTypes(ChestTypeBox);
            UpdateChestTypeSelection();
            ChestTypeBox.SelectionChanged += ChestTypeBox_SelectionChanged;
            UpdateChestNameSelection();
            UpdateChestLevelSelection();
        }
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var condition = ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions[itemIndex];
            condition.level = ChestLevelCondition.Text;
            this.Close();
        }
        public void LoadChestCondition(ChestConditions chestcondition, int index)
        {
            itemIndex = index;

            ChestConditions.ChestType = chestcondition.ChestType;
            ChestConditions.level = chestcondition.level;
            ChestConditions.ChestName = chestcondition.ChestName;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildControls();
            this.DataContext = ChestConditions;
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ChestTypeBox Fired Event!");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = (ComboBoxItem)ChestTypeBox.SelectedItem;
                if (ChestTypeBox.SelectedItem != null)
                {
                    var content = ((ComboBoxItem)ChestTypeBox.SelectedItem).Content;
                    BuildChestNames(ChestNameBox, content.ToString());
                    BuildLevels(ChestLevelCondition);
                }
            }));
        }
    }
}
