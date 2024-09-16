using System;
using System.Collections.Generic;
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
using TBChestTracker.ViewModels;

namespace TBChestTracker.Pages.ClanmatesValidation
{
    /// <summary>
    /// Interaction logic for RepairClanmatesPage.xaml
    /// </summary>
    public partial class RepairClanmatesPage : Page
    {
        public ClanmatesRepairProgress ClanmatesRepairProgress { get; private set; }
        private Progress<ClanmatesRepairProgress> _progress { get; set; }

        private ClanmateValidationWindow ValidationWindow { get; set; }

        private void UpdateProgress(string message, double progress)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ClanmatesRepairProgress.Message = message;
                ClanmatesRepairProgress.Progress = progress;
                ClanmatesRepairProgress.ProgressStr = $"{progress}%";

            }));
        }
        public RepairClanmatesPage()
        {
            InitializeComponent();
            ClanmatesRepairProgress = new ClanmatesRepairProgress();

            this.DataContext = ClanmatesRepairProgress;

        }

        private async Task PerformRepairing(IProgress<ClanmatesRepairProgress> progress)
        {
            ValidationWindow = Window.GetWindow(this) as ClanmateValidationWindow;
            var dailyclanchests = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
            var verifiedClanmates = ValidationWindow.VerifiedClanmatesViewModel.VerifiedClanmates;
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;

            var affectedClanmates = ValidationWindow.affectedClanmates;
            double processed = 0;
            double total = affectedClanmates.Count;

            foreach (var affectedClanmate in affectedClanmates)
            {
                var percent = Math.Round((processed / total) * 100.0);
                var _progress = new ClanmatesRepairProgress($"Repairing {affectedClanmate.Name}...", percent);
                progress.Report(_progress);

                /*
                  Begin repair process; 
                */


                processed += 1;
            }
        }
        private async Task BeginRepairProcess()
        {
            _progress = new Progress<ClanmatesRepairProgress>();
            _progress.ProgressChanged += (s, e) =>
            {
                UpdateProgress(e.Message, e.Progress);
            };
            await PerformRepairing(_progress);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BeginRepairProcess();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
