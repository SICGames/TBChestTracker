using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TBChestTracker.Windows.ClanmateRemoval
{
    /// <summary>
    /// Interaction logic for ClanmateCleaningWindow.xaml
    /// </summary>
    public partial class ClanmateCleaningWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ObservableCollection<string> _Clanmates {  get; set; }
        public ObservableCollection<string> Clanmates
        {
            get => _Clanmates;
            set
            {
                _Clanmates = value;
                onPropertyChanged(nameof(Clanmates));   
            }
        }

        List<ChestsDatabase> ChestData { get; set; }
        public ClanmateCleaningWindow()
        {
            InitializeComponent();
            Clanmates = new ObservableCollection<string>();
            this.DataContext = Clanmates;
        }

        

        private void DeleteClanmateBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> DeletionList = new List<string>();
            var selected_clanmates = CLANMATE_VIEW.SelectedItems;
            foreach (var selected in selected_clanmates)
            {
                {
                    foreach (var item in ChestData.ToList())
                    {
                        var selected_clanmate = (string)selected;
                        if (item.Clanmate == selected_clanmate)
                        {
                            string clanmate = item.Clanmate;
                            ChestData.Remove(item);
                            DeletionList.Add(clanmate);
                        }
                    }
                }
            }
            foreach(var byebye in DeletionList)
            {
                Clanmates.Remove(byebye);
            }
            DeletionList.Clear();

            ClanManager.Instance.ChestDataManager.Save();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var chestdata_Manager = ClanManager.Instance?.ChestDataManager;
            chestdata_Manager.CreateBackup();
            if (chestdata_Manager.Load())
            {
                ChestData = chestdata_Manager?.GetDatabase();
                foreach(var item in ChestData) 
                {
                    if(Clanmates.Contains(item.Clanmate) == false)
                    {
                        Clanmates.Add(item.Clanmate);
                    }
                }
            }
            else
            {
                //-- message
                MessageBox.Show("Failed to load Clan Chest Data.");
            }
            CLANMATE_VIEW.ItemsSource = Clanmates;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
