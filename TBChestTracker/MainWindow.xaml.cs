using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;

using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Emgu.CV.Cuda;
using System.Text;

using com.HellScape.ScreenCapture;
using CaptainHookSharp;
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
using System.Windows.Navigation;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public enum CaptureMode
    {
        CHESTS = 0,
        CLANMATES = 1
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Declarations
        System.Threading.Thread InputHookThread { get; set; }
        System.Threading.Thread AutomationThread { get; set; }
        CaptureMode CaptureMode { get; set; }
        
        public ClanChestManager ClanChestManager { get; set; }

        bool isClosing = false;
        bool stopAutomation = false;
        private double CLANCHEST_IMAGE_BRIGHTNESS = 0.75d;

        private string pAppTitle = $"TotalBattle Chest Tracker v0.1.0 - Untitled";
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

        List<string> recently_opened_files { get; set; }


        Tesseract tessy { get; set; }
        #endregion

        #region PropertyChanged Event
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;
            recently_opened_files = new List<string>();

            if(System.IO.Directory.Exists(ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath) == false)
            {
                System.IO.Directory.CreateDirectory(ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath);
            }
        }

        #region Image Processing Functions & Other Related Functions

        private Task<String[]> GetTextFromBitmap(System.Drawing.Bitmap bitmap)
        {
            Image<Gray, Byte> image = null;
            Image<Gray, Byte> imageOut = null;
            System.Drawing.Bitmap bitmapOut = null;
            var brightness = 0.0d;
            if (CaptureMode == CaptureMode.CLANMATES)
            {
                brightness = 0.5d;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
                bitmapOut = imageOut.AsBitmap();
            }
            else
            {
                brightness = CLANCHEST_IMAGE_BRIGHTNESS;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
                bitmapOut = imageOut.AsBitmap();
            }
            bitmapOut.Save("test-out-image-bw.jpg", ImageFormat.Jpeg);
            tessy.SetImage(imageOut);
            tessy.Recognize();
            
            var result = tessy.GetUTF8Text();
            result = result.Replace("\r\n", ",");
            string[] results = result.Split(',');
            List<String> ocrResults = new List<string>();
            foreach(var r in results.ToList())
            {
                if(!String.IsNullOrEmpty(r))
                {
                    ocrResults.Add(r);
                }
            }
            bitmapOut.Dispose();
            bitmapOut = null;

            imageOut.Dispose();
            imageOut = null;
            image.Dispose();
            image = null;
            results = null;

            return Task.FromResult(ocrResults.ToArray());
        }
        private async Task<OcrResult> GetTextFromBitmap(System.Drawing.Bitmap bitmap, Language language)
        {
            Image<Gray, Byte> image = null;
            Image<Gray, Byte> imageOut = null;
            System.Drawing.Bitmap bitmapOut = null;
            OcrResult result = null;
            var brightness = 0.0d;
            if (CaptureMode == CaptureMode.CLANMATES)
            {
                brightness = 0.5d;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
                bitmapOut = imageOut.AsBitmap();
            }
            else
            {
                brightness = CLANCHEST_IMAGE_BRIGHTNESS;
                image = bitmap.ToImage<Gray, Byte>();
                imageOut = image.Mul(brightness) + brightness;
                bitmapOut = imageOut.AsBitmap();
            }

            bitmapOut.Save("test-out-image-bw.jpg", ImageFormat.Jpeg);
            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {

                bitmapOut.Save(stream.AsStream(), ImageFormat.Bmp);
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream).AsTask(); //-- Can not find component exception when size is == 0.
                var bitmapSoftware = await decoder.GetSoftwareBitmapAsync();

                var engine = OcrEngine.TryCreateFromUserProfileLanguages();
                var ocrResult = await engine.RecognizeAsync(bitmapSoftware);
                result = ocrResult;

                //--- clean up
                ocrResult = null;
                engine = null;
                bitmapSoftware.Dispose();
                bitmapSoftware = null;
                decoder = null;
                stream.Dispose();
            }

            bitmapOut.Dispose();
            bitmapOut = null;
            imageOut.Dispose();
            imageOut = null;
            image.Dispose();
            image = null;
            return result;
        }
        private void CaptureRegion()
        {
            //-- now everything is completely manual when capturing. CLI C++ doesn't do any while loop.
            int sh = Snapture.ScreenHeight;
            int sw = Snapture.ScreenWidth;
            int x = sw / 2; //-- 1920 
            int y = sh / 2; //-- 1080

            double topChestPerc = 0.69d; //--
            double LeftChestPerc = 0.818d; //-- 1364 0.71d
            double widthChestPerc = 0.45d; //-- 0.76d
            

            int left = (int)Math.Round(x * LeftChestPerc);
            int top = (int)Math.Round(y * topChestPerc);
            int width = (int)Math.Round(x * widthChestPerc);
            int height = (int)Math.Round(y * 0.6975);

            CaptureMode = CaptureMode.CHESTS;
            Snapture.CaptureRegion(left, top, width, height);
            GlobalDeclarations.canCaptureAgain = false;

        }

        public void CaptureClanmates()
        {
            var language = new Language("en");
            if (!OcrEngine.IsLanguageSupported(language))
            {
                throw new Exception($"{language.ToString()} is not supported.");
            }

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
        private void ProcessClanmates(OcrResult result)
        {
            var tmpclanmates = new List<Clanmate>();
            foreach(var word in result.Lines)
            {
                Debug.WriteLine(word.Text);

            }

            /*
            for(var x = 0; x < result.Lines.Count; x+=2)
            {
                var word = result.Lines[x].Text;
                var clanmatename = result.Lines[x + 1].Text;
                tmpclanmates.Add(new Clanmate(clanmatename));

            }
            foreach(var clanmatename in tmpclanmates)
            {
                Debug.WriteLine(clanmatename.Name);
            }
            */
        }
        #endregion

        #region Automation Functions
        void StartAutomationThread()
        {
            stopAutomation = false;
            GlobalDeclarations.AuotmationRunning = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Task automationTask = Task.Run(() => StartAutomationProcess());

            }));
        }
        void StopAutomation()
        {
            stopAutomation = true;
            GlobalDeclarations.AuotmationRunning = false;

            ClanChestManager.SaveDataTask();
            ClanChestManager.CreateBackup();

            Debug.WriteLine("Automation Stopped.");
        }

        bool isAutomationBusy = false;
        
        int captureCounter = 0;

        void StartAutomationProcess()
        {
            //--- take snapshot.
            //--- mouse click 4 times on claim button
            //--- take snapshot 
            //--- rinse and repeat UNTIL F8 key is pressed by user to end it.
            var screenX_half = Snapture.ScreenWidth / 2;
            var screenY_half = Snapture.ScreenHeight / 2;
            var xClickPos = new Point(screenX_half * 0.711, screenY_half * 0.826);
            var x = (int)xClickPos.X * 2;
            var y = (int)xClickPos.Y;
            Debug.WriteLine("Automation Started.");
            GlobalDeclarations.canCaptureAgain = true;

            int automatorClicks = 1;
            while (!stopAutomation)
            {
                if (GlobalDeclarations.canCaptureAgain)
                {
                    Thread.Sleep(3000); //-- could prevent the "From:" bug.
                    CaptureRegion();
                    GlobalDeclarations.canCaptureAgain = false;
                    captureCounter++;
                    Debug.WriteLine($"Captured {captureCounter} Times.");
                    
                }

                if (GlobalDeclarations.isAnyGiftsAvailable == false)
                    continue;

                while (true)
                {
                    Automator.LeftClick(x, y);
                    if (automatorClicks == 4)
                        break;
                    else
                        automatorClicks++;

                    Thread.Sleep(50);
                }
                
                //-- canCaptureAgain not being switched back on.
                GlobalDeclarations.canCaptureAgain = true;
                automatorClicks = 1;
            }

        }
        #endregion

        #region InputHooks Events 
        private void InputHooks_onKeyReleased(object sender, InputHookEventArguments e)
        {
            if (GlobalDeclarations.hasHotkeyBeenPressed)
                GlobalDeclarations.hasHotkeyBeenPressed = false;
        }

        private void InputHooks_onKeyPressed(object sender, InputHookEventArguments e)
        {
            if (!GlobalDeclarations.hasHotkeyBeenPressed && !GlobalDeclarations.isBusyProcessingClanchests)
            {
                int key = e.KeyCode;
                int hotkey = KeyInterop.VirtualKeyFromKey(Key.F6);
                if (key == hotkey)
                {
                    Debug.WriteLine($"Snapshot taken.");
                    CaptureRegion();
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                }
                else if(key == KeyInterop.VirtualKeyFromKey(Key.F9))
                {
                    StartAutomationThread();
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                }
                else if(key == KeyInterop.VirtualKeyFromKey(Key.F10))
                {
                    StopAutomation();
                    GlobalDeclarations.hasHotkeyBeenPressed = true;
                }
            }
        }
        void DetectHotkey()
        {
            while (!isClosing)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    InputHooks.InputMessages();
                }));
            }
        }
        #endregion

        #region Snapture onFrameCaptured Event
        private async void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            var ocrResult = await GetTextFromBitmap(e.ScreenCapturedBitmap); //LoadBitmap(e.ScreenCapturedBitmap, new Windows.Globalization.Language("en"));

            if (ocrResult == null)
                throw new Exception("OCR text shouldn't be null or there's a problem.");

            //-- here we process data.
            e.ScreenCapturedBitmap.Dispose();
            
            if (CaptureMode == CaptureMode.CHESTS)
                ClanChestManager.ProcessChestData(ocrResult);
            
        }
        #endregion

        #region Tesseract Loading Languages Functions
        private Task LoadSpecficTesseractLanguagesAsync(string combined_languages) => Task.Run(() => LoadSpecficTesseractLanguages(combined_languages));    
        private void LoadSpecficTesseractLanguages(string combined_languages)
        {
            tessy = new Tesseract(GlobalDeclarations.TesseractData, combined_languages, OcrEngineMode.Default);
        }
        private Task LoadAllTesseractLanguagesAsync() => Task.Run(() => LoadAllTesseractLanguages());   
        private void LoadAllTesseractLanguages()
        {

            var tessdatafiles = System.IO.Directory.GetFiles(GlobalDeclarations.TesseractData, "*.traineddata");
            StringBuilder languages = new StringBuilder();
            for (var x = 0; x < tessdatafiles.Length - 1; x++)
            {
                var file = tessdatafiles[x];
                file = file.Substring(file.LastIndexOf(@"\") + 1);
                var language = file.Substring(0, file.LastIndexOf('.'));
                if (x != tessdatafiles.Length - 1)
                    languages.Append($"{language}+");
                else
                    languages.Append($"{language}");
            }

            //--- startup time takes longer when loading all languages inside tesseract.
            //--- future, have selectable languages and option to have all languages loaded. 
            // - "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus"
            tessy = new Tesseract(GlobalDeclarations.TesseractData, languages.ToString(), OcrEngineMode.Default);
            languages = null;
            tessdatafiles = null;
            Debug.WriteLine($"All Tesseract Trained data has been successfully loaded.");
        }
        #endregion

        #region Main Window Functions
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputHooks.onKeyPressed += InputHooks_onKeyPressed;
            InputHooks.onKeyReleased += InputHooks_onKeyReleased;
            if (InputHooks.Install())
            {
                Debug.WriteLine("Installed Keyboard hooks successfully.");
            }

            InputHookThread = new System.Threading.Thread(new System.Threading.ThreadStart(DetectHotkey));
            InputHookThread.Start();

            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
            Snapture.Start(FrameCapturingMethod.GDI);
            AppTitle = $"TotalBattle Chest Tracker v0.1.0 - Untitled";
            ClanChestManager = new ClanChestManager();
         
            if(File.Exists("recent.lst"))
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
                await LoadSpecficTesseractLanguagesAsync("eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus");
            }
            else
            {
                //-- tessdata folder needs to exist. And if not, should be prevented from even attempting to do any OCR.
                MessageBox.Show($"No Tessdata directory exists. Download tessdata and ensure all traineddata is inside tessdata.");
                GlobalDeclarations.TessDataExists = false;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            if (InputHooks.Uninstall())
            {
                Debug.WriteLine("Successfully uninstalled input hooks.");
            }
            ClanChestManager.ClearData();
            ClanChestSettings.Clear();
            if(tessy != null) 
                tessy.Dispose();

        }

        private void LoadClanDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute= true; 
        }
        private void LoadClanDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Clan Databases | *.cdb";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = ClanDatabaseManager.ClanDatabase.DefaultClanFolderPath;
           
            if (ClanChestSettings.ChestRequirements != null)
                ClanChestSettings.Clear();

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;
                ClanDatabaseManager.Load(file, ClanChestManager, result =>
                {
                    if (result)
                    {
                        AppTitle = $"TotalBattle Chest Tracker v0.1.0 - {ClanDatabaseManager.ClanDatabase.Clanname}";
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
                                foreach(var f in recently_opened_files)
                                {
                                    tw.WriteLine(f);
                                }
                            }
                        }

                       
                    }
                    else
                    {
                        MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                    }
                });
            }
        }

        private void Mu_Click(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            var tag = menuitem.Tag.ToString();
            if (tag != "CLEAR_HISTORY")
            {
                var file = menuitem.Tag.ToString();
                ClanDatabaseManager.Load(file, ClanChestManager, result =>
                {
                    if (result)
                    {
                        AppTitle = $"TotalBattle Chest Tracker v0.1.0 - {ClanDatabaseManager.ClanDatabase.Clanname}";
                    }
                    else
                    {
                        MessageBox.Show("Something went horribly wrong. Database is not suppost to be blank.");
                    }
                });
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
        private void SaveClanDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasNewClanDatabaseCreated)
                e.CanExecute = true;
            else 
                e.CanExecute = false;
        }

        private void SaveClanDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClanDatabaseManager.Save();
        }

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

        private void ExportClanDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasNewClanDatabaseCreated && GlobalDeclarations.hasClanmatesBeenAdded)
                e.CanExecute = true;
            else 
                e.CanExecute = false;
        }

        private void ExportClanDatabase_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //List<ClanChestData> clanChestData = ClanChestManager.clanChestData;
            ExportWindow exportWindow = new ExportWindow();
            exportWindow.mainwindow = this;
            exportWindow.ShowDialog();
        }

        private void QuitApplication_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QuitApplication_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void ManageClanmatesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalDeclarations.hasNewClanDatabaseCreated;
        }

        private void ManageClanmatesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddClanmatesWindow addClanmatesWindow = new AddClanmatesWindow();
            addClanmatesWindow.MainWindow = this;
            if (addClanmatesWindow.ShowDialog() == true)
            {
                GlobalDeclarations.hasClanmatesBeenAdded = true;
            }
        }

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

        private void ManuallyCaptureScreenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (GlobalDeclarations.hasClanmatesBeenAdded &&  GlobalDeclarations.hasNewClanDatabaseCreated && GlobalDeclarations.TessDataExists);
        }

        private void ManuallyCaptureScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!GlobalDeclarations.isBusyProcessingClanchests && GlobalDeclarations.TessDataExists)
                CaptureRegion();
        }

        private void StartAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasClanmatesBeenAdded && GlobalDeclarations.hasNewClanDatabaseCreated && GlobalDeclarations.TessDataExists)
                e.CanExecute = true;
            else 
                e.CanExecute = false;

        }

        private void StartAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!GlobalDeclarations.AuotmationRunning && GlobalDeclarations.TessDataExists)
            {
                StartAutomationThread();
            }
        }

        private void StopAutomationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalDeclarations.hasClanmatesBeenAdded && GlobalDeclarations.hasNewClanDatabaseCreated && GlobalDeclarations.TessDataExists)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void StopAutomationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(GlobalDeclarations.AuotmationRunning)
                StopAutomation();
        }
        #endregion

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
            clanStatisticsWindow.ChestManager = ClanChestManager;
            clanStatisticsWindow.Show();    
        }

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

            }
        }
    }
}
