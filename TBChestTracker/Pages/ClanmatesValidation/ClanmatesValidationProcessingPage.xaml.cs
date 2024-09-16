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
        /*
         "Name": "Naida Il",
      "Aliases": [
        "Naida II",
        "Naida ١",
        "Naida ll",
        "Naida ١١",
        "Naida 11",
        "Naida l|"
        */


        private Dictionary<string, int> ClanmateFineTuningList = new Dictionary<string, int>();

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
            var parentWindow = Window.GetWindow(this) as ClanmateValidationWindow;

            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            var verifiedClanmates = parentWindow.VerifiedClanmatesViewModel.VerifiedClanmates;

            double processedClanmates = 0;
            double totalClanmates = clanmates.Count;

            var jacko = new F23.StringSimilarity.JaroWinkler();

            var ignoreList = new List<string>();

            //-- needs to be configured for verifiedClanmates and clanmates 

            foreach (var clanmate in clanmates)
            {
                var percent = Math.Round((processedClanmates / totalClanmates) * 100.0);
                var _progress = new ClanmatesValidationProgress($"Validating {clanmate.Name}...", percent);
                progress.Report(_progress);
                
                //-- now we begin to validate clanmates. 
                //-- We get first name. Bob then do a string similiarity against the rest of the clanmates. 
                //-- If there's a similiarity. We stuff Bob in affectedClanmates list. ALong with the matches.

                var bAlreadyIgnoreList = ignoreList.Select(c => c).Contains(clanmate.Name);
                if (bAlreadyIgnoreList == false)
                {
                    foreach (var verifedClanmate in verifiedClanmates)
                    {
                        if (clanmate.Name != verifedClanmate.Name)
                        {
                            var similiarity = jacko.Similarity(clanmate.Name, verifedClanmate.Name) * 100.0;

                            if (similiarity >= 92)
                            {
                                //-- we have a winner.
                                Debug.WriteLine($"{clanmate.Name} and {verifedClanmate.Name} similiarity => {similiarity}%");

                                var bExists = parentWindow.affectedClanmates.Where(c => c.Name.ToLower().Equals(clanmate.Name.ToLower())).FirstOrDefault();

                                if (bExists == null)
                                {
                                    var af = new AffectedClanmate();
                                    af.Name = verifedClanmate.Name;

                                    parentWindow.affectedClanmates.Add(af);
                                }

                                var _af = parentWindow.affectedClanmates.Select(c => c).Where(cn => cn.Name.Equals(verifedClanmate.Name)).ToList()[0];
                                _af.AddAlias(clanmate.Name);
                                ignoreList.Add(clanmate.Name);
                            }
                        }
                    }
                }
                processedClanmates++;
                await Task.Delay(50);
            }
            var _progressComplete = new ClanmatesValidationProgress($"Validating Completed...", 100);
            progress.Report(_progressComplete);
        }

        private async Task FineTuneResults(IProgress<ClanmatesValidationProgress> progress)
        {
            var dailyclanchests = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
            double total = dailyclanchests.Count;
            double processed = 0;
            var parentWindow = Window.GetWindow(this) as ClanmateValidationWindow;
            foreach (var k in dailyclanchests.Values)
            {
                total += k.Count();
            }

            //-- init finetuning chests
            foreach(var finetuning in parentWindow.affectedClanmates)
            {
                var n = finetuning.Name;
                ClanmateFineTuningList.Add(n, 0);
                foreach(var c in finetuning.Aliases)
                {
                    ClanmateFineTuningList.Add(c.Name, 0);
                }
            }

            var _validationProgress = new ClanmatesValidationProgress("Finetuning Results...", 0);
            progress.Report(_validationProgress);
           
            foreach (var dailyclanchest in dailyclanchests)
            {
                var dailychests = dailyclanchests[dailyclanchest.Key];
                foreach (var dailychest in dailychests)
                {

                    double percent = Math.Round((processed / total) * 100.0);
                    var _processed = new ClanmatesValidationProgress("Finetuning Results...", percent);
                    progress.Report(_processed);

                    foreach (var affectedClanmate in parentWindow.affectedClanmates)
                    {
                        if (dailychest.Clanmate.ToLower().Equals(affectedClanmate.Name.ToLower()))
                        {
                            var chestCount = dailychest.chests != null ? dailychest.chests.Count : 0;
                            if (ClanmateFineTuningList.ContainsKey(affectedClanmate.Name))
                            {
                                ClanmateFineTuningList[affectedClanmate.Name] += chestCount;
                            }
                        }
                        else
                        {
                            var isAffectedSibling = affectedClanmate.Aliases.Where(a => a.Name.ToLower().Equals(dailychest.Clanmate.ToLower())).FirstOrDefault();
                            if (isAffectedSibling != null)
                            {
                                var chestCount = dailychest.chests != null ? dailychest.chests.Count : 0;
                             
                                if (ClanmateFineTuningList.ContainsKey(isAffectedSibling.Name))
                                {
                                    ClanmateFineTuningList[isAffectedSibling.Name] += chestCount;
                                }
                            }

                        }
                    }
                    processed++;
                    await Task.Delay(5);
                }
                processed++;
            }
        }

        private async Task FinalizeResults(IProgress<ClanmatesValidationProgress> progress)
        {
            var _validationProgress = new ClanmatesValidationProgress("Finalizing Results...", 0);
            progress.Report(_validationProgress);
            var parentWindow = Window.GetWindow(this) as ClanmateValidationWindow;

            double total = ClanmateFineTuningList.Count;
            double processed = 0;

            var sortedResults = ClanmateFineTuningList.OrderByDescending(s => s.Value).ToList();
            foreach (var result in sortedResults)
            {
                foreach(var affected in parentWindow.affectedClanmates)
                {
                    if (result.Key.Equals(affected.Name) == true)
                    {
                        if (result.Value > 0)
                        {

                        }
                    }
                    else
                    {
                        var affectedAlias = affected.Aliases.Where(aa => aa.Name.Equals(result.Key)).FirstOrDefault();
                        if (affectedAlias != null)
                        {

                        }
                    }
                }
            }
        }
        private async Task BeginValidatingClanmates()
        {
            _progress = new Progress<ClanmatesValidationProgress>();
            _progress.ProgressChanged += (s, e) =>
            {
                UpdateProgress(e.Message, e.Progress);
            };
            await ValidateClanmates(_progress);
          
            // await FineTuneResults(_progress);
            //-- time to finalize results.
            // await FinalizeResults(_progress);

            //-- we're done
            await Task.Delay(500);
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            wnd.NavigateTo("Pages/ClanmatesValidation/ClanmatesValidationCompleted.xaml");
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
