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
using TBChestTracker.ViewModels;

namespace TBChestTracker.Pages.ClanmatesValidation
{
    public partial class ClanmatesValidationProcessingPage : Page
    {

        private Dictionary<string, int> ClanmateFineTuningList = new Dictionary<string, int>();
        public ClanmatesValidationProgress ClanmatesValidationProgress { get; private set; }
        private Progress<ClanmatesValidationProgress> _progress;

        private int errors = 0;

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

                            if (similiarity >= parentWindow.ClanmateSimilarity)
                            {
                                //-- we have a winner.
                                try
                                {
                                    Debug.WriteLine($"{clanmate.Name} and {verifedClanmate.Name} similiarity => {similiarity}%");
                                    var bExists = parentWindow.affectedClanmates.Where(c => c.Name.ToLower().Equals(clanmate.Name.ToLower())).FirstOrDefault();

                                    //-- if somehow this exists then there will be a collision.
                                    
                                    if (bExists == null)
                                    {
                                        var af = new AffectedClanmate();
                                        af.Name = verifedClanmate.Name;

                                        parentWindow.affectedClanmates.Add(af);
                                    }

                                    //-- _af throws ArgumentOutOfRangeException.
                                    var _af = parentWindow.affectedClanmates.Select(c => c).Where(cn => cn.Name.Equals(verifedClanmate.Name)).ToList();

                                    if (_af != null && _af.Count > 0)
                                    {
                                        _af[0].AddAlias(clanmate.Name);
                                        ignoreList.Add(clanmate.Name);
                                    }
                                }
                                catch (Exception e)
                                {
                                    throw new Exception(e.Message);
                                }
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
