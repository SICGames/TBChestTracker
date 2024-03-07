using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Windows.Globalization;
using Windows.Graphics.Imaging;
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
using com.CaptainHookSharp;
using CaptainHookInterlop = com.CaptainHookSharp.Interop;

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
        CaptureMode CaptureMode { get; set; }
        bool isAutomationBusy = false;
        int captureCounter = 0;
        public ClanManager ClanManager { get; private set; }
        bool isClosing = false;
        bool stopAutomation = false;
        private double CLANCHEST_IMAGE_BRIGHTNESS = 0.65d;
        private CaptainHook CaptainHook { get; set; }
        public Snapture Snapture { get; private set; }  
        
        public Version AppVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        private string pAppTitle = $"";
        public string AppTitle
        {
            get
            {
                return pAppTitle;
            }
            set
            {
                pAppTitle = value;
                OnPropertyChanged(nameof(AppTitle));
            }
        }

        private bool isAutomationPlayButtonEnabled = false;
        private bool isAutomationPauseButtonEnabled = false;    
        private bool isAutomationStopButtonEnabled = false;
        private bool isCurrentClandatabase = false;

        public bool IsCurrentClandatabase
        {
            get => isCurrentClandatabase;
            set
            {
                isCurrentClandatabase = value;
                OnPropertyChanged(nameof(IsCurrentClandatabase));
            }
        }
        public bool IsAutomationPlayButtonEnabled
        {
            get => isAutomationPlayButtonEnabled;
            set
            {
                isAutomationPlayButtonEnabled = value;
                OnPropertyChanged(nameof(IsAutomationPlayButtonEnabled));   
            }
        }
        public bool IsAutomationStopButtonEnabled
        {
            get => isAutomationStopButtonEnabled;
            set
            {
                isAutomationStopButtonEnabled = value;
                OnPropertyChanged(nameof(IsAutomationStopButtonEnabled));
            }
        }

        List<string> recently_opened_files { get; set; }
        ConsoleWindow consoleWindow { get; set; }
        
        CancellationTokenSource CancellationTokenSource { get; set; }

        private int _ChestCountTotal = 0;
        public int ChestCountTotal
        {
            get
            {
                return _ChestCountTotal;
            }
            set
            {
                _ChestCountTotal = value;
                OnPropertyChanged(nameof(ChestCountTotal));
            }
        }

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
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;
            recently_opened_files = new List<string>();
            consoleWindow = new ConsoleWindow();
        }
        #endregion

        #region Image Processing Functions & Other Related Functions
        private Task<TessResult> GetTextFromBitmap(System.Drawing.Bitmap bitmap)
        {
            
            Image<Gray, Byte> image = null;
            Image<Gray, Byte> imageOut = null;
            var brightness = 0.0d;
            if (CaptureMode == CaptureMode.CLANMATES)
            {
                brightness = 0.5d;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
            
            }
            else
            {
                brightness = CLANCHEST_IMAGE_BRIGHTNESS;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
            }
            var ocrResult = TesseractHelper.Read(imageOut);
            
            imageOut.Dispose();
            imageOut = null;
            image.Dispose();
            image = null;
            
            
            return Task.FromResult(ocrResult);

            
        }
        private void CaptureRegion()
        {
            //-- now everything is completely manual when capturing. CLI C++ doesn't do any while loop.
            int sh = Snapture.ScreenHeight;
            int sw = Snapture.ScreenWidth;
            int x = sw / 2; //-- 1920 
            int y = sh / 2; //-- 1080

            var leftChestPerc = 0.816d;
            var topChestPerc = 0.7004d;
            var widthChestPerc = 0.452d;
            var heightChestPerc = 0.724d;

            int left = (int)Math.Round(x * leftChestPerc);
            int top = (int)Math.Round(y * topChestPerc);
            int width = (int)Math.Round(x * widthChestPerc);
            int height = (int)Math.Round(y * heightChestPerc);

            CaptureMode = CaptureMode.CHESTS;
            Snapture.CaptureRegion(left, top, width, height);
            GlobalDeclarations.canCaptureAgain = false;

        }
        public void CaptureDesktop()
        {
            CaptureMode = CaptureMode.CHESTS;
            Snapture.CaptureDesktop();
            GlobalDeclarations.canCaptureAgain = false;
        }
        public void CaptureClanmates()
        {
            //-- now everything is completely manual when capturing. CLI C++ doesn't do any while loop.
            int sh = Snapture.ScreenHeight;
            int sw = Snapture.ScreenWidth;
            int x = sw / 2; //-- 1920 
            int y = sh / 2; //-- 1080

            double topClanmatePerc = 0.7026;
            double leftClanmatePerc = 0.716d;
            double widthClanmatePerc = 0.7338d;
            double heightClanmatePerc = 0.6667;
            int left = (int)Math.Round(x * leftClanmatePerc);
            int top = (int)Math.Round(y * topClanmatePerc);
            int width = (int)Math.Round(x * widthClanmatePerc);
            int height = (int)Math.Round(y * heightClanmatePerc);

            CaptureMode = CaptureMode.CLANMATES;

            Snapture.CaptureRegion(left, top, width, height);
        }
       
        #endregion

        #region Start Automation Thread
        void StartAutomationThread()
        {
            stopAutomation = false;
            GlobalDeclarations.AutomationRunning = true;
            CancellationTokenSource = new CancellationTokenSource();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Task automationTask = Task.Run(() => StartAutomationProcess(CancellationTokenSource.Token));

            }));
        }
        #endregion

        #region StartAutomation 
        void StartAutomationProcess(CancellationToken token)
        {
            //--- take snapshot.
            //--- mouse click 4 times on claim button
            //--- take snapshot 
            //--- rinse and repeat UNTIL F8 key is pressed by user to end it.
            var screenX_half = Snapture.ScreenWidth / 2;
            var screenY_half = Snapture.ScreenHeight / 2;

            var xClickPos = new Point((screenX_half * 0.706) * 2, screenY_half * 0.834);
            var x = (int)xClickPos.X;
            var y = (int)xClickPos.Y;

            com.HellStormGames.Logging.Console.Write("Automation Started", com.HellStormGames.Logging.LogType.INFO);
            GlobalDeclarations.canCaptureAgain = true;

            ChestCountTotal = 0;

            bool bCanceled = false;
            while (!stopAutomation)
            {
                int automatorClicks = 0;
                if (token.IsCancellationRequested)
                {
                    bCanceled = true;
                    break;
                }
                    

                if (GlobalDeclarations.canCaptureAgain)
                {
                    Thread.Sleep(1250); //-- could prevent the "From:" bug.
                    CaptureRegion();
                    while (ClanManager.Instance.ClanChestManager.ChestProcessingState != ChestProcessingState.COMPLETED)
                    {

                    }
                    GlobalDeclarations.canCaptureAgain = false;
                    captureCounter++;
                }

                if (GlobalDeclarations.isAnyGiftsAvailable == false)
                    continue;

                while(automatorClicks != 4)
                {
                    Automator.LeftClick(x, y);
                    automatorClicks++;
                    
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        ChestCountTotal++;
                        }));
                    
                    Thread.Sleep(100);
                }
                
                //-- canCaptureAgain not being switched back on.
                GlobalDeclarations.canCaptureAgain = true;
            }
            if(bCanceled)
            {
                StopAutomation();
            }

        }
        #endregion

        #region StopAutomation
        void StopAutomation()
        {
            CancellationTokenSource.Cancel();
            stopAutomation = true;
            GlobalDeclarations.AutomationRunning = false;

            ClanManager.Instance.ClanChestManager.SaveDataTask();
            ClanManager.Instance.ClanChestManager.CreateBackup();
            IsAutomationPlayButtonEnabled = true;
            IsAutomationStopButtonEnabled = false;
            com.HellStormGames.Logging.Console.Write("Automation stopped.", com.HellStormGames.Logging.LogType.INFO);
        }
        #endregion

        #region Snapture onFrameCaptured Event
        private async void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
