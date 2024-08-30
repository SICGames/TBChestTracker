using Microsoft.Win32;
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
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanmateVerificationWindow.xaml
    /// </summary>
    public partial class ClanmateVerificationWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<string> _noClanmateExists;
        public ObservableCollection<string> noClanmateExistsList
        {
            get => _noClanmateExists;
            set
            {
                _noClanmateExists = value;
                OnPropertyChanged(nameof(noClanmateExistsList));
            }
        }
        private List<string> verified_clanmates {  get; set; }

        private string _verificationfile = String.Empty;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string VerificationFile
        {
            get => _verificationfile;
            set
            {
                _verificationfile = value;
                OnPropertyChanged(nameof(VerificationFile));
            }
        }

        private bool _createBackup = true;
        public bool CreateBackup
        {
            get => _createBackup;
            set
            {
                _createBackup = value;
                OnPropertyChanged(nameof(CreateBackup));
            }
        }
        public ClanmateVerificationWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void VerificationClanmateFile01_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}";
            openFileDialog.DefaultExt = ".db";
            openFileDialog.Filter = "Databases | *.db";
            if(openFileDialog.ShowDialog() == true)
            {
                VerificationFile = openFileDialog.FileName;
                using(var reader= System.IO.File.OpenText(VerificationFile))
                {
                    var data = reader.ReadToEnd();
                    if (data.Contains("\r\n"))
                    {
                        data = data.Replace("\r\n", ",");
                    }
                    else
                    {
                        data = data.Replace("\n", ",");
                    }
                    data = data.Substring(0, data.LastIndexOf(","));
                    if (verified_clanmates.Count > 0)
                        verified_clanmates.Clear();

                    verified_clanmates = data.Split(',').ToList();
                    VerifyButton.IsEnabled = true;
                    reader.Close();
                    reader.Dispose();

                }
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            var no_mates = clanmates.Where(m => !verified_clanmates.Contains(m.Name)).Select(name => name.Name).ToList();

            foreach(var no_mate in no_mates)
            {
                noClanmateExistsList.Add(no_mate);
            }

            if(noClanmateExistsList.Count > 0)
                ProceedBtn.IsEnabled = true;
            else 
                ProceedBtn.IsEnabled= false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            noClanmateExistsList = new ObservableCollection<string>();
            verified_clanmates = new List<string>();
            VerifyButton.IsEnabled = false;
            ProceedBtn.IsEnabled = false;
        }

        private void ProceedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CreateBackup)
            {
                var backup_clanmates_file = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\old_clanmates.bak";
                var backup_chestdata_file = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\old_clanchests.bak";
                ClanManager.Instance.ClanmateManager.Save(backup_clanmates_file);
                ClanManager.Instance.ClanChestManager.SaveData(backup_chestdata_file);
            }
            
            foreach(var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList())
            {
                bool exists = noClanmateExistsList.Contains(clanmate.Name, StringComparer.InvariantCultureIgnoreCase);
                if(exists)
                {
                    ClanManager.Instance.ClanChestManager.RemoveChestData(clanmate.Name);
                    ClanManager.Instance.ClanmateManager.Remove(clanmate.Name); 
                }
            }

            ClanManager.Instance.ClanmateManager.UpdateCount();

            this.Close();

        }
    }
}
