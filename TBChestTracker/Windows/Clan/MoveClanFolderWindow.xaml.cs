using com.HellStormGames.Diagnostics;
using Emgu.CV.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for MoveClanFolderWindow.xaml
    /// </summary>
    public partial class MoveClanFolderWindow : Window, INotifyPropertyChanged
    {

        public string OldClanRootFolder { get; set; }
        public string NewClanRootFolder {  get; set; }

        private string _status = "";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));  
            }
        }

        private Decimal _progress = 0;
        public Decimal Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        private decimal _max;
        public decimal Max
        {
            get => _max;
            set
            {
                _max = value;
                OnPropertyChanged(nameof(Max)); 
            }
        }
        private string _percent = "0%";
        public string Percent
        {
            get => _percent;
            set
            {
                _percent = value;
                OnPropertyChanged(nameof(Percent));
            }
        }

        private bool bAborted = false;

        private CancellationTokenSource _CancellationTokenSource;

        public MoveClanFolderWindow(string src, string dest)
        {
            InitializeComponent();
            OldClanRootFolder = src;
            NewClanRootFolder = dest;
            this.DataContext = this;

        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) 
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            if (AbortButton.Text == "Abort")
            {
                bAborted = true;
                this.Close();
            }
            else if(AbortButton.Text == "Close")
            {
                this.Close();
            }
        }

        private void UpdateUI(string status, int current, int total, bool isIndeterminate = false, bool aborted = false, bool completed = false)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Status = status;
                Progress = current;
                Progressbar01.IsIndeterminate = isIndeterminate;
                Max = total;
                var percentage = Math.Round(((decimal)current / (decimal)total) * 100) + "%";
                Percent = percentage;
                if(aborted)
                {
                    bAborted = true;
                    AbortButton.Text = "Close";
                }
                if (completed)
                {
                    AbortButton.Text = "Close";
                    bAborted = false;
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }
        private async Task CopyDirectoryAsync(string sourceDir, string destinationDir, bool recursive, CancellationToken token) 
        {
            Debug.WriteLine("Copying files....");

            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (Directory.Exists(destinationDir) == false)
            {
                Directory.CreateDirectory(destinationDir);
            }

            var index = 0;
            bAborted = false;

            foreach(var file in Directory.EnumerateFiles(sourceDir))
            {
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    var destFile = $"{destinationDir}{file.Substring(file.LastIndexOf("\\"))}";
                    var status = $"Copying - {destFile}";
                    var current = index;
                    var total = Directory.EnumerateFiles(sourceDir).Count();
                    UpdateUI(status, current, total);
                    using (FileStream destinationStream = File.OpenWrite(destFile))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                if(token.IsCancellationRequested)
                {
                    bAborted = true;
                    break;
                }

                index++;
                await Task.Delay(20);
            }

            index = 0;
            if(bAborted)
            {
                UpdateUI("Aborted!", 0, 0, false, true);

                await Task.Delay(3000);

                this.Close();
            }

            if(recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    await CopyDirectoryAsync(subDir.FullName, newDestinationDir, true, token);
                }
            }

            Debug.WriteLine("Copying Complete");
            UpdateUI("Completed", 100, 100, false, false, true);
        }

        private Task BeginCopyingTask(string oldFolder, string newFolder, CancellationToken token)
        {
            return Task.Run(() =>
            {
                try
                {
                    CopyDirectoryAsync(oldFolder, newFolder, true, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _CancellationTokenSource.Cancel();  
                }
            });
        }

        private async void BeginMovingClanDatabasesTask(string oldFolder, string newFolder, CancellationToken token)
        {
            await BeginCopyingTask(oldFolder, newFolder, token);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _CancellationTokenSource = new CancellationTokenSource();
            //-- we need to ensure user select only drive letter
            if(Directory.Exists(OldClanRootFolder) == false)
            {
                Loggio.Warn("Moving Clans", "Ran into a issue while attempting to move clans.");
                UpdateUI($"Failed To Move Clans. {OldClanRootFolder} may not exist.", 0, 1, false, true, false);
                return;
            }

            BeginMovingClanDatabasesTask(OldClanRootFolder, NewClanRootFolder, _CancellationTokenSource.Token); 
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = this.bAborted == true ? false : true;
        }
    }
}
