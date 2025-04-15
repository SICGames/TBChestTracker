using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Text;
using System.Net.Http;
using System.Globalization;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Emgu.CV;
using Newtonsoft.Json;

using com.CaptainHook;
using com.HellStormGames.Imaging.ScreenCapture;
using com.HellStormGames.Diagnostics;
using com.HellStormGames.Imaging.Extensions;

using TBChestTracker.Automation;
using TBChestTracker.Managers;
using TBChestTracker.Localization;
using TBChestTracker.Extensions;
using TBChestTracker.Windows.BuildingChests;
using TBChestTracker.Windows.OCRCorrection;
using TBChestTracker.Web;
using TBChestTracker.Windows;
using TBChestTracker.Windows.ClanmateRemoval;

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
        
        //public AppContext appContext { get; private set; }
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
        
        string keyInput = $"";
        private Key previousKey = Key.None;
        private Key newKey = Key.None;
        String keyStr = $"";


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
        }
        #endregion

        #region Initializing functions
        private async Task InitCaptainHook()
        {
            CaptainHook = new CaptainHook();
            CaptainHook.onKeyboardMessage += CaptainHook_KeyboardMessage;
            CaptainHook.onInstalled += CaptainHook_onInstalled;
            CaptainHook.onError += CaptainHook_onError;
            CaptainHook.Install();
        }

        private void CaptainHook_KeyboardMessage(object sender, KeyboardHookMessageEventArgs e)
        {
            if (!AppContext.Instance.IsConfiguringHotKeys)
            {

                int vkey = e.VirtKeyCode;
                var key = KeyInterop.KeyFromVirtualKey(vkey);
                var bIsNewKey = false;

                //--- to allow customized hot keys, will reuire update.
                bool keyDown = e.MessageType == KeyboardMessage.KeyDown ? true : false;
                if (keyDown)
                {
                    newKey = key;
                    if (previousKey != newKey)
                    {
                        bIsNewKey = true;
                        previousKey = newKey;
                    }
                    else
                        bIsNewKey = false;

                    if (bIsNewKey)
                    {
                        if (String.IsNullOrEmpty(keyStr))
                            keyStr = key.ToString();
                        else
                            keyStr += $"+{key.ToString()}";
                    }
                }
                else
                {
                    if (keyStr.Equals(SettingsManager.Instance.Settings.HotKeySettings.StartAutomationKeys))
                    {
                        StartAutomation();
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        keyStr = String.Empty;
                        previousKey = Key.None;
                        newKey = Key.None;
                    }
                    if (keyStr.Equals(SettingsManager.Instance.Settings.HotKeySettings.StopAutomationKeys))
                    {
                        if (AppContext.Instance.IsAutomationStopButtonEnabled == true)
                        {
                            StopAutomation();
                        }
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

        private void CaptainHook_onError(object sender, EventArgs e)
        {
            
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
        private async Task<SettingsManager> InitSettings()
        {

            SettingsManager = new SettingsManager();
            Loggio.Info("Settings Loaded.");
            //-- init appContext
            AppContext.Instance.IsAutomationPlayButtonEnabled = false;
            AppContext.Instance.IsCurrentClandatabase = false;
            AppContext.Instance.IsAutomationStopButtonEnabled = false;
            AppContext.Instance.UpdateApplicationTitle();
            return SettingsManager;
        }

        private async Task InitSnapster()
        {

            //-- depending on settings. Which monitor to load.
            try
            {
                Loggio.Info("Initializing Snapster.");
                var monitorIndex = SettingsManager.Instance?.Settings?.GeneralSettings?.MonitorIndex;
                var usePrimaryMonitor = false;
                if(monitorIndex == -1)
                {
                    usePrimaryMonitor = true;
                }
                if (usePrimaryMonitor)
                {
                    Snapster.Capturer = new SnapsterConfiguration().
                    CapturerContext.WindowsGDI(MonitorConfiguration.MainMonitor).CreateCapturer();
                    SettingsManager.Instance.Settings.GeneralSettings.MonitorIndex = Snapster.MonitorConfiguration.Monitor.ID;
                    SettingsManager.Save();
                }
                else
                {
                    Snapster.Capturer = new SnapsterConfiguration().
                    CapturerContext.WindowsGDI(MonitorConfiguration.ByIndex(monitorIndex.Value)).CreateCapturer();
                }

            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Snapster Init", $"Failed to initialize Snapster. Following reason: {ex.Message}.");
                throw new Exception(ex.Message);
            }
        }
        private async Task FinishingUpTask()
        {

            this.ClanManager = new ClanManager();

            if (System.IO.Directory.Exists(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder) == false)
            {
                System.IO.Directory.CreateDirectory(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder);
            }

            if (File.Exists(AppContext.Instance.RecentOpenedClanDatabases))
            {
                if (recentlyOpenedListManager.Build())
                {
                    foreach (var recent in recentlyOpenedListManager.RecentClanDatabases)
                    {
                        if (string.IsNullOrEmpty(recent.FullClanRootFolder))
                        {
                            continue;
                        }
                        await this.Dispatcher.BeginInvoke(new Action(() =>
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
                await this.Dispatcher.BeginInvoke(new Action(() =>
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
        }

        private async Task BuildChestData()
        {
            //applicationManager = new ApplicationManager();
            applicationManager.Build();
        }

        private async Task StartInsightsServer()
        {
            if (applicationManager != null)
            {
                applicationManager.StartNodeServer();
            }
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
        public async Task<Dictionary<string, bool>> ValidateClanDatabases()
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
        }
               
        public async Task UpgradeClanDatabases(Dictionary<string, bool> clandatabases)
        {
            ClanManager.Instance.ClanDatabaseManager.UpgradeClanDatabases(clandatabases);
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

        private async Task<bool> RequiresTessDataCleaning()
        {
            if(SettingsManager.Instance.Settings.OCRSettings.TessDataConfig == null)
            {
                //-- this shouldn't happen unless migrating from v1.5 to latest version.
                Loggio.Warn("Tesseract Data Configuration inside OCR Settings is null (empty). Possibly migrating from 1.5 to latest version of Total Battle Chest Tracker.");
                SettingsManager.Instance.Settings.OCRSettings.TessDataConfig = new TessDataConfig(TessDataOption.Best);
                SettingsManager.Instance.Save();
                
                return true;
            }
            else  if(SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.Option != TessDataOption.Best)
            {
                Loggio.Info("Not The Best Trained Models are configured within Tesseract Data Configuration.");
                return true;
            }
            Loggio.Info("The Best Trained Models are configured within Tesseract Data Configuration.");
            return false;
        }
        public async Task<bool> ValidateTessDataExists()
        {
            Loggio.Info("Ensuring Tesseract Data Configuration is not null.");
            if(SettingsManager.Instance.Settings.OCRSettings.TessDataConfig == null)
            {
                SettingsManager.Instance.Settings.OCRSettings.TessDataConfig = new TessDataConfig(TessDataOption.Best);
                Loggio.Info("Ensuring Tesseract Data Configuration corrected..");
            }

            Loggio.Info("Checking to see if the Tesseract Data Folder exists specified within OCR Settings.");
            var dirInfo = new System.IO.DirectoryInfo(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder);
            if(dirInfo.Exists == false)
            {
                Loggio.Info("Creating Tesseract Data Folder.");
                dirInfo.Create();
                return false;
            }
            else
            {
                Loggio.Info("Fetching trained data files within Tesseract Data folder.");
                var files = dirInfo.GetFiles("*.traineddata");
                if(files.Length > 0)
                {
                    Loggio.Info("Trained Tesseract Models are present.");
                    return true;
                }
                Loggio.Info("Trained Tesseract Models are not present.");

                return false;
            }
        }

        public async Task<string> GetTesseractGithubDownloadPath()
        {
            try
            {
                var releases = await ApplicationManager.Instance.client.Repository.Release.GetAll("tesseract-ocr", SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.TesseractPackage);
                var latest = releases[0];
                if (latest != null)
                {
                    var downloadUrl = latest.ZipballUrl;
                    return downloadUrl;
                }
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Fetching Tesseract's Trained Models from Github", "An error has occurred while attempting to download Tesseract's Trained Models. Log file has more information to send to developer.");
                return string.Empty;
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

                    //-- archive file was never closed or disposed. Fixed 11/10/24.
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
                Loggio.Info("Automation Started.");
                AppContext.Instance.IsAutomationPlayButtonEnabled = false;
                AppContext.Instance.IsAutomationStopButtonEnabled = true;
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }

        private void BuildClanChests()
        {
            BuildingChestsDateRangeWindow chestDateRangeWindow  = new BuildingChestsDateRangeWindow();
            chestDateRangeWindow.Show();
        }
        private void ChestAutomation_AutomationStopped(object sender, AutomationEventArguments e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                AppContext.Instance.AutomationRunning = false;
                AppContext.Instance.isBusyProcessingClanchests = false;

                //-- automatically repairs chest data if necessary
                AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                AppContext.Instance.IsAutomationStopButtonEnabled = false;
                Loggio.Info("Automation Stopped.");

                if (ChestAutomation.isCancelled)
                {
                    bool buildChestsAfterStopping = SettingsManager.Settings.AutomationSettings.BuildChestsAfterStoppingAutomation;
                    if (buildChestsAfterStopping)
                    {
                        var tmpdirdi = new DirectoryInfo($"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Temp");
                        
                        if (tmpdirdi.Exists)
                        {
                            var chestfiles = tmpdirdi.EnumerateFiles("chests_*.txt", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();
                            if (chestfiles.Length > 0)
                            {
                                BuildingChestsWindow buildingchestswindow = new BuildingChestsWindow(SettingsManager.Settings.AutomationSettings);
                                buildingchestswindow.ChestFiles = chestfiles;
                                buildingchestswindow.Show();
                            }
                        }
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void ChestAutomation_AutomationError(object sender, AutomationErrorEventArguments e)
        {
          
        }
        public async Task DeleteTessData()
        {
            try
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
            catch (Exception ex)
            {
                //-- this shouldn't reach this point because everything should've been handled correctly. 
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

        public async Task<CommonResult> Init(Window window)
        {
            bool bTessDataDownloadComplete = false;

            if (window is SplashScreen splashScreen)
            {
                
                Loggio.Info("Total Battle Chest Tracker Initializing...");

                await UpdateSplashScreen(splashScreen,"Checking For Updates...", 0);
                
                Loggio.Info("Checking for new updates...");

                var upgradeAvailable = await CheckForUpgrades();
                AppContext.Instance.upgradeAvailable = upgradeAvailable;

                await UpgradeAlertButton.Dispatcher.BeginInvoke(() =>
                {
                    UpgradeAlertButton.ToolTip = upgradeAvailable == true ? TBChestTracker.Resources.Strings.UpdateAvailable : TBChestTracker.Resources.Strings.UpToDate;
                });

                await UpdateSplashScreen(splashScreen, "Initializing...", 10);

                recentlyOpenedListManager = new RecentlyOpenedListManager();

                Loggio.Info("Initializing Settings...");

                await UpdateSplashScreen(splashScreen, "Initializing Settings...", 12);

                SettingsManager = await InitSettings();

                if(SettingsManager == null)
                {
                    Loggio.Warn("SettingsManager is null. It should not be null.");
                    return new CommonResult(CommonResult.Error, "Settings Manager is Null");
                    //throw new Exception("SettingsManager is null");
                }

                //-- forcefully remove Contains. We don't care about it no more. 
                if(SettingsManager.Settings.OCRSettings.Tags.Contains("Contains"))
                {
                    SettingsManager.Settings.OCRSettings.Tags.Remove("Contains");
                    SettingsManager.Save();
                }

                Loggio.Info("Settings Manager is initialized.");

                //-- need to figure out if user is new or not. 
                //-- OldClanPath doesn't exist for new users. Created at FinishingUpTask. 
                //-- Older users have files already within OldClanRoot. 
                var newClanRoot = $"{AppContext.Instance.LocalApplicationPath}Clans\\";
                var clanrootDi = new DirectoryInfo(newClanRoot);
                bool hasMigrated = false;
                bool isNewUser = false;
                bool hasNewClanrootHasfiles = false;
                bool hasOldClanrootHasFiles = false;

                if(clanrootDi.Exists)
                {
                    var files = clanrootDi.EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories);
                    
                    hasNewClanrootHasfiles = files.Count() > 0;
                }
                
                var obsoletepath = SettingsManager.Settings.GeneralSettings.ClanRootFolder;
                if (obsoletepath.ToLower().Equals(newClanRoot.ToLower()) == false) 
                {
                    var obsoleteDi = new DirectoryInfo(obsoletepath);   
                    hasOldClanrootHasFiles = obsoleteDi?.Exists == true ? 
                        (obsoleteDi.EnumerateFiles("*.*", SearchOption.AllDirectories).Count() > 0 ? true : false) 
                        : false; 
                }
                if(hasOldClanrootHasFiles == false && hasNewClanrootHasfiles == true)
                {
                    hasMigrated = true;
                    isNewUser = false;
                }
                else if(hasOldClanrootHasFiles == true && hasNewClanrootHasfiles == false)
                {
                    hasMigrated = false;
                    isNewUser = false;
                }
                else if(hasOldClanrootHasFiles == false && hasNewClanrootHasfiles == false)
                {
                    hasMigrated = false;
                    isNewUser = true;
                }

                if (obsoletepath.ToLower().Equals(newClanRoot.ToLower()))
                {
                    hasMigrated = true;
                    isNewUser = hasOldClanrootHasFiles == true ? true : false;
                }
                else
                {
                    if (newClanRoot.ToLower().Contains(obsoletepath.ToLower()))
                    {
                        //-- need to ensure those that had migrated before patch gets fixed.
                        var localappdata = AppContext.Instance.LocalApplicationPath;
                        if (obsoletepath.ToLower().Equals(localappdata.ToLower()))
                        {
                            hasMigrated = true;
                            isNewUser = false;
                            SettingsManager.Settings.GeneralSettings.ClanRootFolder = newClanRoot;
                            SettingsManager.Save();
                        }
                    }
                }
                if (hasMigrated == false && isNewUser == false)
                {
                    //-- user hasn't migrated.
                    //-- Incase user still has 
                    await UpdateSplashScreen(splashScreen, "Checking to see if Clans need to be migrated...", 13);
                    bool needsClanMigration = await ApplicationManager.Instance.NeedsClanMigration(newClanRoot);
                    await Task.Delay(100);
                    if (needsClanMigration)
                    {
                        await UpdateSplashScreen(splashScreen, "Migrating Clans, Please Wait...", 15);
                        bool migrated = await new ClanMigration(obsoletepath, newClanRoot).Migrate(SettingsManager);
                        if (migrated == false)
                        {
                            //-- something happened. Couldn't migrate. But technically, this shouldn't even happen.
                            SettingsManager.Settings.GeneralSettings.ClanRootFolder = newClanRoot;
                            SettingsManager.Save();
                            Loggio.Warn("Clan Migration", "Couldn't migrate clans to new path. Reverting back to using old clan root path.");
                            await UpdateSplashScreen(splashScreen, "Migration Failed, rolling back changes...", 17);
                            await Task.Delay(100);
                        }
                        else
                        {
                            Loggio.Info("Clan Migration", "Clans have successfully migrated to new clan rooth path.");
                            await UpdateSplashScreen(splashScreen, "Clans have been migrated successfully...", 17);
                            await Task.Delay(100);
                            await UpdateSplashScreen(splashScreen, "Removing Obsolete Clans Root Path...", 18);
                            bool cleanedup = await ClanMigration.Cleanup(obsoletepath);
                            if (cleanedup)
                            {
                                await UpdateSplashScreen(splashScreen, "Obsolete Clan Root Path Cleaned...", 19);
                            }
                            else
                            {
                                await UpdateSplashScreen(splashScreen, "Issues cleaning Obsolete Path. Manual Deletion is required...", 19);
                            }
                            await Task.Delay(500);
                        }
                    }
                }
                await UpdateSplashScreen(splashScreen, "Verifying Clans Integrity...", 17);

                var localappPath = $"{AppContext.Instance.LocalApplicationPath}";
                var localAppPathDi = new DirectoryInfo(localappPath);
                var badClanLocation = new List<string>();
                if(localAppPathDi.Exists)
                {
                    var clanFiles = localAppPathDi.EnumerateFileSystemInfos("*.cdb", SearchOption.AllDirectories);
                    foreach(var clanFile in clanFiles.ToList())
                    {
                        var clanDirectory = clanFile.FullName.Substring(0, clanFile.FullName.LastIndexOf("\\") + 1);
                        if(String.IsNullOrEmpty(clanDirectory)  == false)
                        {
                            if (clanDirectory.Contains($"{localappPath}Clans\\") == false)
                            {
                                badClanLocation.Add(clanDirectory);
                            }
                        }
                    }
                    if(badClanLocation?.Count() > 0)
                    {
                        foreach(var badlocation in badClanLocation)
                        {
                            var clanNameFolder = badlocation.Substring(0, badlocation.LastIndexOf("\\"));
                            clanNameFolder = clanNameFolder.Substring(clanNameFolder.LastIndexOf("\\") + 1);
                            var newClanLocation = $"{newClanRoot}{clanNameFolder}\\";
                         

                            var m = await new ClanMigration(badlocation, newClanLocation).Migrate();
                            if(m == false)
                            {

                            }
                            else
                            {
                                await ClanMigration.Cleanup(badlocation);
                            }
                        }
                    }
                }

                //-- now we initialize Snapster. 
                await UpdateSplashScreen(splashScreen, "Initializing Snapster...", 18);
                await InitSnapster();
                if (Snapster.MonitorConfiguration != null)
                {
                    var monitor = Snapster.MonitorConfiguration.Monitor;
                    Loggio.Info($"Snapster Initialized using Monitor ({monitor.MonitorName}, X: {monitor.ScreenBounds.X}, Y: {monitor.ScreenBounds.Y}, Width: {monitor.ScreenBounds.Width}, Height: {monitor.ScreenBounds.Height}) ");
                }
                await Task.Delay(250);

                //-- New TessData folder Path in Local User Data Folder
                var localAppFolder = new System.IO.DirectoryInfo(AppContext.Instance.LocalApplicationPath);
                if (localAppFolder.Exists == false)
                {
                    localAppFolder.Create();
                }

                if(String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder))
                {
                    Loggio.Warn("Wasn't able to obtain TessData path from 'Settings.json' properly.");

                    return new CommonResult(CommonResultCodes.Error, "Tesseract Data Folder field within Settings.json is empty. This isn't correct.");
                }

                if(AppContext.Instance.bDeleteTessData == true)
                {
                    await UpdateSplashScreen(splashScreen, "Deleting Tesseract Data Folder...", 16);

                    Loggio.Info("Cleaning Tesseract Data Folder...");
                    await DeleteTessData();
                    Loggio.Info("Cleaned Tesseract Data Folder.");
                }
                await Task.Delay(200);

                await UpdateSplashScreen(splashScreen, "Tidying everything up...", 30);

                Loggio.Info("Finishing up Initializing...");

                await FinishingUpTask();

                await UpdateSplashScreen(splashScreen, "Validating Clan Databases...", 35);

                Loggio.Info("Validating Clan Databases...");

                var databasesNeedUpgrade = await ValidateClanDatabases();

                if (databasesNeedUpgrade.Count > 0)
                {
                    await UpdateSplashScreen(splashScreen, "Upgrading Clan Databases...", 38);
                    Loggio.Info("Upgrading Clan Databases...");                
                    await UpgradeClanDatabases(databasesNeedUpgrade);
                }

                Loggio.Info("Clan Databases finished validating...");
                Loggio.Info("Checking to see if Tesseract Data is at it's Best.");

                await UpdateSplashScreen(splashScreen, "Verifying Tesseract's Trained Models Are The Best...", 40);

                var requiresTessDataCleanup = await RequiresTessDataCleaning();
                
                await Task.Delay(250); //-- wait incase something needs to catch up.

                string requiresCleanup = requiresTessDataCleanup == true ? "Yes" : "No";

                Loggio.Info($"Does Tesseract Data need spring cleaning? [{requiresCleanup}].");

                if(requiresTessDataCleanup)
                {
                    if(MessageBox.Show("Since this version of Total Battle Chest Tracker uses the best tesseract training models, you will need to perform TessData Cleanup. Press Ok button to restart application to clean TessData.") ==  MessageBoxResult.OK)
                    {
                        //-- should attempt to quit

                        SettingsManager.Instance.Settings.OCRSettings.TessDataConfig = new TessDataConfig(TessDataOption.Best);
                        SettingsManager.Instance.Save();
                        Loggio.Info($"Settings saved with new TessDataConfiguration applied. Restarting application safely.");

                        return new CommonResult(CommonResultCodes.Error, "CleanTesseractData");
                        
                        //AppContext.RestartApplication("--delete_tessdata");
                    }
                }
                
                Loggio.Info("Validating Tesseract Trained Models Are Installed...");

                await UpdateSplashScreen(splashScreen, "Verifying Tesseract's Trained Models Are Installed...", 45);
                var tessDataExists = await ValidateTessDataExists();    
                if(tessDataExists == false)
                {
                    //-- we need to download the TessData-Best.zip from tesseract-ocr Github page.
                    await UpdateSplashScreen(splashScreen, "Downloading Tesseract's Trained Models...", 50);
                    
                    Loggio.Info("Downloading Tesseract's Trained Models...");

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

                        await Task.Delay(150);

                        await UpdateSplashScreen(splashScreen, "Extracting trained tesseract's models...", 55);
                        
                        var archiveFile = $@"{downloadFolderPath}\{SettingsManager.Instance.Settings.OCRSettings.TessDataConfig.TesseractPackage}.zip";

                        Loggio.Info("Extracting Downloaded Tesseract Trained Models...");

                        await ExtractArchive(splashScreen, archiveFile, $"{AppContext.Instance.TesseractData}");

                        //-- remove archive file
                        //-- update SettingsManager.OCRSettings
                        SettingsManager.Instance.Save();
                        bTessDataDownloadComplete = true;

                    }
                    else
                    {
                        Loggio.Warn("Unable to obtain Tesseract Download Path");
                        return new CommonResult(CommonResultCodes.Error, "Unable to obtain Tesseract's Download Path");
                        //throw new Exception("Failed to obtain Tesseract download path.");
                    }
                }


                if(bTessDataDownloadComplete)
                {
                    await UpdateSplashScreen(splashScreen, "Cleaning up temporary files...", 75);

                    Loggio.Info("Cleaning temporary files from Tesseract's Trained Models Download....");

                    await DeleteDownloadedTempFolder();
                    await Task.Delay(150);
                }

                Loggio.Info("Initializing Chest Automation...");

                await UpdateSplashScreen(splashScreen, "Intializing Chest Automation...", 80);

                await InitChestAutomation();

                await UpdateSplashScreen(splashScreen, "Building Chest Variables...", 85);

                Loggio.Info("Building Chest Builder Variables...");
                await BuildChestData();

                await UpdateSplashScreen(splashScreen, "Starting Clan Insights Server...", 90);

                Loggio.Info("Starting Clan Insights NodeJS Server...");
                                
                await StartInsightsServer();

                var language = SettingsManager.Instance.Settings.GeneralSettings.UILanguage;
                switch (language.ToLower())
                {
                    case "english":
                        LocalizationManager.Set("en-US");
                        break;
                }

                Loggio.Info($"Using culture info ('{CultureInfo.CurrentCulture.Name}')");

                await UpdateSplashScreen(splashScreen, "Launching...", 100);

                Loggio.Info("Launching Total Battle Chest Tracker...");

                return new CommonResult(CommonResultCodes.Success, "Launch");
                //await LaunchTask(splashScreen);
            }
            return new CommonResult(CommonResultCodes.Fail, "Not SplashScreenWindow.");

        }

        private async Task UpdateSplashScreen(Window window, string message, Int32 progress, int delay = 200)
        {
            if (window is SplashScreen splash)
            {
                await splash.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splash.UpdateStatus(message, progress); 
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                await Task.Delay(delay);
            }
        }
        #endregion

        #region Window Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // throw new Exception("Testing");
            InitCaptainHook();
        }
        #endregion

        public void ShowWindow()
        {
            this.Show();
            Loggio.Info("Showing Main Window");
            //-- a fancier message will appear.
            if(Snapster.MonitorConfiguration.InvalidMonitorIndexSelected)
            {
                MessageBox.Show("You may have powered off another monitor that you wanted to use for chest automation. But because it is not detected until you power on that monitor, Total Battle Chest Tracker has reverted back to your primary monitor. Please relaunch OCR Wizard to correct any issues.");
                Loggio.Info("Invalid Monitor Index Detected...");
                var PrimaryMonitorIndex = Snapster.MonitorConfiguration.Monitor.isPrimary == 1 ? Snapster.MonitorConfiguration.Monitor.ID : 0;
                SettingsManager.Instance.Settings.GeneralSettings.MonitorIndex = PrimaryMonitorIndex;
                SettingsManager.Save();
            }
            if(AppContext.Instance.RequiresOCRWizard && AppContext.Instance.UserClosedStartPageDirectly == false)
            {
                //-- prompt the user to set up OCR for the first time.
                OCRWizardWindow = new OCRWizardWindow();
                Loggio.Info("Showing OCR Window...");
                if (OCRWizardWindow != null)
                    OCRWizardWindow.Show();
            }
            if(SettingsManager.Settings.GeneralSettings.ShowOcrLanguageSelection)
            {
                OcrLanguageSelectionWindow ocrLanguageSelectionWindow = new OcrLanguageSelectionWindow(SettingsManager);
                ocrLanguageSelectionWindow.Show();
            }
        }
        
        #region CaptionHook Events

        private void CaptainHook_onInstalled(object sender, EventArgs e)
        {
            Loggio.Info("Installed Keyboard hooks successfully.");
        }


        private async Task StartAutomation()
        {
            if (AppContext.Instance.IsAutomationPlayButtonEnabled == true && AppContext.Instance.IsBuildingChests == false)
            {
                if (!AppContext.Instance.AutomationRunning)
                {
                    await ChestAutomation.StartAutomation();
                }
            }
        }

        private void StopAutomation()
        {
            if (AppContext.Instance.IsAutomationStopButtonEnabled)
            {
                if (AppContext.Instance.AutomationRunning)
                {
                    ChestAutomation.StopAutomation();
                 
                }
            }
        }
    
        #endregion

        #region Window Closing
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ChestAutomation?.Release();
            AppContext.Instance.isAppClosing = true;
            if (applicationManager?.NodeServerStarted.Value == true)
            {
                applicationManager?.KillNodeServer();
            }

            CaptainHook?.Uninstall();
            CaptainHook?.Dispose();
            ClanManager.Instance?.Destroy();
            SettingsManager?.Dispose();
            applicationManager?.Dispose();
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
                if(result)
                {
                    ExportClan.IsEnabled = true;
                    CloseDatabase.IsEnabled = true;
                }
                else
                {
                    CloseDatabase.IsEnabled = false;
                }
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
                    // com.HellStormGames.Logging.Loggy.Write($"{ex.Message}", com.HellStormGames.Logging.LogType.ERROR);
                }
            }
        }
        #endregion
        #region LoadRecentFile
        public void LoadReentFile(string file, Action<bool> response)
        {
            bool bLoaded = LoadClanDatabase(file);
            response(bLoaded);
        }

        
        public void CreateNewClan(Action<bool> response)
        {
            ClanManager = new ClanManager();
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
        
        public void ImportClanDatabase(Action<bool> response)
        {
            var clanroot = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Clan Archives | *.zip";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = clanroot;

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;

                ImportDatabaseWindow importDatabaseWindow = new ImportDatabaseWindow();
                importDatabaseWindow.SourceFile = file;
                var clanname = file.Substring(file.LastIndexOf("\\") + 1);
                clanname = clanname.Substring(0, clanname.LastIndexOf("."));
                var destFolderPath = $"{clanroot}\\{clanname}";
                var tdi = new DirectoryInfo(destFolderPath);
                if(tdi.Exists == false)
                {
                    tdi.Create();
                }
                var dest = $@"{destFolderPath}\";

                importDatabaseWindow.DestFolderPath = dest;

                if(importDatabaseWindow.ShowDialog() == true)
                {
                    var clandbFile = $@"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}\{clanname}\clan.cdb";
                    var bloaded = LoadClanDatabase(clandbFile);
                    response(bloaded);
                }
                else
                {
                    response(false);
                }
            }
            else
                response(false);
        }

        private bool LoadClanDatabase(string file)
        {
            bool bIsLoaded = false;

            if(ClanManager.Instance == null || ClanManager == null)
            {
                ClanManager = new ClanManager();
            }
            if (ClanManager.Instance.ClanChestSettings.ChestRequirements != null)
                ClanManager.Instance.ClanChestSettings.Clear();

            ClanManager.Instance.ClanDatabaseManager.Load(file, ClanManager.Instance.ClanChestManager, result =>
            {
                if (result)
                {
                    AppContext.Instance.UpdateCurrentProject($"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}");
                    AppContext.Instance.UpdateApplicationTitle();

                    var recentlyOpenedFiles = recentlyOpenedListManager.RecentClanDatabases.Select(f => f.FullClanRootFolder).ToList();

                    if (!recentlyOpenedFiles.Contains(file))
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

                        if (RecentlyOpenedParent.Items.Count <= 1)
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

                    Loggio.Info($"Loaded Clan ({ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}) successfully.");

                    AppContext.Instance.IsCurrentClandatabase = true;

                    //-- check if archive folder exist
                    var archiveFolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\archives";
                    var di = new DirectoryInfo(archiveFolder);
                    if (di.Exists)
                    {
                        //-- we need to move all files to newer folder 
                        var ChestsFolder = $"{ClanManager.CurrentProjectDirectory}\\Chests\\Data";
                        var dif = new DirectoryInfo(ChestsFolder);
                        if (dif.Exists == false)
                        {
                            dif.Create();
                        }

                        var archiveFiles = di.EnumerateFiles("*.txt");
                        foreach (var file in archiveFiles.ToList())
                        {
                            var oldFile = file.FullName;
                            var newFilename = file.Name;
                            Regex r = new Regex(@"(\d+-\d+-\d+)");
                            var matches = r.Match(newFilename);
                            string newFile = string.Empty;
                            if (matches.Success)
                            {
                                newFile = $"{ChestsFolder}\\chests_{matches.Value}.txt";
                                using (StreamReader sr = new StreamReader(oldFile))
                                {
                                    var data = sr.ReadToEnd();
                                    using (StreamWriter sw = new StreamWriter(newFile, true))
                                    {
                                        sw.Write(data);
                                        sw.Close();
                                    }
                                    sr.Close();
                                }
                            }
                        }

                        di.Delete(true);
                    }

                    var cacheFolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\cache";
                    di = new DirectoryInfo (cacheFolder);
                    if (di.Exists)
                    {
                        //-- we need to move all files to newer folder 
                        var ChestsFolder = $"{ClanManager.CurrentProjectDirectory}\\Chests\\Data";
                        var dif = new DirectoryInfo(ChestsFolder);
                        if (dif.Exists == false)
                        {
                            dif.Create();
                        }

                        var cacheFiles = di.EnumerateFiles("*.txt");
                        foreach (var file in cacheFiles.ToList())
                        {
                            var oldFile = file.FullName;
                            var newFilename = file.Name;
                            Regex r = new Regex(@"(\d+-\d+-\d+)");
                            var matches = r.Match(newFilename);
                            string newFile = string.Empty;
                            if (matches.Success)
                            {
                                newFile = $"{ChestsFolder}\\chests_{matches.Value}.txt";
                                using (StreamReader sr = new StreamReader(oldFile))
                                {
                                    var data = sr.ReadToEnd();
                                    using (StreamWriter sw = new StreamWriter(newFile, true))
                                    {
                                        sw.Write(data);
                                        sw.Close();
                                    }
                                    sr.Close();
                                }
                            }
                        }

                        di.Delete(true);
                    }

                    AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                    ExportClan.IsEnabled = true;
                    CloseDatabase.IsEnabled = true;
                    bIsLoaded = true;

                    //-- we need to check to see if we can load ClanSettings.json. If not, not big of a deal.
                    //-- If fails, we'd know it either doesn't exist or there's an issue.
                    if(ClanManager.Instance.ClanSettings.Load() == false)
                    {
                        //-- we couldn't load it. So perhaps it doesn't exist and this is the user's first time using newest version.
                        var aoirect = SettingsManager.Instance.ObtainObseleteAOIRect();
                        if (aoirect != null)
                        {
                            //-- obselete AOI data obtained.
                            ClanManager.Instance.AddOcrProfile("Default", aoirect);
                            ClanManager.Instance.SetCurrentOcrProfile("Default");
                            ClanManager.Instance.ClanSettings.Save();
                            AppContext.Instance.RequiresOCRWizard = false;
                            AppContext.Instance.OCRCompleted = true;

                            //-- now we add to OCR Profiles Menu.
                        }
                        else
                        {
                            if (AppContext.Instance.UserClosedStartPageDirectly == false)
                            {
                                ClanManager.Instance.AddOcrProfile("Default", null);
                                ClanManager.Instance.SetCurrentOcrProfile("Default");
                                AppContext.Instance.OCRCompleted = false;
                                AppContext.Instance.RequiresOCRWizard = true;
                            }
                        }
                    }
                    else
                    {
                        //-- We're able to load ClanSettings.json successfully.
                        //-- We need to obtain all the profiles. Get the current profile. 
                        //-- Also create the menu items inside OCR Profiles Menu.
                        var currentProfileName = ClanManager.Instance.GetCurrentOcrProfileName();
                        ClanManager.Instance.SetCurrentOcrProfile(currentProfileName);
                        AppContext.Instance.OCRCompleted = true;
                        AppContext.Instance.RequiresOCRWizard = false;
                    }
                }
                else
                {
                    Loggio.Warn("Wasn't able to load clan chest database. It is empty.");
                    MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                    bIsLoaded = false;
                }
            });
            ClanManager.Instance.ChestDataManager ??= new ChestDataManager();
            return bIsLoaded;
        }
        public void ShowLoadClanWindow(Action<bool> response)
        {
            if(String.IsNullOrEmpty(ClanManager.CurrentProjectDirectory) == false)
            {
                CloseClanProject();
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Clan Databases | *.cdb";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;
                bool bLoaded = LoadClanDatabase(file);
                response(bLoaded);
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
            if (AppContext.Instance.NewClandatabaseBeenCreated && AppContext.Instance.HasBuiltChests)
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
                &&AppContext.Instance.OCRCompleted && AppContext.Instance.IsBuildingChests == false)
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
            StartAutomation();
        }
        #endregion

        #region Stop Automation
        private void StopAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.NewClandatabaseBeenCreated && AppContext.Instance.OCRCompleted)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        private void StopAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopAutomation();
        }
        #endregion

        #region Clan Stats
        private void ClanStatsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ClanManager.Instance != null)
            {
                var chestsfile = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestData.csv";
                AppContext.Instance.HasBuiltChests = File.Exists(chestsfile);
            }
            else
            {
                AppContext.Instance.HasBuiltChests = false;
            }
            if (AppContext.Instance.NewClandatabaseBeenCreated && AppContext.Instance.HasBuiltChests)
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
            IChestDataCollection chestDataCollection = null;
            bool use_chest_conditions = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions != ChestOptions.UseConditions ? false : true;
            ClanManager.Instance.ChestDataManager.Load();
            var chestcollection = ClanManager.Instance.ChestDataManager.GetDatabase();

            if(use_chest_conditions)
            {
                chestDataCollection = new ChestDataConditionalCollection(chestcollection, ClanManager.Instance.ClanChestSettings.ChestRequirements);
            }
            else
            {
                chestDataCollection = new ChestDataCollection(chestcollection);
            }

            chestDataCollection.Build();
            var builtChestData = chestDataCollection.Items;
            var gameChests = new List<string>();
            var previousChest = String.Empty;
            var dateformat = AppContext.Instance.ForcedDateFormat;
            var members = new List<string>();

            foreach(var chest in ApplicationManager.Instance.Chests)
            {
                if (chest.ChestType != previousChest)
                {
                    gameChests.Add(chest.ChestType);
                    previousChest = chest.ChestType;
                }
            }

            foreach(var chestdata in builtChestData)
            {
                var d = builtChestData[chestdata.Key];
                foreach (var x in d)
                {
                    if (members.Contains(x.Key) == false)
                        members.Add(x.Key);
                }
            }

            var clan = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname;
            var usePoints = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints;
            var locale = CultureInfo.CurrentCulture.Name;

            ClanInsightsData insightsData = new ClanInsightsData(clan, members, gameChests, builtChestData, usePoints, dateformat, locale);
            var JsonStr = JsonConvert.SerializeObject(insightsData, Formatting.Indented);

            //-- we should save to where Clan Insights Server is. What happens if string is too big?
            var dataPath = $"{AppContext.Instance.LocalApplicationPath}Server\\NodeJS\\public\\ClanInsights\\data\\ClanInsights.json";
            using(var streamwriter= new StreamWriter(dataPath))
            {
                streamwriter.Write(JsonStr);
                streamwriter.Close();
            }
        }
        private void ClanStatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrepareClanInsightsData();
            WebView.Show("Clan Insights", "http://127.0.0.1:8888/View/ClanInsights", 800,800, false, true, true);
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
            using(var image = Snapster.CaptureDesktop()) {
                try
                {
                    SettingsWindow.MonitorPreviewImage = image.ToBitmap().AsBitmapSource();
                }
                catch(Exception ex)
                {
                    Loggio.Error(ex, "Monitor Preview", "Couldn't capture desktop for monitor preview. More details in log.");
                }
            }

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

        private void ExportClan_Click(object sender, RoutedEventArgs e)
        {

            var clanpath = $@"{ClanManager.Instance.CurrentProjectDirectory}";
            var archivename = $@"{ClanManager.ClanDatabaseManager.ClanDatabase.Clanname}.zip";
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ExportDatabaseWindow exportDatabaseWindow = new ExportDatabaseWindow();
                exportDatabaseWindow.DestinationFilePath = $@"{dialog.FileName}\{archivename}";
                exportDatabaseWindow.SourceFolderPath = clanpath;
                exportDatabaseWindow.Show();
            }
        }

        private void RebuildTessData_Click(object sender, RoutedEventArgs e)
        {
            AppContext.RestartApplication("--delete_tessdata");
        }

        private void BuildChests_Click(object sender, RoutedEventArgs e)
        {
            BuildClanChests();
        }

        private void ReloadStartPage()
        {
            if (startPageWindow != null)
            {
                startPageWindow.Close();
                startPageWindow = null;
            }

            startPageWindow = new StartPageWindow();

            startPageWindow.MainWindow = this;
            startPageWindow.Show();
            this.Hide();
        }
        private void CloseClanProject()
        {
            ClanManager.UnloadClan();
        }
        private void CloseDatabase_Click(object sender, RoutedEventArgs e)
        {
            CloseClanProject();
            ReloadStartPage();
        }
        
        private void OcrCorrectionToolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OcrCorrectionWindow ocrCorrectionWindow
                = new OcrCorrectionWindow();

            ocrCorrectionWindow.Show();
        }

        private void BuildClanChestsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool? isAutomatic = SettingsManager?.Settings?.AutomationSettings?.BuildChestsAfterStoppingAutomation;
            isAutomatic ??= false;
            var chestsDirectory = $"{ClanManager?.CurrentProjectDirectory}\\Chests\\Data";
            var chestsDI = new DirectoryInfo(chestsDirectory);
            bool hasChestFiles = false;
            if(chestsDI.Exists)
            {
                hasChestFiles = chestsDI.EnumerateFiles("chests_*.txt").Count() > 0;
            }

            if(AppContext.Instance.NewClandatabaseBeenCreated && hasChestFiles && isAutomatic == false)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BuildClanChestsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BuildClanChests();
        }

        private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)e.OriginalSource;
            var menutag = mi.Tag;
            switch (menutag)
            {
                case "ClansRoot":
                    Process.Start($"{AppContext.Instance.LocalApplicationPath}Clans\\");
                    break;
                case "LogFiles":
                    Process.Start($"{AppContext.Instance.LocalApplicationPath}Logs\\");
                    break;
            }
        }
        private void DoBug()
        {
            var a = new string[5];
            a[7] = "Meow!";
        }
        private void CauseCrashMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DoBug();            
        }

        private void MergeChestDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChestDataMergerWindow chestDataMergerWindow = new ChestDataMergerWindow();
            chestDataMergerWindow.Show();
        }

        private void CleanClanmates_Click(object sender, RoutedEventArgs e)
        {
            ClanmateCleaningWindow clanmateCleaningWindow = new ClanmateCleaningWindow();
            clanmateCleaningWindow.Show();
        }
    }
}
