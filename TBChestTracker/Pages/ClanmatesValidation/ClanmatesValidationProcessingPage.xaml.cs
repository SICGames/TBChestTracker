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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBChestTracker.Managers;

namespace TBChestTracker.Pages.ClanmatesValidation
{

    public class ClanmatesValidationProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public ClanmatesValidationProgress() { }
        public ClanmatesValidationProgress(string msg, double progress)
        {
            Message = msg;
            Progress = progress;
            ProgressStr = $"{Progress}%";
        }

        private string _Message = String.Empty;
        public string Message
        {
            get => _Message;
            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private string _progressStr = String.Empty;
        public string ProgressStr
        {
            get => _progressStr;
            set
            {
                _progressStr = value;
                OnPropertyChanged(nameof(ProgressStr));
            }
        }
    }

    public partial class ClanmatesValidationProcessingPage : Page
    {
        public ClanmatesValidationProgress ClanmatesValidationProgress { get; private set; }

        private Progress<ClanmatesValidationProgress> _progress;

        public ClanmatesValidationProcessingPage()
        {
            InitializeComponent();
            ClanmatesValidationProgress = new ClanmatesValidationProgress();

            this.DataContext = ClanmatesValidationProgress;
        }
        private void UpdateProgress(string message, double progress)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ClanmatesValidationProgress.Message = message;
                ClanmatesValidationProgress.Progress = progress;
                ClanmatesValidationProgress.ProgressStr = $"{progress}%";
            }));
        }

        private async Task ValidateClanmates(IProgress<ClanmatesValidationProgress> progress)
        {
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            double processedClanmates = 0;
            double totalClanmates = clanmates.Count;
            foreach (var clanmate in clanmates)
            {
                var percent = Math.Round((processedClanmates / totalClanmates) * 100.0);
                var _progress = new ClanmatesValidationProgress($"Validating {clanmate.Name}...", percent);
                progress.Report(_progress);
                
                processedClanmates++;
                await Task.Delay(200);
            }
            var _progressComplete = new ClanmatesValidationProgress($"Validating Completed...", 100);
            progress.Report(_progressComplete);
        }
        private async Task BeginValidatingClanmates()
        {
            _progress = new Progress<ClanmatesValidationProgress>();
            _progress.ProgressChanged += (s, e) =>
            {
                UpdateProgress(e.Message, e.Progress);
            };
            await ValidateClanmates(_progress);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BeginValidatingClanmates();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
