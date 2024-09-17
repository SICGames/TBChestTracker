using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            var affectedClanmates = ValidationWindow.affectedClanmates;
            double processed = 0;
            double total = affectedClanmates.Count;

            var previous_chestdata = new Dictionary<string, List<ClanChestData>>();
          
          

            foreach (var affectedClanmate in affectedClanmates)
            {
                var percent = Math.Round((processed / total) * 100.0);
                var _progress = new ClanmatesRepairProgress($"Repairing {affectedClanmate.Name}...", percent);
                
                progress.Report(_progress);

                /*
                  Begin repair process; 
                */

                var affectedAliases = affectedClanmate.Aliases;
                foreach (var alias in affectedAliases)
                {
                    var aliasName = alias.Name; 

                    foreach(var dailychestdata in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
                    {
                        var alias_clanchestdata = dailychestdata.Value.Where(chestdata => chestdata.Clanmate.Equals(aliasName, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        var temp_clanchest = new List<ClanChestData>();
                        foreach(var alias_chestdata in alias_clanchestdata)
                        {
                            temp_clanchest.Add(new ClanChestData(affectedClanmate.Name, alias_chestdata.chests));
                            previous_chestdata.Add(dailychestdata.Key, temp_clanchest);
                        }
                    }
                    
                    ClanManager.Instance.ClanmateManager.Remove(aliasName);
                    ClanManager.Instance.ClanChestManager.RemoveChestData(aliasName);
                }

                //-- something's happening with the proper clanmates not getting chests.
                foreach (var dailychestdata in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
                {
                    var chestsdata = dailychestdata.Value.Where(name => name.Clanmate.Equals(affectedClanmate.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    foreach (var chestdata in chestsdata)
                    {
                        foreach(var affectedData in previous_chestdata.ToList())
                        {
                            if (dailychestdata.Key == affectedData.Key)
                            {
                                var chests = affectedData.Value.Select(c => c.chests).ToList()[0];
                                if(chestdata.chests == null)
                                {
                                    chestdata.chests = new List<Chest>();
                                }
                                if (chests != null)
                                {
                                    chestdata.chests.AddRange(chests);
                                }
                            }
                        }
                    }
                }

                await Task.Delay(100);
                previous_chestdata.Clear();
                processed += 1;
            }
            
            var _completed = new ClanmatesRepairProgress("Everything is repaired...", 100);
            progress.Report(_completed);
            await Task.Delay(500);

            previous_chestdata.Clear();
            previous_chestdata = null;
        }

        private async Task RemoveInvalidClanmates(IProgress<ClanmatesRepairProgress> progress)
        {
            var verifiedClanmates = ValidationWindow.VerifiedClanmatesViewModel.VerifiedClanmates;
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();
            var dirtyClanmates = new List<string>();

            //-- clanmates that do exist are being removed. Whereas they shouldn't.
            //-- Those that being removed may have not been added to clanmates because they don't do shit.
            foreach (var clanmate in clanmates)
            {
                var oClanmate = verifiedClanmates.Select(x => x).Where(x => x.Name.ToLower().Equals(clanmate.Name.ToLower())).FirstOrDefault();

                if(oClanmate == null)   
                {
                    dirtyClanmates.Add(clanmate.Name);
                }
            }

            var dailyclanchests = ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList();

            double processed = 0;
            double total = dirtyClanmates.Count;

            foreach (var dirtyClanmate in dirtyClanmates)
            {
                var percent = Math.Round((processed / total) * 100.0);
                var _progres = new ClanmatesRepairProgress($"Removing Invalid Clanmate {dirtyClanmate}...", percent);
                progress.Report(_progres);
                ClanManager.Instance.ClanmateManager.Remove(dirtyClanmate);
                ClanManager.Instance.ClanChestManager.RemoveChestData(dirtyClanmate);
                processed++;
                await Task.Delay(100);
            }
        }

        private async Task FinishRepairing(Dictionary<string, int> lazyClanmates, IProgress<ClanmatesRepairProgress> progress)
        {
            var j = new F23.StringSimilarity.JaroWinkler();

            if (lazyClanmates.Count > 0)
            {
                foreach (var lazyClanmate in lazyClanmates.ToList())
                {
                    var lazyboy = lazyClanmate.Key;
                    foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
                    {
                        var similiarity = j.Similarity(lazyboy, clanmate.Name) * 100;
                        Debug.WriteLine($"{lazyClanmate.Key} and {clanmate.Name} have a {similiarity}% similiarity");
                        if(similiarity > 95)
                        {
                            lazyClanmates.Remove(lazyboy);
                            break;
                        }
                    }
                }
            }

            double processed = 0;
            double total = lazyClanmates.Count;
            await Task.Delay(250);

            foreach (var clanmate in lazyClanmates.Keys)
            {
                var percent = Math.Round((processed / total) * 100.0);
                var _addingback = new ClanmatesRepairProgress($"Adding {clanmate} to Clanmates...", percent);
                progress.Report(_addingback);
                ClanManager.Instance.ClanmateManager.Add(clanmate);
                processed++;
                await Task.Delay(100);
            }
        }
        private async Task<Dictionary<string, int>> CleanupLazyList(Dictionary<string, int> lazyClanmates, IProgress<ClanmatesRepairProgress> progress)
        {
            double processed = 0;
            double total = lazyClanmates.Count;
            var result = lazyClanmates;

            foreach (var lazyclanmate in result.ToList())
            {
                if (lazyclanmate.Value > 0)
                {
                    var percent = Math.Round((processed / total) * 100.0);
                    var _cleanup = new ClanmatesRepairProgress($"Removing {lazyclanmate.Key} from Lazy Clanmates List...", percent);
                    progress.Report(_cleanup);
                    result.Remove(lazyclanmate.Key);
                    processed++;
                    await Task.Delay(100);
                }
            }

            return result;
        }
        private async Task ProcessLazyClanmatesList(Dictionary<string,int> lazyClanmates, IProgress<ClanmatesRepairProgress> progress)
        {
            double processed = 0;
            double chesttotal = 0;

            var _totalChestCount = new ClanmatesRepairProgress($"Calculating Total Chest Count...", 0);
            progress.Report(_totalChestCount);
            await Task.Delay(500);

            foreach (var dailyclanchest in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
            {
                var dailyChestData = dailyclanchest.Value.ToList();
                foreach (var chestdata in dailyChestData)
                {
                    chesttotal += chestdata.chests != null ? chestdata.chests.Count() : 0;
                }
            }

            var _totalChestCountCompleted = new ClanmatesRepairProgress($"Completed Total Chest Count...", 0);
            progress.Report(_totalChestCountCompleted);
            await Task.Delay(500);

            processed = 0;
            double total = chesttotal;
            
            foreach (var dailyclanchest in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
            {
                var dailyChestData = dailyclanchest.Value.ToList();
                
                foreach (var chestdata in dailyChestData)
                {
                    var percent = Math.Round((processed / total) * 100.0);
                    var _processinglazylist = new ClanmatesRepairProgress($"Processing Lazy Clanmates List...", percent);
                    progress.Report(_processinglazylist);

                    lazyClanmates[chestdata.Clanmate] += chestdata.chests != null ? chestdata.chests.Count() : 0;
                    
                    processed++;
                }
                await Task.Delay(100);
            }
        }
        private async Task<Dictionary<string,int>> CreateLazyList(ObservableCollection<VerifiedClanmate> verifiedClanmates, IProgress<ClanmatesRepairProgress> progress)
        {
            var lazyClanmates = new Dictionary<string, int>();
            double processed = 0;
            double total = verifiedClanmates.Count;

            foreach (var verifiedClanmate in verifiedClanmates.ToList())
            {
                double percent = Math.Round((processed / total) * 100.0);
                var _lazylist = new ClanmatesRepairProgress($"Initializing Lazy Clanmates List...", percent);
                progress.Report(_lazylist);

                try
                {
                    lazyClanmates.Add(verifiedClanmate.Name, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                await Task.Delay(100);
                processed++;
            }

            var _lazylistcomplete = new ClanmatesRepairProgress($"Initialized Lazy Clanmates List...", 100);
            progress.Report(_lazylistcomplete);
            await Task.Delay(500);

            return lazyClanmates;
        }

        private async Task CreateClanmatesBackup(IProgress<ClanmatesRepairProgress> progress)
        {
            var backup_progres = new ClanmatesRepairProgress("Creating Clanmates.db Backup File.", 0);
            ClanManager.Instance.ClanmateManager.CreateBackup();
            progress.Report(backup_progres);

            await Task.Delay(1500);
        }
        private async Task CreateChestDataBackup(IProgress<ClanmatesRepairProgress> progress)
        {
            var backup_dailyclanchestdata = new ClanmatesRepairProgress("Creating ClanChests.db Backup File", 0);
            ClanManager.Instance.ClanChestManager.CreateBackup();
            progress.Report(backup_dailyclanchestdata);
            await Task.Delay(1500);
        }
        private async Task BeginRepairProcess()
        {
            _progress = new Progress<ClanmatesRepairProgress>();
            _progress.ProgressChanged += (s, e) =>
            {
                UpdateProgress(e.Message, e.Progress);
            };

            await CreateClanmatesBackup(_progress);
            await CreateChestDataBackup(_progress);

            await PerformRepairing(_progress);

            await RemoveInvalidClanmates(_progress);

            var lazyClanmates = await CreateLazyList(VerifiedClanmatesViewModel.Instance.VerifiedClanmates, _progress);
           
            await ProcessLazyClanmatesList(lazyClanmates, _progress); //-- causing a fuss with progress bar.

            lazyClanmates = await CleanupLazyList(lazyClanmates, _progress);

            await FinishRepairing(lazyClanmates, _progress);

            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            wnd.NavigateTo("Pages/ClanmatesValidation/CompletedPage.xaml");

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
