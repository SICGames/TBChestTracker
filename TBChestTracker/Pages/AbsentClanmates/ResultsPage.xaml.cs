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

namespace TBChestTracker.Pages.AbsentClanmates
{
    /// <summary>
    /// Interaction logic for ResultsPage.xaml
    /// </summary>
    public partial class ResultsPage : Page
    {
        public ResultsPage()
        {
            InitializeComponent();
            this.DataContext = AbsentClanmatesViewModel.Instance;   
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AbsentClanmatesViewModel.Instance.AbsentMessage = $"There are {AbsentClanmatesViewModel.Instance.AbsentClanmateList.Count()} clanmates that have been absent.";
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //-- Begin to delete those clanmates.
            if (AbsentClanmatesViewModel.Instance.AbsentClanmateList.Count() > 0)
            {
                ClanManager.Instance.ClanmateManager.CreateBackup();
                var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";


                var clanmatedatabaseFile = $@"{AppContext.Instance.LocalApplicationPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";

                foreach (var absentClanmate in AbsentClanmatesViewModel.Instance.AbsentClanmateList)
                {
                    ClanManager.Instance.ClanChestManager.RemoveChestData(absentClanmate);
                    ClanManager.Instance.ClanmateManager.Remove(absentClanmate);
                }

                ClanManager.Instance.ClanmateManager.Save();
            }
            var wnd = Window.GetWindow(this) as AbsentClanmatesWindow;
            wnd.DialogResult = true;
            wnd.Close();
        }

        private void RemoveClanmateButton_Click(object sender, RoutedEventArgs e)
        {
            var removelist = new List<string>();

            foreach(var selecteditem in AbsentClanmatesListView.SelectedItems)
            {
                removelist.Add(selecteditem.ToString());    
            }
            foreach(var removeItem in removelist)
            {
                AbsentClanmatesViewModel.Instance.AbsentClanmateList.Remove(removeItem);
            }
        }
    }
}
