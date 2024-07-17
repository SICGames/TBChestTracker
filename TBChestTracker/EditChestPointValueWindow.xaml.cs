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
    /// Interaction logic for EditChestPointValueWindow.xaml
    /// </summary>
    public partial class EditChestPointValueWindow : Window, INotifyPropertyChanged
    {

        private ChestRef _ChestRef = new ChestRef();
        public ChestRef pChestRef
        {
            get => _ChestRef;
            set
            {
                _ChestRef = value;
                OnPropertyChanged(nameof(pChestRef));
            }
        }

        private int ChestPointsItemIndex { get; set; }

        public ChestPoints ChestPoints { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditChestPointValueWindow()
        {
            InitializeComponent();
            ChestPoints = new ChestPoints();
            pChestRef = new ChestRef();
            pChestRef.ReferenceOption = RefEnum.BYTYPE;
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

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ChestTypeBox.SelectedItem;
            if(ChestTypeBox.SelectedItem != null)
            {
                var content = ((ComboBoxItem)ChestTypeBox.SelectedItem).Content;    
                if(content.ToString().Equals("Heroic"))
                {
                    BuildLevels(ChestPointLevel, 16, 45, 1);
                }
                else
                {
                    BuildLevels(ChestPointLevel, 0, 45, 5);
                }
            }
            else
            {
                BuildLevels(ChestPointLevel, 0, 45, 5);
            }
        }

        private void ChestPointLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (ChestPoints == null)
            {
                return;
            }
            
            var item = (ComboBoxItem)ChestPointLevel.SelectedItem;
            if (item == null)
                return;


            if (!String.IsNullOrEmpty(item.Content.ToString()))
            {
                ChestPoints.Level = Int32.Parse((string)item.Content.ToString());
            }
            */
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var cp = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints[ChestPointsItemIndex];

            if (pChestRef.ReferenceOption == RefEnum.BYNAME)
            {
                cp.ChestType = "Custom";
                cp.ChestName = ChestNameBox.Text;
            }
            else
            {
                cp.ChestType = ChestTypeBox.Text;
                cp.ChestName = "";
            }

            cp.Level = Int32.Parse(ChestPointLevel.Text);
            cp.ChestRef = pChestRef;
            cp.PointValue = ChestPoints.PointValue;

            this.DialogResult = true;
            this.Close();
        }

        public void LoadChestPoints(ChestPoints c, int index)
        {
            ChestPointsItemIndex = index;
            ChestPoints.ChestRef = c.ChestRef;
            pChestRef = ChestPoints.ChestRef;
            if(pChestRef.ReferenceOption == RefEnum.BYNAME)
            {
                ChestPoints.ChestType = "Custom";
                ChestPoints.ChestName = c.ChestName;
            }
            else
            {
                ChestPoints.ChestType = c.ChestType;
                ChestPoints.ChestName = "";
            }

            ChestPoints.Level = c.Level;
            ChestPoints.PointValue = c.PointValue;

            foreach ( var item in ChestTypeBox.Items)
            {
                var comboboxItem = (ComboBoxItem)item;
                if( comboboxItem.Content.Equals(ChestPoints.ChestType) )
                {
                    comboboxItem.IsSelected = true;
                    break;
                }
            }

            switch (ChestPoints.ChestRef.ReferenceOption)
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
            BuildLevels(ChestPointLevel, 0, 45, 5);

            foreach (var chesttype in ApplicationManager.Instance.ChestTypes)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chesttype.Name;
                ChestTypeBox.Items.Add(ci);
            }

            foreach (var chestname in ApplicationManager.Instance.ChestNames)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chestname.Name;
                ChestNameBox.Items.Add(ci);
            }

            this.DataContext = ChestPoints;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void StackPanel_Click_1(object sender, RoutedEventArgs e)
        {
            switch (ChestPoints.ChestRef.ReferenceOption)
            {
                case RefEnum.BYTYPE:
                    {
                        ChestTypeBox.Visibility = Visibility.Visible;
                        ChestNameBox.Visibility = Visibility.Collapsed;
                        ChestTypeBox.SelectedIndex = 0;
                    }
                    break;
                case RefEnum.BYNAME:
                    {
                        ChestNameBox.Visibility = Visibility.Visible;
                        ChestTypeBox.Visibility = Visibility.Collapsed;
                        ChestTypeBox.Text = "Custom";
                        ChestNameBox.SelectedIndex = 0;
                    }
                    break;
            }
            ChestPointLevel.SelectedIndex = 0;
        }
        private void ChestNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