#if SNAPTURE_DEBUG
            var outputpath = $@"{GlobalDeclarations.AppFolder}Test\SnaptureDebug.jpg";
            System.Drawing.Bitmap debug_screenshot = e.ScreenCapturedBitmap;
            debug_screenshot.Save($@"{outputpath}", ImageFormat.Jpeg);

            var ocrResult = await GetTextFromBitmap(e.ScreenCapturedBitmap); //LoadBitmap(e.ScreenCapturedBitmap, new Windows.Globalization.Language("en"));
            if (ocrResult == null)
            {
            }
            foreach(var word in ocrResult.Words)
            {
                Debug.WriteLine($"{word}");
            }
            //-- simulate fake left mouse click by using MoveTo function.
            var screenX_half = Snapture.ScreenWidth / 2;
            var screenY_half = Snapture.ScreenHeight / 2;

            var xClickPos = new Point((screenX_half * 0.706) * 2, screenY_half * 0.834);
            var x = (int)xClickPos.X;
            var y = (int)xClickPos.Y;

            Automator.MoveTo((int)x, y);
            e.ScreenCapturedBitmap.Dispose();
#elif !SNAPTURE_DEBUG 
            var ocrResult = await GetTextFromBitmap(e.ScreenCapturedBitmap); //LoadBitmap(e.ScreenCapturedBitmap, new Windows.Globalization.Language("en"));
            if (ocrResult == null)
            {
            }

            //-- here we process data.
            e.ScreenCapturedBitmap.Dispose();

            if (CaptureMode == CaptureMode.CHESTS && ocrResult != null)
                ClanManager.Instance.ClanChestManager.ProcessChestData(ocrResult.Words, onError =>
                {
                    if (onError)
                    {
                        var result = MessageBox.Show("A critical error has occured during processing text from image. Stopping automation and saving chest data.", "OCR Error", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                            StopAutomation();
                    }
                });
#endif
        }
