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
        private string level = "(Any)";
        public string Level
        {
            get => level;
            set
            {
                level = value;
                OnPropertyChanged(nameof(Level));
            }
        }

        private List<GameChest> GameChests { get; set; }

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
        private Task BuildChestTypesAsync(ComboBox cb) => Task.Run(()=> BuildChestTypes(cb));

        private void BuildChestTypes(ComboBox cb)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                cb.Items.Clear();
                
                foreach (var gameChest in GameChests)
                {
                    bool bAlreadyExists = cb.Items.Cast<ComboBoxItem>().Any(cbi => cbi.Content.Equals(gameChest.ChestType));
                    if(bAlreadyExists == false)
                    {
                        ComboBoxItem cbi = new ComboBoxItem();
                        cbi.Content = gameChest.ChestType;
                        cb.Items.Add(cbi);
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
                if(chestType == gameChest.ChestType)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content=gameChest.ChestName;
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
                if(gamechest.ChestType == ChestTypeSelectedString)
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestPoints chestPoints = new ChestPoints();
            chestPoints.ChestName = ChestNameBox.Text;
            chestPoints.ChestType = ChestTypeBox.Text;  
            chestPoints.Level = ChestPointLevel.Text;
            chestPoints.PointValue = PointValue;
            ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints.Add(chestPoints);
            this.DialogResult = true;
            this.Close();
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ChestTypeBox.SelectedItem != null)
                {
                    var content = ((ComboBoxItem)ChestTypeBox.SelectedItem).Content;
                    BuildLevels(ChestPointLevel);
                    BuildChestNames(ChestNameBox, content.ToString());
                }
            }));
        }

        private void ChestPointLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
        }
        private async void Init()
        {
            await BuildChestTypesAsync(ChestTypeBox);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //-- load chestypes from chesttypes.csv file
            GameChests = ApplicationManager.Instance.Chests;
            BuildChestTypes(ChestTypeBox);
        }

        private void ChestNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
        }
    }
}
