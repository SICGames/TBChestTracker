using Microsoft.WindowsAPICodePack.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for EditChestPointValueWindow.xaml
    /// </summary>
    public partial class EditChestPointValueWindow : Window, INotifyPropertyChanged
    {
        public int ChestPointsItemIndex { get; set; }
        public ChestPoints ChestPoints { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private List<GameChest> GameChests { get; set; }
        private bool bDontRebuild = false;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditChestPointValueWindow()
        {
            InitializeComponent();
            ChestPoints = new ChestPoints();
            this.DataContext = ChestPoints;
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
                    BuildLevels(ChestPointLevel);
                }
            }));
        }
        private Task BuildChestTypesAsync(ComboBox cb) => Task.Run(() => BuildChestTypes(cb));

        private void BuildChestTypes(ComboBox cb)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                cb.Items.Clear();
                var previousChestType = String.Empty;

                foreach (var gameChest in GameChests)
                {
                    if (gameChest.ChestType != previousChestType)
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var cp = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints[ChestPointsItemIndex];
            cp.Level = ChestPointLevel.Text;
            cp.PointValue = ChestPoints.PointValue;
            this.DialogResult = true;
            this.Close();
        }

        public void LoadChestPoints(ChestPoints c, int index)
        {
            
            ChestPointsItemIndex = index;
            ChestPoints.ChestType = c.ChestType;
            ChestPoints.ChestName = c.ChestName;
            ChestPoints.Level = c.Level;
            ChestPoints.PointValue = c.PointValue;
            
        }

        private void UpdateChestTypeSelection()
        {
            Debug.WriteLine("Updating Chest Types!");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ChestTypeBox.Items)
                {
                    var comboboxItem = (ComboBoxItem)item;
                    if (comboboxItem.Content.Equals(ChestPoints.ChestType))
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
                    if (cbi.Content.Equals(ChestPoints.ChestName))
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
                foreach (var item in ChestPointLevel.Items)
                {
                    var cbi = (ComboBoxItem)item;
                    if (cbi.Content.Equals(ChestPoints.Level))
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameChests = ApplicationManager.Instance.Chests;
            BuildControls();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
