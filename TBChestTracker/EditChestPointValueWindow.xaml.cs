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
            this.DataContext = ChestPoints;
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ChestTypeBox.SelectedItem;

            if (item.Content.ToString().Equals("Heroic", StringComparison.CurrentCultureIgnoreCase))
            {
                if (LevelTextBox != null)
                    LevelTextBox.Visibility = Visibility.Visible;

                if (ChestPointLevel != null)
                    ChestPointLevel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (LevelTextBox != null)
                    LevelTextBox.Visibility = Visibility.Collapsed;

                if (ChestPointLevel != null)
                    ChestPointLevel.Visibility = Visibility.Visible;
            }
        }

        private void ChestPointLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChestPoints == null)
                return;
            
            var item = (ComboBoxItem)ChestPointLevel.SelectedItem;
            if (item == null)
                return;

            if (!String.IsNullOrEmpty(item.Content.ToString()))
            {
                ChestPoints.Level = Int32.Parse((string)item.Content.ToString());
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var cp = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;

            var index = 0;
            foreach(var chestpointItem in cp.ToList())
            {
                if(chestpointItem.ChestType == ChestPoints.ChestType && index == ChestPointsItemIndex)
                {
                    chestpointItem.Level = ChestPoints.Level;
                    chestpointItem.PointValue = ChestPoints.PointValue;
                    break;
                }
                index++;
            }

            this.DialogResult = true;
            this.Close();
        }

        public void LoadChestPoints(ChestPoints c, int index)
        {
            ChestPointsItemIndex = index;

            ChestPoints.ChestType = c.ChestType;
            ChestPoints.Level = c.Level;
            ChestPoints.PointValue = c.PointValue;
            
            var ChestType = c.ChestType;
            var chestboxchildren = ChestTypeBox.Items;
            
            foreach ( var item in chestboxchildren )
            {
                var comboboxItem = (ComboBoxItem)item;
                if( comboboxItem.Content.Equals(ChestType) )
                {
                    comboboxItem.IsSelected = true;
                    
                    break;
                }
            }

            if(ChestPointLevel.Visibility == Visibility.Visible )
            {
                foreach( var item in ChestPointLevel.Items )
                {
                    var cbi = (ComboBoxItem)item;
                    if(cbi.Content.Equals(c.Level.ToString()) )
                    {
                        cbi.IsSelected = true;
                        ChestPoints.Level = Int32.Parse(ChestPointLevel.Text);

                        break;
                    }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
