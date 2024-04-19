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
using System.Windows.Shapes;
using Newtonsoft;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for NewClanDatabaseWindow.xaml
    /// </summary>
    public partial class NewClanDatabaseWindow : Window
    {

        public NewClanDatabaseWindow()
        {
            InitializeComponent();
            this.DataContext = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
        }
        private void CreateClanFolders(Action<bool> result)
        {
            var clanname = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname;
            var mainpath = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;
            if (String.IsNullOrEmpty(clanname) || clanname.Length < 3) 
            {
                MessageBox.Show("Clan name must be more than three characters.");
                result(false);
                return;
            }
            ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath = $"{mainpath}{clanname}";
            ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder = $"{mainpath}{clanname}\\db";
            ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestReportFolderPath = $"{mainpath}{clanname}\\reports";
            ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath = $"{mainpath}{clanname}\\exports";
            ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath = $"{mainpath}{clanname}\\backups";
            
            System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath);
            System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder);
            System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestReportFolderPath);
            System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath);
            System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath);
            result(true);
        }

        private void CreateClanDatabaseBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateClanFolders(result =>
            {
                if (result)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
