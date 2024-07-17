using CsvHelper.Configuration.Attributes;
using Emgu.CV.CvEnum;
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
    /// Interaction logic for NewChestPointWindow.xaml
    /// </summary>
    public partial class NewChestPointWindow : Window, INotifyPropertyChanged
    {

        private string _chestName = "";
        private int pointvalue = 0;
        private ChestRef _ChestRef = new ChestRef();
        public ChestRef ChestRef
        {
            get => _ChestRef;
            set
            {
                _ChestRef = value;
                OnPropertyChanged(nameof(ChestRef));
            }
        }
        public string ChestName
        {
            get => this._chestName;
            set
            {
                this._chestName = value;
                OnPropertyChanged(nameof(ChestName));
            }
        }
        
        public int PointValue
        {
            get => pointvalue;
            set
            {
                pointvalue = value;
                OnPropertyChanged(nameof(pointvalue));
            }
        }
        private int level = 5;
        public int Level
        {
            get => level;
            set
            {
                level = value;
                OnPropertyChanged(nameof(Level));
            }
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public NewChestPointWindow()
        {
            InitializeComponent();
            ChestRef = new ChestRef();
            ChestRef.ReferenceOption = RefEnum.BYTYPE;
            this.DataContext = this;
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestPoints chestPoints = new ChestPoints();
            chestPoints.ChestRef = ChestRef;

            
            if (chestPoints.ChestRef.ReferenceOption == RefEnum.BYNAME)
            {
                chestPoints.ChestType = "Custom";
                chestPoints.ChestName = ChestNameBox.Text;
            }
            else
            {
                chestPoints.ChestType = ChestTypeBox.Text;
                chestPoints.ChestName = "";
            }
            chestPoints.Level = Int32.Parse(ChestPointLevel.Text);
            chestPoints.PointValue = PointValue;
            ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints.Add(chestPoints);
            this.DialogResult = true;
            this.Close();
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
          
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            switch(ChestRef.ReferenceOption)
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
                    }
                    break;
            }
            ChestPointLevel.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildLevels(ChestPointLevel, 0, 45, 5);

            //-- load chestypes from chesttypes.csv file
            var chesttypes = ApplicationManager.Instance.ChestTypes;
            var chestnames = ApplicationManager.Instance.ChestNames;
            foreach( var chesttype in chesttypes )
            {
                var ci = new ComboBoxItem();
                ci.Content = chesttype.Name;
                ChestTypeBox.Items.Add(ci);
            }

            foreach (var chestname in chestnames )
            {
                var ci = new ComboBoxItem();
                ci.Content = chestname.Name;
                ChestNameBox.Items.Add(ci);  
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void ChestNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
