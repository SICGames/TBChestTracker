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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for StartPageWindow.xaml
    /// </summary>
    public partial class StartPageWindow : Window
    {
        public ObservableCollection<RecentClanDatabase> RecentClanDatabases { get; set; }

        //public ObservableCollection<String> RecentFiles;
        public MainWindow MainWindow { get; set; }
        public StartPageWindow()
        {
            InitializeComponent();
            RecentClanDatabases = new ObservableCollection<RecentClanDatabase>();   
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowWindow();
            this.Close();
        }

        private void LoadRecentFilesList()
        {
            if(File.Exists(AppContext.Instance.RecentOpenedClanDatabases))
            {
                var recentDatabase = new RecentDatabase();
                recentDatabase.Load();
                
                foreach(var recent in recentDatabase.RecentClanDatabases)
                {
                    RecentClanDatabases.Add(recent);
                }

                recentFilesView.ItemsSource = RecentClanDatabases;
            }

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
            LoadRecentFilesList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (RecentClanDatabases.Count > 0)
                RecentClanDatabases.Clear();

            RecentClanDatabases = null;
            MainWindow = null;
        }

        private void NewClanWizard()
        {
            ClanWizardWindow clanWizardWindow = new ClanWizardWindow();
            clanWizardWindow.Show();
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
                default: break;
            }
        }
    }
}
