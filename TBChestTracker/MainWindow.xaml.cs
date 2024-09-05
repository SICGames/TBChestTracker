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


namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
        RecentDatabase recentlyOpenedDatabases { get; set; }
        ApplicationManager applicationManager { get; set; }
        Task AutomationTask { get; set; }

        OCREngine OCREngine { get; set; }
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

            this.DataContext = AppContext.Instance;
            this.Closing += MainWindow_Closing;
            recentlyOpenedDatabases = new RecentDatabase();
            consoleWindow = new ConsoleWindow();
        }
        #endregion

        #region Image Processing Functions & Other Related Functions
        private Task<TessResult> GetTextFromBitmap(System.Drawing.Bitmap bitmap)
        {
            Image<Gray, Byte> image = null;
            Image<Gray, Byte> imageOut = null;
            var ocrSettings = SettingsManager.Instance.Settings.OCRSettings;
            var brightness = ocrSettings.GlobalBrightness;
            image = bitmap.ToImage<Gray, Byte>();
            imageOut = image.Mul(brightness) + brightness;

            //-- OCR Incorrect Text Bug - e.g. Slash Jr III is read Slash )r III
            //-- Fix: Upscaling input image large enough to read properly.
            var imageScaled = imageOut.Resize(5, Emgu.CV.CvEnum.Inter.Cubic);

            var threshold = new Gray(ocrSettings.Threshold); //-- 85
            var maxThreshold = new Gray(ocrSettings.MaxThreshold); //--- 255
            var imageThreshold = imageScaled.ThresholdBinaryInv(threshold, maxThreshold);

            //-- if it is null or empty somehow, we update it.
            if (String.IsNullOrEmpty(ocrSettings.PreviewImage))
            {
                imageThreshold.Save($@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png");
                ocrSettings.PreviewImage = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png";
            }

            if (AppContext.Instance.SaveOCRImages)
            {
                imageScaled.Save($"OCR_ImageScaled.png");
                imageOut.Save($"OCR_ImageOut.png");
                imageThreshold.Save($"OCR_Threshold.png");
            }
            
            var ocrResult = OCREngine.Read(imageThreshold);
            
            imageThreshold.Dispose();
            imageScaled.Dispose();
            imageOut.Dispose();
            image.Dispose();

            imageThreshold = null;
            imageScaled = null;
            imageOut = null;
            image = null;

            return Task.FromResult(ocrResult);
        }
        private void CaptureRegion()
        {
            CaptureMode = CaptureMode.CHESTS;
            var capture_region = SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest;
            
            int ca_x = (int)capture_region.x;
            int ca_y = (int)capture_region.y;
            int ca_width = (int)capture_region.width;
            int ca_height = (int)capture_region.height;

            Snapture.CaptureRegion(ca_x, ca_y, ca_width, ca_height);

            AppContext.Instance.canCaptureAgain = false;

        }
        public void CaptureDesktop()
        {
            CaptureMode = CaptureMode.CHESTS;
            Snapture.CaptureDesktop();
            AppContext.Instance.canCaptureAgain = false;
        }
       
        #endregion

        #region Start Automation Thread
        void StartAutomationThread()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                AppContext.Instance.AutomationRunning = true;

                CancellationTokenSource = new CancellationTokenSource();
                AutomationTask = Task.Run(() => StartAutomationProcess(CancellationTokenSource.Token));
                if (AutomationTask.IsCompleted)
                {
                    Debug.WriteLine("Automation is completed");
                }
                else if (AutomationTask.IsCanceled)
                {
                    Debug.WriteLine("Automation is canceled");
                }

            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        #endregion

        #region StartAutomation 
        void StartAutomationProcess(CancellationToken token)
        {

            var claim_button = SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons[0];
            com.HellStormGames.Logging.Console.Write("Automation Started", com.HellStormGames.Logging.LogType.INFO);
            AppContext.Instance.canCaptureAgain = true;

            //-- we want to speed up this.
            while (!token.IsCancellationRequested)
            {

                int automatorClicks = 0;
                if (AppContext.Instance.canCaptureAgain)
                {
                    Thread.Sleep(1250); //-- could prevent the "From:" bug.

                    CaptureRegion();

                    while (ClanManager.Instance.ClanChestManager.ChestProcessingState != ChestProcessingState.COMPLETED)
                    {
                    }
                }

                if (clanChestProcessResult.Result == ClanChestProcessEnum.SUCCESS)
                {
                    while (automatorClicks != 4)
                    {
                        Automator.LeftClick(claim_button.X, claim_button.Y);
                        automatorClicks++;
                        Thread.Sleep(100);
                    }

                    //-- canCaptureAgain not being switched back on.
                    AppContext.Instance.canCaptureAgain = true;
                }
            }

            StopAutomation();
        }
        #endregion

        #region StopAutomation
        void StopAutomation()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                CancellationTokenSource.Cancel();

                AppContext.Instance.AutomationRunning = false;
                AppContext.Instance.isBusyProcessingClanchests = false;
                ClanManager.Instance.ClanChestManager.SaveDataTask();
                ClanManager.Instance.ClanChestManager.CreateBackup();
                AppContext.Instance.IsAutomationPlayButtonEnabled = true;
                AppContext.Instance.IsAutomationStopButtonEnabled = false;
                com.HellStormGames.Logging.Console.Write("Automation stopped.", com.HellStormGames.Logging.LogType.INFO);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }
        #endregion

        #region Snapture onFrameCaptured Event
        private async void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            var ocrResult = await GetTextFromBitmap(e.ScreenCapturedBitmap); //LoadBitmap(e.ScreenCapturedBitmap, new Windows.Globalization.Language("en"));
            if (ocrResult == null)
            {

            }

            //-- here we process data.
            e.ScreenCapturedBitmap.Dispose();

            if (CaptureMode == CaptureMode.CHESTS && ocrResult != null)
            {
               clanChestProcessResult = ClanManager.Instance.ClanChestManager.ProcessChestData(ocrResult.Words, onError =>
                {
                    if (onError != null && !String.IsNullOrEmpty(onError.Message))
                    {
                        
                        var result = MessageBox.Show($"Stopping automation and saving chest data. Reason: {onError.Message}", "OCR Error", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                        {
                            com.HellStormGames.Logging.Console.Write($"Error Occured Processing Chests. Automation Stopped.", "Automation Result", com.HellStormGames.Logging.LogType.ERROR);
                            StopAutomation();
                        }
                    }
                });
                if(clanChestProcessResult.Result == ClanChestProcessEnum.NO_GIFTS)
                {
                    StopAutomation();
                    com.HellStormGames.Logging.Console.Write($"There are no more gifts to collect.", "Automation Result", com.HellStormGames.Logging.LogType.INFO);
                }
            }
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

        private Task InitSnapture()
        {
            return Task.Run(() =>
            {
                Snapture = new Snapture();
                Snapture.onFrameCaptured += Snapture_onFrameCaptured;
                var dpi = 600; //-- testing new dpi.

                Snapture.SetBitmapResolution(dpi);
                Snapture.Start(FrameCapturingMethod.GDI);
                com.HellStormGames.Logging.Console.Write("Snapture Started.", com.HellStormGames.Logging.LogType.INFO);
            });
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
            return Task.Run(async () =>
            {
                SettingsManager = new SettingsManager();

                /*
                var settingsFile = $@"{AppContext.Instance.CommonAppFolder}\Settings.json";

                var isFirstRun = await IsFirstLaunch();

                if (isFirstRun)
                {
                    SettingsManager.Instance.BuildDefaultConfiguration();
                }
                else
                {
                    if (System.IO.File.Exists(settingsFile) == false)
                    {
                        SettingsManager.Instance.BuildDefaultConfiguration();
                    }
                }

                SettingsManager.Load();
                */

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
                if (String.IsNullOrEmpty(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder))
                {
                    SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath;
                }

                if (System.IO.Directory.Exists(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder) == false)
                {
                    System.IO.Directory.CreateDirectory(SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder);
                }

                if (File.Exists(AppContext.Instance.RecentOpenedClanDatabases))
                {
                    
                    if(recentlyOpenedDatabases.Load())
                    {
                        foreach(var recent in recentlyOpenedDatabases.RecentClanDatabases)
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

        private Task InitTesseract()
        {
            return Task.Run(() =>
            {
                if (System.IO.Directory.Exists(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder))
                {
                    AppContext.Instance.TessDataExists = true;
                    var languages = SettingsManager.Instance.Settings.OCRSettings.Languages;
                    OCREngine = new OCREngine();

                    OCREngine.Init(SettingsManager.Instance.Settings.OCRSettings);
                }
                else
                {
                    //-- tessdata folder needs to exist. And if not, should be prevented from even attempting to do any OCR.
                    MessageBox.Show($"No Tessdata directory exists. Download tessdata and ensure all traineddata is inside tessdata.");
                    AppContext.Instance.TessDataExists = false;
                }
            });
        }

        private Task BuildChestData()
        {
            return Task.Run(() =>
            {
                applicationManager = new ApplicationManager();
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

        public async void Init(Window window)
        {
            if (window is SplashScreen splashScreen)
            {

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initalizing...", 0);
                }));
                
                SettingsManager = await InitSettings();

                if(SettingsManager == null)
                {
                    throw new Exception("SettingsManager is null");
                }

                if(String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.TessDataFolder))
                {
                    throw new Exception("Unable to correctly populate settings.");
                }

//                await Task.Delay(2000);

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initializing Captain Hook...", 25);
                }));

                await Task.Delay(500);
                
                InitCaptainHook();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initializing Snapture...", 40);
                }));
                await Task.Delay(500);
                await InitSnapture();
              

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Tidying everything up...", 90);
                
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
                        splashScreen.UpdateStatus("Upgrading Clan databases...", 94);
                    }));
                    await Task.Delay(500);
                    await UpgradeClanDatabases(databasesNeedUpgrade);

                }
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Initializing Tesseract...", 95);
                }));

                await Task.Delay(500);

                await InitTesseract();

                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus("Building Chest Variables...", 97);
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

                await Task.Delay(2000);

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
            if(AppContext.Instance.IsFirstRun || AppContext.Instance.RequiresOCRWizard)
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
                        StartAutomationThread();
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        AppContext.Instance.IsAutomationPlayButtonEnabled = false;
                        AppContext.Instance.IsAutomationStopButtonEnabled = true;

                        keyStr = String.Empty;
                        previousKey = Key.None;
                        newKey = Key.None;
                    }
                    if(keyStr.Equals(SettingsManager.Instance.Settings.HotKeySettings.StopAutomationKeys))
                    {
                        StopAutomation();
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        keyStr = String.Empty;
                        previousKey = Key.None;
                        newKey = Key.None;
                    }
