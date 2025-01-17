using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ChestBuilderWindow.xaml
    /// </summary>
    public partial class ChestBuilderWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));  
            }
        }

        private ObservableCollection<GameChest> gameChests = new ObservableCollection<GameChest>();
        public ObservableCollection<GameChest> Chests {
            get => gameChests;
            set
            {
                this.gameChests = value;
                OnPropertyChanged(nameof(Chests));  
            }
        }
        private string _ChestType = String.Empty;
        private string _ChestName = String.Empty;   
        private bool _HasLevel = false;
        private int _MinLevel = 5;
        private int _MaxLevel = 35;
        private int _IncrementPerLevel = 5;

        private int _unsavedChanges = 0;
        private int _selectedIndex = 0;

        public string ChestType
        {
            get => _ChestType;
            set
            {
                _ChestType= value;
                OnPropertyChanged(nameof(ChestType));
            }
        }
        public string ChestName
        {
            get => _ChestName;
            set
            {
                _ChestName= value;
                OnPropertyChanged(nameof(ChestName));
            }
        }
        public bool HasLevel
        {
            get { return _HasLevel; }
            set
            {
                _HasLevel = value;
                OnPropertyChanged(nameof(HasLevel));
            }
        }
        public int MinLevel
        {
            get => (int)_MinLevel;
            set
            {
                _MinLevel= value;
                OnPropertyChanged(nameof(MinLevel));
            }
        }
        public int MaxLevel
        {
            get => (int)_MaxLevel;
            set
            {
                _MaxLevel= value;
                OnPropertyChanged(nameof(MaxLevel));
            }
        }
        public int IncrementPerLevel
        {
            get => _IncrementPerLevel;
            set
            {
                _IncrementPerLevel= value;
                OnPropertyChanged(nameof(IncrementPerLevel));
            }
        }

        public ChestBuilderWindow()
        {
            InitializeComponent();
            
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var chest in ApplicationManager.Instance.Chests)
            {
                Chests.Add(chest);
            }
            
            this.DataContext = this;
        }

        private void AddChestBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ChestBuilder_Editor.Visibility == Visibility.Collapsed)
            {
                ChestBuilder_Editor.Visibility = Visibility.Visible;
                OkBtn.Content = "OK";
            }
        }

        private void RemoveAllChestsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will clear all chests. Are you sure?", "Remove All Chests", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Chests.Clear();
                _unsavedChanges++;
            }
        }

        private void RemoveChestBtn_Click(object sender, RoutedEventArgs e)
        {
            Chests.RemoveAt(_selectedIndex);

            _unsavedChanges++;
        }
        private void EditChestBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestBuilder_Editor.Visibility = Visibility.Visible;
            OkBtn.Content = "Apply";
            var selectedChest = Chests[_selectedIndex];
            this.ChestType = selectedChest.ChestType;
            this.ChestName = selectedChest.ChestName;
            this.HasLevel = selectedChest.HasLevel;
            this.MinLevel = selectedChest.MinLevel;
            this.MaxLevel = selectedChest.MaxLevel;
            this.IncrementPerLevel = selectedChest.IncrementPerLevel;
        }
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            var gamechest = new GameChest();
            gamechest.ChestType = this.ChestType;
            gamechest.ChestName = this.ChestName;
            gamechest.HasLevel = this.HasLevel;
            if (this.HasLevel)
            {
                gamechest.MinLevel = this.MinLevel;
                gamechest.MaxLevel = this.MaxLevel;
                gamechest.IncrementPerLevel = this.IncrementPerLevel;
            }
            else
            {
                gamechest.MinLevel = 0;
                gamechest.MaxLevel = 0;
                gamechest.IncrementPerLevel = 0;
            }

            if (OkBtn.Content.ToString().ToLower().Contains("ok"))
            {
                Chests.Add(gamechest);
            }
            else if(OkBtn.Content.ToString().ToLower().Contains("apply"))
            {
                Chests[_selectedIndex] = gamechest;   
            }

            this.ChestType = String.Empty;
            this.ChestName = String.Empty;
            this.HasLevel = false;
            _unsavedChanges++;
            ChestBuilder_Editor.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_unsavedChanges > 0)
            {
                var result = MessageBox.Show("You have unsaved changes done to Chests file. Save them?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    //-- save it time.
                    Save();
                    _unsavedChanges = 0;
                }
            }
        }

        private void Save()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            var file = $"{ApplicationManager.Instance.LocalePath}Chests.csv";
            using (StreamWriter sw = new StreamWriter(file))
            {
                using (CsvWriter csv = new CsvWriter(sw, config))
                {
                    csv.WriteRecords(Chests);
                }
            }
            ApplicationManager.Instance.Chests.Clear();
            foreach (var chest in Chests)
            {
                ApplicationManager.Instance.Chests.Add(chest);
            }
        }

        private void CHESTS_LISTVIEW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedIndex = CHESTS_LISTVIEW.SelectedIndex;
            OkBtn.Content = "Apply";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Save();
            _unsavedChanges = 0;
            this.Close();
        }
    }
}
