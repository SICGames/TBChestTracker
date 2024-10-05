using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TBChestTracker.Managers;
using TBChestTracker.ViewModels;

using Newtonsoft.Json;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for RestoreClanChestDataWindow.xaml
    /// </summary>
    public partial class RestoreClanChestDataWindow : Window
    {
        private ClanDatabaseBackupItem SelectedBackupItem = null;

        public RestoreClanDatabaseViewModel RestoreClanDatabaseViewModel { get; private set; } = new RestoreClanDatabaseViewModel();
        public RestoreClanChestDataWindow()
        {
            InitializeComponent();
            this.DataContext = RestoreClanDatabaseViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";
            
            var backupFolder = $@"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}\Clanchests";
            var di = new DirectoryInfo(backupFolder);
            if (di.Exists)
            {
                var backups = di.GetFiles("*.db");
                foreach (var backup in backups)
                {
                    var file = backup.Name;
                    var timestampstr = file.Substring(file.LastIndexOf("_") + 1);
                    timestampstr = timestampstr.Substring(0, timestampstr.LastIndexOf("."));
                    var timestamp = Double.Parse(timestampstr);
                    var d = timestamp.UnixTimeStampToDateTime();

                    var date = d.ToShortDateString();
                    var time = d.ToShortTimeString();
                    RestoreClanDatabaseViewModel.Add(date, time, backup.FullName);
                }
            }
            SelectedBackupItem = new ClanDatabaseBackupItem(String.Empty, string.Empty, string.Empty);

            RestoreButton.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RestoreClanDatabaseViewModel.Dispose();
            SelectedBackupItem = null;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var l = (ListView)sender;
            SelectedBackupItem = (ClanDatabaseBackupItem)l.SelectedValue;

            if(l.SelectedItems.Count > 0)
            {
                RestoreButton.IsEnabled = true;
            }
            else
            {
                RestoreButton.IsEnabled= false;
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBackupItem != null)
            {
                var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";

                var clanchestdbfile = $@"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
                var oldClanChestDBFile = clanchestdbfile.Substring(0,clanchestdbfile.LastIndexOf("."));
                oldClanChestDBFile += ".old";
                using(StreamWriter sw = File.CreateText(oldClanChestDBFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;    
                    serializer.Serialize(sw, ClanManager.Instance.ClanChestManager.ClanChestDailyData);
                    sw.Close();
                }

                try
                {
                    File.Delete(clanchestdbfile);
                }
                catch(IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                ClanManager.Instance.ClanChestManager.LoadBackup(SelectedBackupItem.File);
                ClanManager.Instance.ClanChestManager.SaveData();
                if (AppContext.Instance.IsClanChestDataCorrupted)
                {
                    AppContext.Instance.IsClanChestDataCorrupted = false;
                    AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                    AppContext.Instance.IsAutomationStopButtonEnabled = false;
                }
                CommandManager.InvalidateRequerySuggested();
                this.Close();   
            }
        }
    }
}
