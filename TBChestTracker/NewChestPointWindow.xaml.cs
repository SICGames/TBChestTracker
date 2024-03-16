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


        private int pointvalue = 0;
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
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

            ChestPoints chestPoints = new ChestPoints();
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
    }
}
