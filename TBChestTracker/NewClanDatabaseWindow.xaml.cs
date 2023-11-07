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
            this.DataContext = ClanDatabaseManager.ClanDatabase;
        }
        private void CreateClanFolders(Action<bool> result)
        {
            var clanname = ClanDatabaseManager.ClanDatabase.Clanname;
            var mainpath = ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath;
            if (String.IsNullOrEmpty(clanname) || clanname.Length < 3) 
            {
                MessageBox.Show("Clan name must be more than three characters.");
                result(false);
                return;
            }
            ClanDatabaseManager.ClanDatabase.ClanFolderPath = $"{mainpath}{clanname}";
            ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder = $"{mainpath}{clanname}\\db";
            ClanDatabaseManager.ClanDatabase.ClanChestReportFolderPath = $"{mainpath}{clanname}\\reports";
            ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath = $"{mainpath}{clanname}\\exports";
            ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath = $"{mainpath}{clanname}\\backups";
            
            System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.ClanFolderPath);
            System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder);
            System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.ClanChestReportFolderPath);
            System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.ClanChestDatabaseExportFolderPath);
            System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath);
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
