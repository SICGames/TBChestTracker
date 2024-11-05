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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ImportDatabaseWindow.xaml
    /// </summary>
    public partial class ImportDatabaseWindow : Window, INotifyPropertyChanged
    {
        private double? _Percentage;
        public double? Percentage
        {
            get => _Percentage.GetValueOrDefault(0);
            set
            {
                _Percentage = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }

        private string _PercentageText = string.Empty;
        public string PercentageText
        {
            get => _PercentageText;
            set
            {
                _PercentageText = value;
                OnPropertyChanged(nameof(PercentageText));
            }
        }
        
        public string SourceFile { get; set; }
        public string DestFolderPath { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UpdateUI(double percentage)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Percentage = percentage;
                PercentageText = $"{Percentage}%";
            }));
        }


        public ImportDatabaseWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private async Task ImportClanDatabaseAsync(string src, string dest)
        {
            var progress = new Progress<ZipProgress>(x =>
            {
                double total = x.Total;
                var current = x.CurrentItem;
                double processed = x.Processed;
                double percentage = Math.Round((processed / total) * 100.0);
                UpdateUI(percentage);
            });
            await ArchiveManager.Extract(src, dest, progress);
            await Task.Delay(1000);
        }

        private async void ImportClanDatabase(string src, string dest)
        {
            await Task.Run(() => ImportClanDatabaseAsync(src, dest));
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImportClanDatabase(SourceFile, DestFolderPath);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
