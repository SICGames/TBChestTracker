using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window, INotifyPropertyChanged
    {
        public UpdateWindow()
        {
            InitializeComponent();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _title;
        private string _description;
        public string UpgradeTitle
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));   
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description)); 
            }
        }
        private string downloadUrlPath = string.Empty;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var updateManifest = ApplicationManager.Instance.UpdateManifest;

            UpgradeTitle = $"TotalBattle Chest Tracker {updateManifest.Name} is available!";
            Description = updateManifest.Description;
            downloadUrlPath = updateManifest.Url;

            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void FancyButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(downloadUrlPath);
            this.Close();
        }
    }
}
