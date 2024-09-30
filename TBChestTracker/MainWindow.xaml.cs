using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Emgu.CV.Cuda;
using System.Text;
using System.Diagnostics;
using System.Windows.Input;
using Emgu.CV.OCR;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using TBChestTracker.Helpers;
using TBChestTracker.Managers;
using System.Reflection;

using com.HellstormGames.ScreenCapture;
using com.HellstormGames.Imaging;
using com.HellstormGames.Imaging.Extensions;
using com.CaptainHook;
using com.KonquestUI.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Net;
using System.Net.Http;
using TBChestTracker.Engine;
using TBChestTracker.Properties;
using TBChestTracker.Resources;

using TBChestTracker.Effects;
using TBChestTracker.Automation;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region TessData Option
        //private TessDataConfig TessDataConfig { get; set; }
        #endregion
        #region Declarations
        System.Threading.Thread InputHookThread { get; set; }
        System.Threading.Thread AutomationThread { get; set; }
        private double CLANCHEST_IMAGE_BRIGHTNESS = 0.65d;
        CaptureMode CaptureMode { get; set; }
        int captureCounter = 0;
        public SettingsManager SettingsManager { get; set; }
        public ClanManager ClanManager { get; private set; }
        private CaptainHook CaptainHook { get; set; }
        public Snapture Snapture { get; private set; }

        //public AppContext appContext { get; private set; }
        
        ConsoleWindow consoleWindow { get; set; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        SettingsWindow SettingsWindow { get; set; }
        OCRWizardWindow OCRWizardWindow { get; set; }
        StartPageWindow startPageWindow {  get; set; }  
        ClanChestProcessResult clanChestProcessResult { get; set; }
        
        //RecentDatabase recentlyOpenedDatabases { get; set; }
        
        ApplicationManager applicationManager { get; set; }
        RecentlyOpenedListManager recentlyOpenedListManager { get; set; }
        Task AutomationTask { get; set; }

        ChestAutomation ChestAutomation { get; set; }
        #endregion

        #region PropertyChanged Event
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region MainWindow()
        public MainWindow()
        {
            //appContext = new AppContext();
            InitializeComponent();

            //TessDataConfig = new TessDataConfig(TessDataOption.Best);

            this.DataContext = AppContext.Instance;
            this.Closing += MainWindow_Closing;
            //recentlyOpenedDatabases = new RecentDatabase();
            consoleWindow = new ConsoleWindow();
        }
        #endregion
        #region Initializing functions
        private void InitCaptainHook()
        {

            CaptainHook = new CaptainHook();
            CaptainHook.onKeyboardMessage += CaptainHook_onKeyboardMessage;
            CaptainHook.onInstalled += CaptainHook_onInstalled;
            CaptainHook.Install();
        }
        
        private void InitSettingsTask()
        {
            Task t = new Task(new Action(async () =>
            {
                await InitSettings();
            }));
            t.Start();
            t.Wait();
        }
        
        //--- missing Settings.json file causes headaches for people. Need to fix this.
        private Task<SettingsManager> InitSettings()
        {
            return Task.Run(() =>
            {
                SettingsManager = new SettingsManager();

                com.HellStormGames.Logging.Console.Write("Settings Loaded.", com.HellStormGames.Logging.LogType.INFO);

                //-- init appContext
                AppContext.Instance.IsAutomationPlayButtonEnabled = false;
                AppContext.Instance.IsCurrentClandatabase = false;
                AppContext.Instance.IsAutomationStopButtonEnabled = false;
                AppContext.Instance.UpdateApplicationTitle();
                return SettingsManager;

            });
        }

        private Task FinishingUpTask()
        {
            return Task.Run(() =>
            {
                this.ClanManager = new ClanManager();

                if (System.IO.Directory.Exists(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder) == false)
                {
                    System.IO.Directory.CreateDirectory(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder);
                }

                if (File.Exists(AppContext.Instance.RecentOpenedClanDatabases))
                {
                    if(recentlyOpenedListManager.Build())
                    {
                        foreach(var recent in recentlyOpenedListManager.RecentClanDatabases)
                        {
                            if(string.IsNullOrEmpty(recent.FullClanRootFolder) ) 
                            {
                                continue;
                            }
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MenuItem mu = new MenuItem();
                                mu.Header = recent.ShortClanRootFolder;
                                mu.Tag = recent.FullClanRootFolder;
                                mu.Click += Mu_Click;
                                RecentlyOpenedParent.Items.Add(mu);
                            }));
                        }
                    }
                    //-- add seperator to recently opened clan databases.
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Separator separator = new Separator();
                        RecentlyOpenedParent.Items.Add(separator);
                        MenuItem mi = new MenuItem();
                        mi.Tag = "CLEAR_HISTORY";
                        mi.Header = "Clear Recent Clan Databases";
                        mi.Click += Mu_Click;
                        RecentlyOpenedParent.Items.Add(mi);
                    }));
                }
                else
                {

                }
              
            });
        }

        private Task BuildChestData()
        {
            return Task.Run(() =>
            {
                //applicationManager = new ApplicationManager();
                applicationManager.Build();
            });
        }

        private Task StartInsightsServer()
        {
            return Task.Run(() =>
            {
                if(applicationManager != null)
                {
                    applicationManager.StartNodeServer();
                }
            });
        }
        private Task LaunchTask(SplashScreen splashScreen)
        {
            return Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.Complete();
                }));
            });
        }
        public Task<Dictionary<string, bool>> ValidateClanDatabases()
        {
            return Task.Run(() =>
            {
                if (ClanManager != null)
                {
                    var clandatabases = ClanManager.Instance.ClanDatabaseManager.ClanDatabasesToUpgrade();
                    return clandatabases;
                }
                else
                {
                    return null;
                }
            });
        }
               
        public Task UpgradeClanDatabases(Dictionary<string, bool> clandatabases)
        {
            return Task.Run(() =>
            {
                ClanManager.Instance.ClanDatabaseManager.UpgradeClanDatabases(clandatabases);
            });
        }

        public async Task<bool> CheckForUpgrades()
        {
            if (applicationManager == null)
            {
                applicationManager = new ApplicationManager();
            }
            bool updateAvailable = await applicationManager.IsUpdateAvailable();
            return updateAvailable;
        }

        public async Task<bool> ValidateTessDataExists()
        {
            var dirInfo = new System.IO.DirectoryInfo(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder);
            if(dirInfo.Exists == false)
            {
                dirInfo.Create();
                return false;
            }
            else
            {
                var files = dirInfo.GetFiles("*.traineddata");
                if(files.Length > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public async Task<string> GetTesseractGithubDownloadPath()
        {

            var releases = await ApplicationManager.Instance.client.Repository.Release.GetAll("tesseract-ocr", SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.TesseractPackage);
            var latest = releases[0];
            if(latest != null)
            {
                var downloadUrl = latest.ZipballUrl;
                return downloadUrl;
            }
            return String.Empty;
        }

        public async Task DownloadTessData(Window window, string downloadUrl, string downloadFolder)
        {
            try
            {
                if (window is SplashScreen splashScreen)
                {

                    var downloadFile = $@"{downloadFolder}\{SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.TesseractPackage}.zip";
                    if(System.IO.File.Exists(downloadFile))
                    {
                        return;
                    }

                    CancellationTokenSource s = new CancellationTokenSource();

                    var progress = new Progress<double>(x =>
                    {
                        var p = x * 100.0;
                        Debug.WriteLine(p);
                        splashScreen.UpdateStatus($"Downloading Tesseract's Trained Models ({Math.Round(p)}%)", Math.Round(p));
                    });

                    await DownloadManager.Download(window, downloadUrl, downloadFile, progress, s.Token).ConfigureAwait(false);
                    
                    await Task.Delay(500);

                    splashScreen.UpdateStatus($"Downloading Tesseract's Trained Models (100%)", 100);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ExtractArchive(Window window, string archiveFile, string destinationFolder)
        {
            if (window is SplashScreen splashScreen)
            {
                if(System.IO.File.Exists(archiveFile))
                {
                    var progress = new Progress<ZipProgress>(x =>
                    {
                        double total = x.Total;
                        var current = x.CurrentItem;
                        double processed = x.Processed;
                        double percentage = (processed/total) * 100.0;
                        splashScreen.UpdateStatus($"Extracting Trained Tesseract Models.... ({Math.Round(processed)}/{Math.Round(total)})", percentage);
                    });

                    await ArchiveManager.Extract(archiveFile, destinationFolder, progress);

                    //-- now we should clean up.
                }
            }
        }


        public async Task InitChestAutomation()
        {
            ChestAutomation = new ChestAutomation();
            ChestAutomation.AutomationStarted += ChestAutomation_AutomationStarted;
            ChestAutomation.AutomationStopped += ChestAutomation_AutomationStopped;
            ChestAutomation.AutomationError += ChestAutomation_AutomationError;

            bool result = ChestAutomation.Initialize(SettingsManager.Instance.Settings.OCRSettings);
        }

        
        private void ChestAutomation_AutomationStarted(object sender, AutomationEventArguments e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                com.HellStormGames.Logging.Console.Write("Automation Started", com.HellStormGames.Logging.LogType.INFO);
                AppContext.Instance.IsAutomationPlayButtonEnabled = false;
                AppContext.Instance.IsAutomationStopButtonEnabled = true;

            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }
        private void ChestAutomation_AutomationStopped(object sender, AutomationEventArguments e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                AppContext.Instance.AutomationRunning = false;
                AppContext.Instance.isBusyProcessingClanchests = false;
               
                ClanManager.Instance.ClanChestManager.SaveDataTask();
                ClanManager.Instance.ClanChestManager.CreateBackup();
                
                //-- automatically repairs chest data if necessary
                //ClanManager.Instance.ClanChestManager.RepairChestData();

                AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                AppContext.Instance.IsAutomationStopButtonEnabled = false;
                com.HellStormGames.Logging.Console.Write("Automation stopped.", com.HellStormGames.Logging.LogType.INFO);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void ChestAutomation_AutomationError(object sender, AutomationErrorEventArguments e)
        {

        }
        public async Task DeleteTessData()
        {
            var di = new DirectoryInfo(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder);
            if (di.Exists)
            {
                di.Delete(true);
            }
            var tessdatatemp = new DirectoryInfo($"{AppContext.Instance.LocalApplicationPath}\\Temp");
            if (tessdatatemp.Exists)
            {
                tessdatatemp.Delete(true);
            }
        }

        public async Task DeleteDownloadedTempFolder()
        {
            try
            {
                var tessdatatemp = new DirectoryInfo($"{AppContext.Instance.LocalApplicationPath}\\Temp");
                if (tessdatatemp.Exists)
                {
                    tessdatatemp.Delete(true);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public async void Init(Window window)
        {


            bool bTessDataDownloadComplete = false;


            if (window is SplashScreen splashScreen)
            {


                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Checking For Updates...", 0);
                }));
                await Task.Delay(500);
                var upgradeAvailable = await CheckForUpgrades();
                AppContext.Instance.upgradeAvailable = upgradeAvailable;
                UpgradeAlertButton.ToolTip = upgradeAvailable == true ? TBChestTracker.Resources.Strings.UpdateAvailable : TBChestTracker.Resources.Strings.UpToDate;
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initalizing...", 10);
                }));

                recentlyOpenedListManager = new RecentlyOpenedListManager();

                await Task.Delay(500);
                SettingsManager = await InitSettings();

                if(SettingsManager == null)
                {
                    throw new Exception("SettingsManager is null");
                }

                //-- New TessData folder Path in Local User Data Folder
                var localAppFolder = new System.IO.DirectoryInfo(AppContext.Instance.LocalApplicationPath);
                if (localAppFolder.Exists == false)
                {
                    localAppFolder.Create();
                }

                if(String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder))
                {
                    throw new Exception("Unable to correctly populate settings.");
                }

                if(AppContext.Instance.bDeleteTessData == true)
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        splashScreen.UpdateStatus("Deleting Tesseract Data folder...", 10);
                    }));

                    await DeleteTessData();
                    await Task.Delay(500);
                }

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initializing Captain Hook...", 25);
                }));

                await Task.Delay(500);
                
                InitCaptainHook();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Tidying everything up...", 50);
                
                }));

                await Task.Delay(500);
                await FinishingUpTask();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Validating Clan databases...", 91);
                }));
                await Task.Delay(500);
                var databasesNeedUpgrade = await ValidateClanDatabases();
                if (databasesNeedUpgrade.Count > 0)
                {

                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        splashScreen.UpdateStatus("Upgrading Clan databases...", 92);
                    }));
                    await Task.Delay(500);
                    await UpgradeClanDatabases(databasesNeedUpgrade);
                }


                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Verifying Tesseract's TessData Installed...", 93);
                }));

                await Task.Delay(500);
                var tessDataExists = await ValidateTessDataExists();    
                if(tessDataExists == false)
                {
                    //-- we need to download the TessData-Best.zip from tesseract-ocr Github page.
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        splashScreen.UpdateStatus("Downloading Tesseract's Trained Models...", 94);
                    }));

                    var downloadUrl = await GetTesseractGithubDownloadPath();

                    if(String.IsNullOrEmpty(downloadUrl) == false)
                    {
                        var downloadFolderPath = $@"{AppContext.Instance.LocalApplicationPath}Temp";
                        var downloadFolderDirInfo = new System.IO.DirectoryInfo(downloadFolderPath);
                        if(downloadFolderDirInfo.Exists == false)
                        {
                            downloadFolderDirInfo.Create(); 
                        }
                        //-- we begin downloading now
                        await DownloadTessData(splashScreen, downloadUrl, downloadFolderPath);

                        await Task.Delay(500);

                        await this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            splashScreen.UpdateStatus("Extracting trained Tesseract models...", 95);
                        }));

                        var archiveFile = $@"{downloadFolderPath}\{SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.TesseractPackage}.zip";
                        await ExtractArchive(splashScreen, archiveFile, $"{AppContext.Instance.TesseractData}");

                        //-- remove archive file
                        //-- update SettingsManager.OCRSettings
                        //SettingsManager.Instance.Settings.OCRSettings.TessDataFolder = $@"{AppContext.Instance.TesseractData}";
                        SettingsManager.Instance.Save();
                        bTessDataDownloadComplete = true;

                    }
                    else
                    {
                        throw new Exception("Failed to obtain Tesseract download path.");
                    }
                }


                if(bTessDataDownloadComplete)
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        splashScreen.UpdateStatus("Cleaning up Temporary Files...", 97);
                    }));
                    await DeleteDownloadedTempFolder();
                    await Task.Delay(500);

                }
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initializing Chest Automation...", 98);
                }));

                await Task.Delay(500);
                
                await InitChestAutomation();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Building Chest Variables...", 99);
                }));
                await Task.Delay(250);

                await BuildChestData();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Starting Clan Insights Server...", 99);
                }));

                await Task.Delay(250);
                await StartInsightsServer();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Launching...", 100);
                }));

                await Task.Delay(1000);
                await LaunchTask(splashScreen);
                
            }
        }
        #endregion

        #region Window Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        public void ShowWindow()
        {
            this.Show();
            if(AppContext.Instance.RequiresOCRWizard)
            {
                //-- prompt the user to set up OCR for the first time.
                OCRWizardWindow = new OCRWizardWindow();
                if (OCRWizardWindow != null)
                    OCRWizardWindow.Show();
            }
            
        }
        
        #region CaptionHook Events

        private void CaptainHook_onInstalled(object sender, EventArgs e)
        {
            com.HellStormGames.Logging.Console.Write("Installed Keyboard hooks successfully.", com.HellStormGames.Logging.LogType.INFO);
        }

        string keyInput = $"";

        private Key previousKey = Key.None;
        private Key newKey = Key.None;
        String keyStr = $"";
        private void CaptainHook_onKeyboardMessage(object sender, KeyboardHookMessageEventArgs e)
        {
            if(!AppContext.Instance.IsConfiguringHotKeys) { 
                
                int vkey = e.VirtKeyCode;
                var key = KeyInterop.KeyFromVirtualKey(vkey);
                var bIsNewKey = false;
                
                //--- to allow customized hot keys, will reuire update.
                bool keyDown = e.MessageType == KeyboardMessage.KeyDown ? true : false;
                if(keyDown)
                {
                    newKey = key;
                    if (previousKey != newKey)
                    {
                        bIsNewKey = true;
                        previousKey = newKey;
                    }
                    else
                        bIsNewKey = false;

                    if(bIsNewKey)
                    {
                        if (String.IsNullOrEmpty(keyStr))
                            keyStr = key.ToString();
                        else
                            keyStr += $"+{key.ToString()}";
                    }
                }
                else
                {
                    if(keyStr.Equals(SettingsManager.Instance.Settings.HotKeySettings.StartAutomationKeys))
                    {
                        ChestAutomation.StartAutomation();
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        keyStr = String.Empty;
                        previousKey = Key.None;
                        newKey = Key.None;
                    }
                    if(keyStr.Equals(SettingsManager.Instance.Settings.HotKeySettings.StopAutomationKeys))
                    {
                        ChestAutomation.StopAutomation();
                        
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        keyStr = String.Empty;
                        previousKey = Key.None;
                        newKey = Key.None;
                    }
                    keyStr = String.Empty;
                    bIsNewKey = false;

                }
            }
        }

        #endregion

        #region Window Closing
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ChestAutomation.Release();

            AppContext.Instance.isAppClosing = true;
            applicationManager.KillNodeServer();
            CaptainHook.Uninstall();
            
            ClanManager.Instance.Destroy();
            
            com.HellStormGames.Logging.Console.Destroy();
            SettingsManager.Dispose();
            applicationManager.Dispose();
        }
        #endregion

        #region Menu Command Functions

        #region Load Clan Database
        private void LoadClanDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute= true; 
        }
        private void LoadClanDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
          ShowLoadClanWindow(result =>
          {

          });
        }
        #endregion

        #region Recently Opened Databases Menu Item Click
        private void Mu_Click(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            var tag = menuitem.Tag.ToString();
            if (tag != "CLEAR_HISTORY")
            {
                var file = menuitem.Tag.ToString();
                LoadReentFile(file, result => { });
            }
            else
            {
                try
                {
                    recentlyOpenedListManager.Delete();

                    //System.IO.File.Delete($@"{AppContext.Instance.RecentOpenedClanDatabases}");
                    RecentlyOpenedParent.Items.Clear();
                    //recentlyOpenedDatabases.RecentClanDatabases.Clear();
                }
                catch(Exception ex)
                {
                    com.HellStormGames.Logging.Loggy.Write($"{ex.Message}", com.HellStormGames.Logging.LogType.ERROR);
                }
            }
        }
        #endregion
        #region LoadRecentFile
        public void LoadReentFile(string file, Action<bool> response)
        {
            ClanManager.Instance.ClanDatabaseManager.Load(file, ClanManager.Instance.ClanChestManager, result =>
            {
                if (result)
                {
                    com.HellStormGames.Logging.Console.Write($"Loaded Clan ({ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}) Database Successfully.",
                        com.HellStormGames.Logging.LogType.INFO);

                    AppContext.Instance.UpdateCurrentProject($"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}");
                    AppContext.Instance.UpdateApplicationTitle();
                    
                    AppContext.Instance.IsCurrentClandatabase = true;
                    AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                    //ClanManager.Instance.ClanChestManager.RepairChestData();
                    response(true);
                }
                else
                {
                    MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                }
            });
        }

        
        public void CreateNewClan(Action<bool> response)
        {
            NewClanDatabaseWindow newClanDatabaseWindow = new NewClanDatabaseWindow();
            if (newClanDatabaseWindow.ShowDialog() == true)
            {
                AppContext.Instance.NewClandatabaseBeenCreated = true;
                AppContext.Instance.UpdateCurrentProject($"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}");
                AppContext.Instance.UpdateApplicationTitle();
                
                response(true);
            }
            else
                response(false);
        }
        

        public void ShowLoadClanWindow(Action<bool> response)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Clan Databases | *.cdb";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;
           

            if (ClanManager.Instance.ClanChestSettings.ChestRequirements != null)
                ClanManager.Instance.ClanChestSettings.Clear();

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;
                ClanManager.Instance.ClanDatabaseManager.Load(file, ClanManager.ClanChestManager, result =>
                {
                    if (result)
                    {
                        AppContext.Instance.UpdateCurrentProject($"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}");
                        AppContext.Instance.UpdateApplicationTitle();

                        var recentlyOpenedFiles = recentlyOpenedListManager.RecentClanDatabases.Select(f => f.FullClanRootFolder).ToList();

                        if(!recentlyOpenedFiles.Contains(file))
                        {
                            RecentClanDatabase recentdb = new RecentClanDatabase();
                            recentdb.ClanAbbreviations = ClanManager.ClanDatabaseManager.ClanDatabase.ClanAbbreviations;
                            recentdb.ClanName = ClanManager.ClanDatabaseManager.ClanDatabase.Clanname;
                            var position = StringHelpers.findNthOccurance(file, Convert.ToChar(@"\"), 3);
                            recentdb.ShortClanRootFolder = StringHelpers.truncate_file_name(file, position);
                            recentdb.FullClanRootFolder = file;
                            recentdb.LastOpened = DateTime.Now.ToFileTimeUtc().ToString();

                            recentlyOpenedListManager.RecentClanDatabases.Add(recentdb);

                            MenuItem mu = new MenuItem();
                            mu.Header = StringHelpers.truncate_file_name(file, position);
                            mu.Tag = file;
                            mu.Click += Mu_Click;
                            RecentlyOpenedParent.Items.Add(mu);

                            recentlyOpenedListManager.Save();
                            
                            if(RecentlyOpenedParent.Items.Count <= 1)
                            {
                                Separator separator = new Separator();
                                RecentlyOpenedParent.Items.Add(separator);
                                MenuItem mi = new MenuItem();
                                mi.Tag = "CLEAR_HISTORY";
                                mi.Header = "Clear Recent Clan Databases";
                                mi.Click += Mu_Click;
                                RecentlyOpenedParent.Items.Add(mi);
                            }

                        }

                        com.HellStormGames.Logging.Console.Write($"Loaded Clan ({ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}) Database Successfully.",
                            com.HellStormGames.Logging.LogType.INFO);

                        AppContext.Instance.IsCurrentClandatabase = true;
                        AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                        //ClanManager.Instance.ClanChestManager.RepairChestData();
                        response(true);
                    }
                    else
                    {
                        MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                        response(false);
                    }
                });
            }
            else
                response(false);
        }
        #endregion
        #region Save Clan database
        private void SaveClanDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.NewClandatabaseBeenCreated)
                e.CanExecute = true;
            else 
                e.CanExecute = false;
        }
        
        private void SaveClanDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClanManager.Instance.ClanDatabaseManager.Save();
        }
        #endregion
        #region Save Clan Database As
        private void SaveAsClanDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(AppContext.Instance.NewClandatabaseBeenCreated)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void SaveAsClanDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Export Clan Database
        private void ExportClanDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.NewClandatabaseBeenCreated && AppContext.Instance.ClanmatesBeenAdded)
                e.CanExecute = true;
            else 
                e.CanExecute = false;
        }

        
        private void ExportClanDatabase_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportWindow exportWindow = new ExportWindow();
            exportWindow.ShowDialog();
        }
        #endregion
        #region Quit Application
        private void QuitApplication_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QuitApplication_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Manage Clanmates
        private void ManageClanmatesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = AppContext.Instance.NewClandatabaseBeenCreated;
        }

        private void ManageClanmatesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ManageClanmatesWindow addClanmatesWindow = new ManageClanmatesWindow();
            if (addClanmatesWindow.ShowDialog() == true)
            {
                AppContext.Instance.ClanmatesBeenAdded = true;
                AppContext.Instance.IsAutomationPlayButtonEnabled = true;
            }
        }
        #endregion

        #region Manage Clan Chests Settings
        private void ManageClanchestSettingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = AppContext.Instance.NewClandatabaseBeenCreated;
        }
        private void ManageClanchestSettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChestingRequirementsWindow che = new ChestingRequirementsWindow();
            if(che.ShowDialog() == true)
            {

            }
        }
        #endregion
   
        #region Start Automation
        private void StartAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.NewClandatabaseBeenCreated
                && AppContext.Instance.ClanmatesBeenAdded && AppContext.Instance.OCRCompleted && AppContext.Instance.IsClanChestDataCorrupted == false)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        private void StartAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!AppContext.Instance.AutomationRunning)
            {
                ChestAutomation.StartAutomation();
               
            }
        }
        #endregion

        #region Stop Automation
        private void StopAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.ClanmatesBeenAdded 
                && AppContext.Instance.NewClandatabaseBeenCreated && AppContext.Instance.OCRCompleted)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        private void StopAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (AppContext.Instance.AutomationRunning)
            {
                ChestAutomation.StopAutomation();
            }
        }
        #endregion

        #region Clan Stats
        private void ClanStatsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.ClanmatesBeenAdded && AppContext.Instance.NewClandatabaseBeenCreated)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }


        private async void PrepareClanInsightsData()
        {
            var server = "http://127.0.0.1:8888/";
            var apiCall = "api/Build";
            var responseData = String.Empty;

            //-- build ClanInsightsData
            var chestdata = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions != ChestOptions.UseConditions  ? ClanManager.Instance.ClanChestManager.ClanChestDailyData : ClanManager.Instance.ClanChestManager.FilterClanChestByConditions();
            var gameChests = new List<string>();
            var previousChest = String.Empty;
            foreach(var chest in ApplicationManager.Instance.Chests)
            {
                if (chest.ChestType != previousChest)
                {
                    gameChests.Add(chest.ChestType);
                    previousChest = chest.ChestType;
                }
            }

            var clan = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname;
            var size = ClanManager.Instance.ClanmateManager.Database.NumClanmates;
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            var clanmateNames = clanmates.Select(n => n.Name).ToList();
            var usePoints = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints;

            ClanInsightsData insightsData = new ClanInsightsData(clan,size,clanmateNames,gameChests, chestdata, usePoints);

            var JsonStr = JsonConvert.SerializeObject(insightsData, Formatting.Indented);

            var stringContent = new StringContent(JsonStr, Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync($"{server}{apiCall}", stringContent);
            if(httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                Debug.WriteLine(responseContent);
            }
        }
        private void ClanStatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrepareClanInsightsData();
            
            ClanInsightsWindow clanInsightsWindow = new ClanInsightsWindow();
            clanInsightsWindow.Show();
        }
        #endregion

        #region Clan Manager
        private void ClanManagerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ClanManagerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClanManagementWindow clanManagementWindow = new ClanManagementWindow();
            clanManagementWindow.mainWindow = this; 
            if(clanManagementWindow.ShowDialog() == true)
            {
                AppContext.Instance.IsCurrentClandatabase = true;
                //AppContext.Instance.IsAutomationPlayButtonEnabled = true;
            }
        }
        #endregion

        #region Show Console
        private void ConsoleMenuButton_Click(object sender, RoutedEventArgs e)
        {
            consoleWindow.Show();
        }

        #endregion

        #region Validate Chest Data Integrity
        
        #endregion

        #region Manage Settings
        private void ManageSettingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void ManageSettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow = new SettingsWindow();
            SettingsWindow.Show();
        }
        #endregion
        #region OCR Wizard
        private void OCRWizard_Click(object sender, RoutedEventArgs e)
        {
            OCRWizardWindow ocrWizardWindow = new OCRWizardWindow();
            ocrWizardWindow.Show();
        }

        #endregion

        #endregion

        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            if(consoleWindow != null) consoleWindow.Show();
            else
            {
                consoleWindow = new ConsoleWindow();
                consoleWindow.Show();
            }
        }

        private void OCRWizardButton_Click(object sender, RoutedEventArgs e)
        {
            if (OCRWizardWindow != null)
                OCRWizardWindow = null;

            
            OCRWizardWindow = new OCRWizardWindow();
            OCRWizardWindow.Show();

        }

        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)e.OriginalSource;
            var _tag = mi.Tag;
            if (_tag != null)
            {
                switch(_tag)
                {
                    case "AboutApp":
                        {
                            AboutWindow aboutWindow = new AboutWindow();
                            aboutWindow.Show();
                        }
                        break;
                    case "Tutorial":
                        {

                        }
                        break;
                    case "ReportIssue":
                        {
                            Process.Start("https://github.com/SICGames/TBChestTracker/issues");
                        }
                        break;
                    case "VisitGithub":
                        {
                            Process.Start("https://www.github.com/SICGames/TBChestTracker");
                        }
                        break;
                    case "Youtube":
                        {
                            Process.Start("https://www.youtube.com/@TotalBattleGuide");
                        }
                        break;
                    case "Patreon":
                        {
                            Process.Start($"https://www.patreon.com/TotalBattleGuide");
                        }
                        break;
                }
            }
        }

        private void ChestBuilderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChestBuilderWindow chestBuilderWindow = new ChestBuilderWindow();
            chestBuilderWindow.Show();
        }

        private void AbsentClanmateCleanerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AbsentClanmatesCleanerWindow absentClanmatesCleanerWindow = new AbsentClanmatesCleanerWindow();
            absentClanmatesCleanerWindow.ShowDialog();
        }

        private void ClanWealthBuilderMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpgradeAlertButton_Click(object sender, RoutedEventArgs e)
        {
            if(AppContext.Instance.upgradeAvailable)
            {
                //-- show notificationbox. 
                UpdateWindow updateWindow = new UpdateWindow();
                updateWindow.Show();

            }
        }

        private void ClanmateValidationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClanmateValidationWindow clanmateValidationWindow = new ClanmateValidationWindow();
            clanmateValidationWindow.Show();
        }

        private void RestoreClanChestDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RestoreClanChestDataWindow restoreclandatabasewindow = new RestoreClanChestDataWindow();
            if(restoreclandatabasewindow.ShowDialog() == true)
            {

            }
        }

        private void ValidateClanChestIntegrityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ValidateClanChestsIntegrityWindow validateChestDataIntegrity = new ValidateClanChestsIntegrityWindow();
            validateChestDataIntegrity.Show();
        }
    }
}
