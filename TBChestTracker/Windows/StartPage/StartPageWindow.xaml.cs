using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.IO;
using TBChestTracker.Managers;
using TBChestTracker.UI;
using System.Runtime.CompilerServices;
using System.Windows.Shell;
using com.KonquestUI.Controls;
using System.Diagnostics;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for StartPageWindow.xaml
    /// </summary>
    public partial class StartPageWindow : Window
    {
        //public ObservableCollection<RecentClanDatabase> RecentClanDatabases { get; set; }
        
        public MainWindow MainWindow { get; set; }
        public StartPageWindow()
        {
            InitializeComponent();
            this.DataContext = RecentlyOpenedListManager.Instance;
            //RecentClanDatabases = new ObservableCollection<RecentClanDatabase>();   
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowWindow();
            this.Close();
        }

        private void Lvi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as RecentClanDatabase;

            MainWindow.LoadReentFile(item.FullClanRootFolder, result =>
            {
                MainWindow.ShowWindow();
                this.Close();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RecentlyOpenedListManager.Instance.Build();

            recentFilesView.ItemsSource = RecentlyOpenedListManager.Instance.RecentClanDatabases;
            if (RecentlyOpenedListManager.Instance.RecentClanDatabases.Count > 0)
            {
                ClearRecentListBtn.IsEnabled = true;
            }
            else
            {
                ClearRecentListBtn.IsEnabled = false;
            }
            //LoadRecentFilesList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            recentFilesView.ItemsSource = null;
            MainWindow = null;
        }

        private void NewClanWizard()
        {
            ClanWizardWindow clanWizardWindow = new ClanWizardWindow();
            clanWizardWindow.Show();
        }

        private void ImportClanDatabase()
        {
            MainWindow.ImportClanDatabase(result =>
            {
                MainWindow.ShowWindow();
                this.Close();
            });
        }
        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            var button = e.OriginalSource as FancyButton;
            string action = button.Tag.ToString();
            switch (action)
            {
                case "NEW":
                    MainWindow.CreateNewClan(result =>
                    {
                        if (result)
                        {
                            MainWindow.ShowWindow();
                            this.Close();
                        }
                    });
                    break;
                case "LOAD":
                    MainWindow.ShowLoadClanWindow(result =>
                    {
                        if (result)
                        {
                            MainWindow.ShowWindow();
                            this.Close();
                        }
                    });
                    break;
                case "NEW_WIZARD":
                    NewClanWizard();
                    this.Close();
                    break;
                case "IMPORT":
                    ImportClanDatabase();
                    break;
                default: break;
            }
        }

        private void ClearRecentListBtn_Click(object sender, RoutedEventArgs e)
        {
            RecentlyOpenedListManager.Instance.Delete();
            if (MainWindow.RecentlyOpenedParent.Items.Count > 0)
            {
                MainWindow.RecentlyOpenedParent.Items.Clear();
            }
            ClearRecentListBtn.IsEnabled = false;   
        }

        private void PatreonBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/TotalBattleGuide");
        }
    }
}
