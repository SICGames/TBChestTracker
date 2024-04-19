using CsvHelper.Configuration.Attributes;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestPoints chestPoints = new ChestPoints();
            chestPoints.ChestRef = ChestRef;

            chestPoints.ChestName = ChestName;
            chestPoints.ChestType = ChestTypeBox.Text;
            chestPoints.Level = Level;
            chestPoints.PointValue = PointValue;
            ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints.Add(chestPoints);
            this.DialogResult = true;
            this.Close();
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ChestTypeBox.SelectedItem;

            if(item.Content.ToString().Equals("Heroic", StringComparison.CurrentCultureIgnoreCase))
            {
                if(LevelTextBox != null)
                    LevelTextBox.Visibility = Visibility.Visible;

                if(ChestPointLevel != null)
                    ChestPointLevel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if(LevelTextBox != null)
                    LevelTextBox.Visibility = Visibility.Collapsed;

                if (ChestPointLevel != null)
                    ChestPointLevel.Visibility = Visibility.Visible;
            }
        }

        private void ChestPointLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ChestPointLevel.SelectedItem;
            if (item == null)
                return;

            if (!String.IsNullOrEmpty(item.Content.ToString()))
            {
                Level = Int32.Parse((string)item.Content.ToString());
            }
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            switch(ChestRef.ReferenceOption)
            {
                case RefEnum.BYTYPE:
                    {
                        ChestTypeBox.Visibility = Visibility.Visible;
                        ChestTextBox.Visibility = Visibility.Collapsed;
                    }
                    break;
                case RefEnum.BYNAME:
                    {
                        ChestTextBox.Visibility = Visibility.Visible;
                        ChestTypeBox.Visibility = Visibility.Collapsed;
                        ChestTypeBox.Text = "Custom";
                    }
                    break;

            }
        }
    }
}
