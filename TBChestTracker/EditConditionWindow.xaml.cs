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
    public partial class EditConditionWindow : Window, INotifyPropertyChanged
    {
        ChestConditions ChestConditions { get; set; }
        private ChestRef _ChestRef = new ChestRef();
        private int itemIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ChestRef ChestRef
        {
            get => _ChestRef;
            set
            {
                _ChestRef = value;
                OnPropertyChanged(nameof(ChestRef));
            }
        }

        public EditConditionWindow()
        {
            InitializeComponent();
            ChestConditions = new ChestConditions();
            ChestRef.ReferenceOption = RefEnum.BYNAME;
        }

        private void BuildLevels(ComboBox cb, int minLevel = 0, int maxLevel = 45, int StepAmount = 1)
        {
            cb.Items.Clear();
            for (int i = minLevel; i <= maxLevel; i += StepAmount)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = i.ToString();
                cb.Items.Add(cbi);
            }
            cb.SelectedIndex = 0;
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var condition = ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions[itemIndex];

            if (ChestRef.ReferenceOption == RefEnum.BYNAME)
            {
                condition.ChestType = "Custom";
                condition.ChestName = ChestNameBox.Text;
            }
            else
            {
                condition.ChestType = ChestTypeBox.Text;
                condition.ChestName = "";
            }

            condition.ChestRef = ChestRef;
            condition.level = Int32.Parse(ChestLevelCondition.Text);
            this.Close();
        }
        public void LoadChestCondition(ChestConditions chestcondition, int index)
        {
            itemIndex = index;

            ChestConditions.ChestType = chestcondition.ChestType;
            ChestConditions.level = chestcondition.level;
            ChestConditions.ChestRef = chestcondition.ChestRef;
            ChestRef = chestcondition.ChestRef;

            ChestConditions.ChestName = chestcondition.ChestName;
            
            foreach (var item in ChestTypeBox.Items)
            {
                var comboboxItem = (ComboBoxItem)item;
                if (comboboxItem.Content.Equals(chestcondition.ChestType))
                {
                    comboboxItem.IsSelected = true;
                    break;
                }
            }

            switch (ChestConditions.ChestRef.ReferenceOption)
            {
                case RefEnum.BYTYPE:
                    {
                        ChestTypeBox.Visibility = Visibility.Visible;
                        ChestNameBox.Visibility = Visibility.Collapsed;
                    }
                    break;
                case RefEnum.BYNAME:
                    {
                        ChestNameBox.Visibility = Visibility.Visible;
                        ChestTypeBox.Visibility = Visibility.Collapsed;
                        ChestTypeBox.Text = "Custom";
                    }
                    break;
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildLevels(ChestLevelCondition, 5);

            foreach (var chesttype in ApplicationManager.Instance.ChestTypes)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chesttype.Name;
                ChestTypeBox.Items.Add(ci);
            }
            /*
            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Content = "Custom";
            ChestTypeBox.Items.Add(cbi);
            */
            foreach (var chestname in ApplicationManager.Instance.ChestNames)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chestname.Name;
                ChestNameBox.Items.Add(ci);
            }

            this.DataContext = ChestConditions;
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ChestTypeBox.SelectedItem != null)
            {
                var content = ((ComboBoxItem)ChestTypeBox.SelectedItem).Content;    
                if(content.ToString().Equals("Heroic"))
                {
                    BuildLevels(ChestLevelCondition, 16, 45,1);
                }
                else
                {
                    BuildLevels(ChestLevelCondition, 0, 45,5);
                }
            }
            else
            {
                BuildLevels(ChestLevelCondition, 0, 45, 5);
            }
        }

        private void ChestNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            switch (ChestConditions.ChestRef.ReferenceOption)
            {
                case RefEnum.BYTYPE:
                    {
                        ChestTypeBox.Visibility = Visibility.Visible;
                        ChestTypeBox.SelectedIndex = 0;
                        ChestNameBox.Visibility = Visibility.Collapsed;
                    }
                    break;
                case RefEnum.BYNAME:
                    {
                        ChestNameBox.Visibility = Visibility.Visible;
                        ChestNameBox.SelectedIndex = 0;
                        ChestTypeBox.Visibility = Visibility.Collapsed;
                        ChestTypeBox.Text = "Custom";
                    }
                    break;
            }
            ChestLevelCondition.SelectedIndex = 0;
        }
    }
}