#endregion
     
        #region Window Loaded
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CaptainHook = new CaptainHook();
            CaptainHook.onKeyboardMessage += CaptainHook_onKeyboardMessage;
            CaptainHook.onInstalled += CaptainHook_onInstalled;
            CaptainHook.onError += CaptainHook_onError;
            CaptainHook.Install();
            
            
            Snapture = new Snapture();
            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
#if !SNAPTURE_DEBUG
            Snapture.SetBitmapResolution((int)Snapture.MonitorInfo.Monitors[0].Dpi.X);
#elif SNAPTURE_DEBUG
            Snapture.SetBitmapResolution(96);
#endif
            Snapture.Start(FrameCapturingMethod.GDI);

            com.HellStormGames.Logging.Console.Write("Snapture Started.", com.HellStormGames.Logging.LogType.INFO);

            AppTitle = $"TotalBattle Chest Tracker v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} - Untitled";

            this.ClanManager = new ClanManager();
            
            if (System.IO.Directory.Exists(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath) == false)
            {
                System.IO.Directory.CreateDirectory(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath);
            }

            if (File.Exists("recent.lst"))
            {
                using(var sr = File.OpenText("recent.lst"))
                {
                    var data = sr.ReadToEnd();
                    if (data.Contains("\r\n"))
                    {
                        data = data.Replace("\r\n", ",");
                    }
                    else
                        data = data.Replace("\n", ",");

                    var list = data.Split(',');
                    
                    foreach(var file in list)
                    {
                        if(string.IsNullOrEmpty(file))
                            continue;

                        MenuItem mu = new MenuItem();
                        var position = StringHelpers.findNthOccurance(file, Convert.ToChar(@"\"), 3);
                        var truncated = StringHelpers.truncate_file_name(file, position);
                        mu.Header = truncated;
                        mu.Tag = file;
                        mu.Click += Mu_Click;
                        RecentlyOpenedParent.Items.Add(mu);
                        recently_opened_files.Add(file);
                    }
                }

                //-- add seperator to recently opened clan databases.
                Separator separator = new Separator();
                RecentlyOpenedParent.Items.Add(separator);
                MenuItem mi = new MenuItem();
                mi.Tag = "CLEAR_HISTORY";
                mi.Header = "Clear Recent Clan Databases";
                mi.Click += Mu_Click;
                RecentlyOpenedParent.Items.Add(mi);
            }
            else
            {

            }

            if (System.IO.Directory.Exists(GlobalDeclarations.TesseractData))
            {
                GlobalDeclarations.TessDataExists = true;
                await TesseractHelper.InitAsync(GlobalDeclarations.TesseractData, "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus+pol+por+pus");
            }
            else
            {
                //-- tessdata folder needs to exist. And if not, should be prevented from even attempting to do any OCR.
                MessageBox.Show($"No Tessdata directory exists. Download tessdata and ensure all traineddata is inside tessdata.");
                GlobalDeclarations.TessDataExists = false;
            }

            StartPageWindow startPageWindow = new StartPageWindow();
            startPageWindow.MainWindow = this;
            startPageWindow.Show();
            //var translation = Translator.Translate("I'm So Silly.", "en", "fr");

        }
        #endregion
        #region CaptionHook Events
        private void CaptainHook_onError(object sender, CaptainHook.HookErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CaptainHook_onInstalled(object sender, EventArgs e)
        {
            com.HellStormGames.Logging.Console.Write("Installed Keyboard hooks successfully.", com.HellStormGames.Logging.LogType.INFO);
        }

        private void CaptainHook_onKeyboardMessage(object sender, CaptainHook.KeyboardHookMessageEventArgs e)
        {
            int key = e.VirtKeyCode;
            switch (e.MessageType)
            {
                case CaptainHookInterlop.KeyboardMessage.KeyDown:
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                    break;
                case CaptainHookInterlop.KeyboardMessage.KeyUp:
                    GlobalDeclarations.hasHotkeyBeenPressed = false;
                    break;
            }
            
            if (!GlobalDeclarations.hasHotkeyBeenPressed && !GlobalDeclarations.isBusyProcessingClanchests)
            {
#if !SNAPTURE_DEBUG
                if (key == KeyInterop.VirtualKeyFromKey(Key.F9))
                {
                    StartAutomationThread();
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                    IsAutomationPlayButtonEnabled = false;
                    IsAutomationStopButtonEnabled = true;
                }
                else if (key == KeyInterop.VirtualKeyFromKey(Key.F10))
                {
                    StopAutomation();
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                   
                }
#elif SNAPTURE_DEBUG
                if(key == KeyInterop.VirtualKeyFromKey(Key.F2))
                {
                    CaptureRegion();
                    GlobalDeclarations.hasHotkeyBeenPressed= true;
                }
#endif
            }
        }
        #endregion

        #region Window Closing
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            if (CaptainHook.Uninstall())
            {
             
            }
            ClanManager.Instance.Destroy();
            TesseractHelper.Destroy();
            com.HellStormGames.Logging.Console.Destroy();

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
                LoadReentFile(file, null);
            }
            else
            {
                try
                {
                    System.IO.File.Delete("recent.lst");
                    RecentlyOpenedParent.Items.Clear();
                    recently_opened_files.Clear();

                }
                catch(Exception ex)
                {

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

                    AppTitle = $"TotalBattle Chest Tracker v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} - {ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}";
                    IsCurrentClandatabase = true;
                    IsAutomationPlayButtonEnabled = true;
                    response(true);
                }
                else
                {
                    MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                }
            });
        }

        private void UpdateAppTitle()
        {
            AppTitle = $"TotalBattle Chest Tracker v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} - {ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}";
        }
        public void CreateNewClan(Action<bool> response)
        {
            NewClanDatabaseWindow newClanDatabaseWindow = new NewClanDatabaseWindow();
            if (newClanDatabaseWindow.ShowDialog() == true)
            {
                GlobalDeclarations.hasNewClanDatabaseCreated = true;
                UpdateAppTitle();
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
                        UpdateAppTitle();

                        //AppTitle = $"TotalBattle Chest Tracker v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} - {ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}";
                        if (!recently_opened_files.Contains(file, StringComparer.CurrentCultureIgnoreCase))
                        {
                            recently_opened_files.Add(file);
                            MenuItem mu = new MenuItem();
                            var position = StringHelpers.findNthOccurance(file, Convert.ToChar(@"\"), 3);
                            mu.Header = StringHelpers.truncate_file_name(file, position);
                            mu.Tag = file;
                            mu.Click += Mu_Click;
                            RecentlyOpenedParent.Items.Add(mu);
                            using (var tw = File.CreateText("recent.lst"))
                            {
                                foreach (var f in recently_opened_files)
                                {
                                    tw.WriteLine(f);
                                }
                            }
                        }

                        com.HellStormGames.Logging.Console.Write($"Loaded Clan ({ClanManager.Instance.ClanDatabaseManager.ClanDatabase.Clanname}) Database Successfully.",
                            com.HellStormGames.Logging.LogType.INFO);

                        IsCurrentClandatabase = true;
                        IsAutomationPlayButtonEnabled = true;
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
            if (GlobalDeclarations.hasNewClanDatabaseCreated)
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
            if(GlobalDeclarations.hasNewClanDatabaseCreated)
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
            if (GlobalDeclarations.hasNewClanDatabaseCreated && GlobalDeclarations.hasClanmatesBeenAdded)
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
            e.CanExecute = GlobalDeclarations.hasNewClanDatabaseCreated;
        }

        private void ManageClanmatesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddClanmatesWindow addClanmatesWindow = new AddClanmatesWindow();
            if (addClanmatesWindow.ShowDialog() == true)
            {
                GlobalDeclarations.hasClanmatesBeenAdded = true;
            }
        }
        #endregion

        #region Manage Clan Chests Settings
        private void ManageClanchestSettingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalDeclarations.hasNewClanDatabaseCreated;
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
            if (GlobalDeclarations.hasClanmatesBeenAdded
                && GlobalDeclarations.hasNewClanDatabaseCreated
                && GlobalDeclarations.TessDataExists)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        private void StartAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!GlobalDeclarations.AutomationRunning
                && GlobalDeclarations.TessDataExists)
            {
                StartAutomationThread();
                IsAutomationStopButtonEnabled = true;
                IsAutomationPlayButtonEnabled = false;
            }
        }
        #endregion

        #region Stop Automation
        private void StopAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasClanmatesBeenAdded 
                && GlobalDeclarations.hasNewClanDatabaseCreated 
                && GlobalDeclarations.TessDataExists)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        private void StopAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalDeclarations.AutomationRunning)
            {
                StopAutomation();
                IsAutomationStopButtonEnabled = false;
                IsAutomationPlayButtonEnabled = true;
            }
        }
        #endregion

        #region Clan Stats
        private void ClanStatsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasClanmatesBeenAdded && GlobalDeclarations.hasNewClanDatabaseCreated)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void ClanStatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClanStatisticsWindow clanStatisticsWindow = new ClanStatisticsWindow();
            clanStatisticsWindow.ChestManager = ClanManager.Instance.ClanChestManager;
            clanStatisticsWindow.Show();    
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
                IsCurrentClandatabase = true;
                IsAutomationPlayButtonEnabled = true;
            }
        }
        #endregion

        private void ConsoleMenuButton_Click(object sender, RoutedEventArgs e)
        {
            consoleWindow.Show();
        }

        #endregion
    }
}