#if DEBUG
                    else if(keyStr.Equals(Key.F8.ToString()))
                    {
                        CaptureRegion();
                        AppContext.Instance.hasHotkeyBeenPressed = true;
                        keyStr = String.Empty;
                        previousKey= Key.None;
                        newKey= Key.None;
                    }
#endif
                    
                    keyStr = String.Empty;
                    bIsNewKey = false;

                }
            }
        }

        #endregion

        #region Window Closing
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppContext.Instance.isAppClosing = true;
            applicationManager.KillNodeServer();
            CaptainHook.Uninstall();
            
            ClanManager.Instance.Destroy();
            OCREngine.Destroy();
            com.HellStormGames.Logging.Console.Destroy();
            SettingsManager.Dispose();
            applicationManager = null;
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
                    System.IO.File.Delete($@"{AppContext.Instance.RecentOpenedClanDatabases}");
                    RecentlyOpenedParent.Items.Clear();
                    recentlyOpenedDatabases.RecentClanDatabases.Clear();
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
            openFileDialog.InitialDirectory = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath;

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

                        var recentlyOpenedFiles = recentlyOpenedDatabases.RecentClanDatabases.Select(f => f.FullClanRootFolder).ToList();

                        if(!recentlyOpenedFiles.Contains(file))
                        {
                            RecentClanDatabase recentdb = new RecentClanDatabase();
                            recentdb.ClanAbbreviations = ClanManager.ClanDatabaseManager.ClanDatabase.ClanAbbreviations;
                            recentdb.ClanName = ClanManager.ClanDatabaseManager.ClanDatabase.Clanname;
                            var position = StringHelpers.findNthOccurance(file, Convert.ToChar(@"\"), 3);
                            recentdb.ShortClanRootFolder = StringHelpers.truncate_file_name(file, position);
                            recentdb.FullClanRootFolder = file;
                            recentdb.LastOpened = DateTime.Now.ToFileTimeUtc().ToString();
                            recentlyOpenedDatabases.RecentClanDatabases.Add(recentdb);

                            MenuItem mu = new MenuItem();
                            mu.Header = StringHelpers.truncate_file_name(file, position);
                            mu.Tag = file;
                            mu.Click += Mu_Click;
                            RecentlyOpenedParent.Items.Add(mu);

                            recentlyOpenedDatabases.Save();

                        }

                        com.HellStormGames.Logging.Console.Write($"Loaded Clan ({ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}) Database Successfully.",
                            com.HellStormGames.Logging.LogType.INFO);

                        AppContext.Instance.IsCurrentClandatabase = true;
                        AppContext.Instance.IsAutomationPlayButtonEnabled = true;
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
                && AppContext.Instance.ClanmatesBeenAdded && AppContext.Instance.OCRCompleted)
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
                StartAutomationThread();
                AppContext.Instance.IsAutomationStopButtonEnabled = true;
                AppContext.Instance.IsAutomationPlayButtonEnabled = false;
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
                StopAutomation();
                AppContext.Instance.IsAutomationStopButtonEnabled = false;
                AppContext.Instance.IsAutomationPlayButtonEnabled = true;
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
        private void ValidateChestData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AppContext.Instance.ClanmatesBeenAdded)
                e.CanExecute = true;
            else 
                e.CanExecute = false;
        }

        private void ValidateChestData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ValidateChestDataWindow validateChestDataWindow = new ValidateChestDataWindow();
            validateChestDataWindow.Show();
        }
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

            if (_tag.Equals("AboutApp"))
            {
                AboutWindow aboutWindow = new AboutWindow();
                aboutWindow.Show();
            }

        }

        private void Patreon_Click(object sender, RoutedEventArgs e)
        {
            Process.Start($"https://www.patreon.com/TotalBattleGuide");
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
    }
}
