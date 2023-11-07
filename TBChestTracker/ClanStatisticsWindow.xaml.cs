using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ClanStatisticsWindow.xaml
    /// </summary>
    /// 

  
    public partial class ClanStatisticsWindow : Window, INotifyPropertyChanged
    {
        public ClanChestManager ChestManager { get; set; }
        bool isready { get; set; }
        public ObservableCollection<ClanStatisticData> ClanStatisticData { get; set; }

        private DateTime m_StartDate = new DateTime();
        private DateTime m_EndDate = new DateTime();
        public DateTime StartDate
        {
            get => (DateTime)m_StartDate;
            set
            {
                m_StartDate = value;
                OnPropertyChanged(nameof(StartDate));   
            }
        }
        public DateTime EndDate
        {
            get => (DateTime)m_EndDate;
            set
            {
                m_EndDate = (DateTime)value;
                OnPropertyChanged(nameof(EndDate));
            }
        }
        

        private bool m_bShowCommonCryptsTotal = true;
        private bool m_bShowRareCryptsTotal = true;
        private bool m_bShowEpicCryptsTotal = true;
        private bool m_bShowCitadelsTotal = true;
        private bool m_bShowArenasTotal = true;
        private bool m_bShowUnionTriumphTotal = true;
        private bool m_bShowVaultAncientsTotal = true;
        private bool m_bShowHeroicsTotal = true;
        private bool m_bShowAncientChestsTotal = true;
        private bool m_bShowBanksTotal = true;
        private bool m_bShowStoryChestsTotal = true;
        private bool m_bShowAll = true;
        private bool m_bShowTotal = true;

        #region PropertyChanged Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            Debug.WriteLine($"{propertyName} changed");
        }
        #endregion

        #region Visibility Variables
        public bool bShowCommonCryptsTotal
        {
            get => m_bShowCommonCryptsTotal;
            set
            {
                m_bShowCommonCryptsTotal = value;
                OnPropertyChanged(nameof(bShowCommonCryptsTotal));
            }
        }
        public bool bShowRareCryptsTotal
        {
            get => m_bShowRareCryptsTotal;
            set
            {
                m_bShowRareCryptsTotal = value;
                OnPropertyChanged(nameof(bShowRareCryptsTotal));
            }
        }
        public bool bShowEpicCryptsTotal
        {
            get => m_bShowEpicCryptsTotal;
            set
            {
                m_bShowEpicCryptsTotal = value;
                OnPropertyChanged(nameof(bShowEpicCryptsTotal));
            }
        }
        public bool bShowCitadelsTotal
        {
            get => m_bShowCitadelsTotal;
            set
            {
                m_bShowCitadelsTotal = value;
                OnPropertyChanged(nameof(bShowCitadelsTotal));
            }
        }
        public bool bShowArenasTotal
        {
            get => m_bShowArenasTotal;
            set
            {
                m_bShowArenasTotal = value;
                OnPropertyChanged(nameof(bShowArenasTotal));
            }
        }
        #endregion

        public bool bShowUnionTriumphsTotal
        {
            get => m_bShowUnionTriumphTotal;
            set
            {
                m_bShowUnionTriumphTotal = value;
                OnPropertyChanged(nameof(bShowUnionTriumphsTotal));
            }
        }
        public bool bShowVaultAncientsTotal
        {
            get => m_bShowVaultAncientsTotal;
            set
            {
                m_bShowCitadelsTotal = value;
                OnPropertyChanged(nameof(bShowVaultAncientsTotal));
            }
        }
        public bool bShowHeroicsTotal
        {
            get => m_bShowHeroicsTotal;
            set
            {
                m_bShowHeroicsTotal = value;
                OnPropertyChanged(nameof(bShowHeroicsTotal));
            }
        }
        public bool bShowAncientChestsTotal
        {
            get => m_bShowAncientChestsTotal;
            set
            {
                m_bShowAncientChestsTotal = value;
                OnPropertyChanged(nameof(bShowAncientChestsTotal));
            }

        }
        public bool bShowStoryChestsTotal
        {
            get => m_bShowStoryChestsTotal;
            set
            {
                m_bShowStoryChestsTotal = value;
                OnPropertyChanged(nameof(bShowStoryChestsTotal));
            }
        }
        public bool bShowBanksTotal
        {
            get => m_bShowBanksTotal;
            set
            {
                m_bShowBanksTotal = value;
                OnPropertyChanged(nameof(bShowBanksTotal));
            }
        }
        public bool bShowTotal
        {
            get => m_bShowTotal;
            set
            {
                m_bShowTotal = value;
                OnPropertyChanged(nameof(bShowTotal));  
            }
        }
        public bool bShowAll
        {
            get => m_bShowAll;
            set
            {
                m_bShowAll = value;
                OnPropertyChanged(nameof(bShowAll));
            }
        }

        public ClanStatisticsWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ClanStatisticData = new ObservableCollection<ClanStatisticData>();
            isready = false;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            ClanStatsListView.ItemsSource = ClanStatisticData;

            CollectionViewSource.GetDefaultView(ClanStatsListView.ItemsSource).Filter = Filter_Clanmate_Results;
            DateTime currentDate = DateTime.Now;
            var firstDateEntry = ChestManager.ClanChestDailyData.First().Key;
            var firstDate = DateTime.Parse(firstDateEntry);
            var weekEndDate = firstDate.AddDays(7);
            firstDate = firstDate.AddDays(-1);

            var lastDateEntry = ChestManager.ClanChestDailyData.Last().Key;
            var lastDate = DateTime.Parse(lastDateEntry).AddDays(1);

            var todayDate = DateTime.Now;

            DateSelection.BlackoutDates.Add(new CalendarDateRange(new DateTime(1900, 1, 1), firstDate));
            DateSelection.BlackoutDates.Add(new CalendarDateRange(lastDate, new DateTime(9999, 1, 1)));
            DateSelection.DisplayDate = DateTime.Now;
            StartDate = firstDate.AddDays(1);

            EndDateSelection.BlackoutDates.Add(new CalendarDateRange(new DateTime(1900, 1, 1), firstDate));
            EndDateSelection.BlackoutDates.Add(new CalendarDateRange(lastDate, new DateTime(9999, 1, 1)));
            EndDateSelection.DisplayDate = lastDate;
            EndDate = lastDate.AddDays(-1);
            isready = true;
            LoadDateEntry(StartDate, EndDate);
        }

        bool Filter_Clanmate_Results(object item)
        {
            var filtered_name = FilterClanmate.Text;
            if (String.IsNullOrEmpty(filtered_name))
                return true;
            else if (String.IsNullOrWhiteSpace(filtered_name)) return true;
            else
            {
                var clanmate = (ClanStatisticData)item;
                return clanmate.Clanmate.StartsWith(filtered_name, StringComparison.CurrentCultureIgnoreCase);

            }
        }
        private void LoadDateEntry(DateTime startdate, DateTime enddate)
        {
            try
            {
                if(ClanStatisticData !=  null) 
                    ClanStatisticData.Clear();
                //var dateEntry = ChestManager.ClanChestDailyData.Where(d => d.Key.Equals(date)).ToList()[0];
                var numDays = (enddate - startdate).TotalDays;
                var dateRange = ChestManager.ClanChestDailyData.ToList().GetRange(0, (int)numDays + 1);

                foreach (var date in dateRange)
                {
                    foreach (var entry in date.Value)
                    {
                        int commoncryptstotal, rarecryptstotal, epiccryptstotal, citadelsstotal,
                          arenastotal, uniontriumphstotal, vaultancienttotal, ancientchests, heroicstotal, bankstotal, storycheststotal, total, otherTotal;
                        commoncryptstotal = rarecryptstotal = epiccryptstotal = citadelsstotal = arenastotal =
                    heroicstotal = bankstotal = uniontriumphstotal = vaultancienttotal = ancientchests = storycheststotal = total = otherTotal = 0;

                        var clanmate = entry.Clanmate;
                        //var chests = dateEntry.Value.Where(name => name.Clanmate.Equals(clanmate)).Select(chest => chest.chests).ToList()[0];
                        var chests = date.Value.Where(name => name.Clanmate.Equals(clanmate, StringComparison.CurrentCultureIgnoreCase)).Select(chest => chest.chests).ToList()[0];
                       
                        if (chests != null)
                        {
                            commoncryptstotal = chests.Where(common => common.Type == ChestType.COMMON).Count();
                            rarecryptstotal = chests.Where(common => common.Type == ChestType.RARE).Count();
                            epiccryptstotal = chests.Where(common => common.Type == ChestType.EPIC).Count();
                            citadelsstotal = chests.Where(common => common.Type == ChestType.CITADEL).Count();
                            arenastotal = chests.Where(common => common.Type == ChestType.ARENA).Count();
                            heroicstotal = chests.Where(common => common.Type == ChestType.HEROIC).Count();
                            uniontriumphstotal = chests.Where(common => common.Type == ChestType.UNION_TRIUMPH).Count();
                            vaultancienttotal = chests.Where(common => common.Type == ChestType.VAULT).Count();
                            bankstotal = chests.Where(common => common.Type == ChestType.BANK).Count();
                            ancientchests = chests.Where(common => common.Type == ChestType.ANCIENT_EVENT).Count();
                            storycheststotal = chests.Where(common => common.Type == ChestType.STORY).Count();
                            otherTotal = chests.Where(common => common.Type == ChestType.OTHER).Count();    
                            total = commoncryptstotal + rarecryptstotal + epiccryptstotal + citadelsstotal + arenastotal + uniontriumphstotal + heroicstotal + vaultancienttotal + bankstotal + storycheststotal + otherTotal;
                        }

                        bool alreadyExists = ClanStatisticData.Where(mate => mate.Clanmate.Equals(clanmate)).Count() > 0 ? true : false;
                        if (alreadyExists)
                        {
                            var updateStats = ClanStatisticData.Where(mate => mate.Clanmate.Equals(clanmate)).ToList()[0];
                            updateStats.CommonCryptsTotal += commoncryptstotal;
                            updateStats.RareCryptsTotal += rarecryptstotal;
                            updateStats.EpicCryptsTotal += epiccryptstotal;
                            updateStats.CitadelsTotal += citadelsstotal;
                            updateStats.ArenasTotal += arenastotal;
                            updateStats.HeroicsTotal += heroicstotal;
                            updateStats.UnionTriumphsTotal += uniontriumphstotal;
                            updateStats.VaultAncientsTotal += vaultancienttotal;    
                            updateStats.BanksTotal += bankstotal;
                            updateStats.AncientChestsTotal += ancientchests;
                            updateStats.StoryChestsTotal += storycheststotal;
                            updateStats.Total += total;
                        }
                        else
                            ClanStatisticData.Add(new TBChestTracker.ClanStatisticData(clanmate, commoncryptstotal, rarecryptstotal, epiccryptstotal, citadelsstotal,
                            arenastotal, uniontriumphstotal, vaultancienttotal, heroicstotal, ancientchests, storycheststotal, bankstotal, total));
                    }
                }
            }
            catch(Exception ex)
            {
                
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void DateSelection_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isready)
                LoadDateEntry(StartDate, EndDate);
        
        }

        private void FilterClanmate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ClanStatsListView.ItemsSource).Refresh();
        }

        private void ExportClanChest_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportWindow exportWindow = new ExportWindow();
            exportWindow.ShowDialog();
        }

        private void Close_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
