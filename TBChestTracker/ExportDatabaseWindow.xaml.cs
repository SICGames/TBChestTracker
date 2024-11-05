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
using TBChestTracker.Pages;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ExportDatabaseWindow.xaml
    /// </summary>
    public partial class ExportDatabaseWindow : Window, INotifyPropertyChanged
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
        public ExportDatabaseWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        public string DestinationFilePath { get; set; }
        public string SourceFolderPath { get; set; }

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
        private async Task ExportClanDatabaseAsync(string src, string dest)
        {
            var progress = new Progress<ZipProgress>(x =>
            {
                double total = x.Total;
                var current = x.CurrentItem;
                double processed = x.Processed;
                double percentage = Math.Round((processed / total) * 100.0);
                UpdateUI(percentage); 
                //splashScreen.UpdateStatus($"Extracting Trained Tesseract Models.... ({Math.Round(processed)}/{Math.Round(total)})", percentage);
            });

           await ArchiveManager.Create(src, dest, progress);
           await Task.Delay(1000);
        }

        private async void ExportClanDatabase(string src, string dest)
        {
            await Task.Run(() => ExportClanDatabaseAsync(src,dest));
            this.Close();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExportClanDatabase(SourceFolderPath, DestinationFilePath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
