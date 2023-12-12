using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using TBChestTracker.ClanData;
using TBChestTracker.Managers;
using Windows.UI.Xaml.Shapes;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanManagementWindow.xaml
    /// </summary>
    public partial class ClanManagementWindow : Window
    {
        ObservableCollection<Clan> clans { get; set; }
        public MainWindow mainWindow { get; set; }  
        public ClanManagementWindow()
        {
            InitializeComponent();
            if(clans == null )
            {
                clans = new ObservableCollection<Clan>();

                //-- build clans 
                var clan_default_folder = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath;
                var clan_directories = System.IO.Directory.GetDirectories(clan_default_folder);
                foreach( var directory in clan_directories )
                {
                    Clan clan = new Clan();
                    clan.FolderPath = directory;

                    var path = $@"{directory}\";
                    var clandatabase = Directory.GetFiles(path, "*.cdb");
                    using (StreamReader sr = File.OpenText(clandatabase[0]))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        ClanDatabase clandb = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));
                        if(clandb != null)
                        {
                            clan.Name = clandb.Clanname;
                        }
                        clandb = null;
                        sr.Close();
                    }
                    var clanmates_file = $@"{directory}\db\clanmates.db";
                    using (TextReader reader = new StreamReader(clanmates_file))
                    {
                        string data = reader.ReadToEnd();
                        if (data.Contains("\r\n"))
                        {
                            data = data.Replace("\r\n", ",");
                        }
                        else
                        {
                            data = data.Replace("\n", ",");
                        }

                        data = data.Substring(0, data.LastIndexOf(","));

                        string[] dataCollection = data.Split(',');
                        clan.Members = dataCollection.Count(c => !String.IsNullOrEmpty(c));
                        reader.Close();
                        dataCollection = null;
                    }
                    clans.Add(clan);    
                }
            }
            ClansListView.ItemsSource = clans;  
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            clans.Clear();
            clans = null;
        }

        private void DeleteClanBtn_Click(object sender, RoutedEventArgs e)
        {
            var warn_deletion = MessageBox.Show("This will delete every file associated with the clan database. Including any clan members, previous chest count, etc... Continue?", "Delete Clan Database", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
            if(warn_deletion == MessageBoxResult.Yes)
            {
                //--- delete folder.
                //--- and remove from recent list.
                var selectedIndex = ClansListView.SelectedIndex;
                var clan = clans[selectedIndex];
                try
                {
                    //Debug.WriteLine($"--- DELETING CLAN DIRECTORY: {clan.FolderPath}");
                    Directory.Delete(clan.FolderPath, true);

                    //--- remove from recent databases.

                    //--- remove from clan manager
                    clans.Remove(clan);
                }
                catch( Exception ex )
                {
                    //Debug.WriteLine($"--- EXCEPTION CAUGHT: {ex.Message}");
                }
            }
        }

        private void NewClanBtn_Click(object sender, RoutedEventArgs e)
        {
            NewClanDatabaseWindow newClanDatabaseWindow = new NewClanDatabaseWindow();
            if (newClanDatabaseWindow.ShowDialog() == true)
            {
                GlobalDeclarations.hasNewClanDatabaseCreated = true;
                mainWindow.AppTitle = $"TotalBattle Chest Tracker v0.1.0 - {ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}";
                this.Close();
            }
        }
    }
}
