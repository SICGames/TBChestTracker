using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using com.HellstormGames.ScreenCapture;
using com.HellstormGames.Imaging.Extensions;
using com.HellStormGames.Logging;
using System.Windows;
using TBChestTracker.Engine;
using TBChestTracker.Managers;
using Emgu.CV.Structure;
using TBChestTracker.Effects;
using TBChestTracker.Helpers;
using System.Diagnostics;
using System.Threading;

namespace TBChestTracker.Automation
{
    public class ChestAutomation
    {
        public Snapture Snapture { get; private set; }
        private ClanChestProcessResult clanChestProcessResult { get; set; }
        private CancellationTokenSource CancellationToken { get; set; }

        public bool isInitialized { get; private set; }
        public bool isRunning { get; private set; }
        public bool isCancelled { get; private set; }

        public bool isCompleted { get; private set; }
        public bool isFaulted { get; private set; }


        public event EventHandler<AutomationEventArguments> AutomationStarted = null;
        public event EventHandler<AutomationEventArguments> AutomationStopped = null;
        public event EventHandler<AutomationErrorEventArguments> AutomationError = null;

        protected virtual void onAutomationStarted(AutomationEventArguments args)
        {
            EventHandler<AutomationEventArguments> handler = AutomationStarted;
            if (handler != null)
            {
                handler(this, args);
            }
        }
        protected virtual void onAutomationStopped(AutomationEventArguments args) {
            EventHandler<AutomationEventArguments> handler = AutomationStopped;
            if(handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void onAutomationError(AutomationErrorEventArguments args) 
        {
            EventHandler<AutomationErrorEventArguments> handler = AutomationError;
            if(handler != null)
            {
                handler (this, args);   
            }
        }

        public ChestAutomation() 
        {
            isCompleted = false;
            isRunning = false;
            isCompleted = true;
            isFaulted = false;
        }

        public bool Initialize(OCRSettings ocrSettings)
        {
            bool result = false;
            if (System.IO.Directory.Exists(ocrSettings.TessDataFolder))
            {
                AppContext.Instance.TessDataExists = true;
                var languages = ocrSettings.Languages;
                OCREngine.Init(ocrSettings);
            }
            else
            {
                //-- tessdata folder needs to exist. And if not, should be prevented from even attempting to do any OCR.
                MessageBox.Show($"No Tessdata directory exists. Download tessdata and ensure all traineddata is inside tessdata.");
                AppContext.Instance.TessDataExists = false;
            }

            Snapture = new Snapture();
            Snapture.isDPIAware = true;
            Snapture.SetBitmapResolution((int)ocrSettings.Dpi);
            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
            Snapture.Start(FrameCapturingMethod.GDI);

            com.HellStormGames.Logging.Console.Write("Snapture Started.", com.HellStormGames.Logging.LogType.INFO);
            isInitialized = true;
            return true;
        }


        #region Snapture onFrameCaptured Event
        private async void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            var ocrResult = await GetTextFromBitmap(e.ScreenCapturedBitmap); //LoadBitmap(e.ScreenCapturedBitmap, new Windows.Globalization.Language("en"));
            if (ocrResult == null)
            {

            }

            //-- here we process data.
            e.ScreenCapturedBitmap.Dispose();

            if (ocrResult != null)
            {
                clanChestProcessResult = await ClanManager.Instance.ClanChestManager.ProcessChestData(ocrResult.Words, onError =>
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
                if (clanChestProcessResult.Result == ClanChestProcessEnum.NO_GIFTS)
                {
                    StopAutomation();
                    com.HellStormGames.Logging.Console.Write($"There are no more gifts to collect.", "Automation Result", com.HellStormGames.Logging.LogType.INFO);
                }
            }
        }
        private void CaptureRegion()
        {
            var capture_region = SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest;

            int ca_x = (int)capture_region.x;
            int ca_y = (int)capture_region.y;
            int ca_width = (int)capture_region.width;
            int ca_height = (int)capture_region.height;

            Snapture.CaptureRegion(ca_x, ca_y, ca_width, ca_height);

            AppContext.Instance.canCaptureAgain = false;
        }

        private Task<TessResult> GetTextFromBitmap(System.Drawing.Bitmap bitmap)
        {

            var ocrSettings = SettingsManager.Instance.Settings.OCRSettings;
            var brightness = ocrSettings.GlobalBrightness;
            var threshold = new Gray(ocrSettings.Threshold); //-- 85
            var maxThreshold = new Gray(ocrSettings.MaxThreshold); //--- 255

            bool bSave = false;
            string outputPath = String.Empty;

            Mat outputImage = new Mat();
            Mat original_image = bitmap.ToImage<Bgr, Byte>().Mat;

            if (AppContext.Instance.SaveOCRImages)
            {
                bSave = true;
                outputPath = $@"{AppContext.Instance.AppFolder}\Output";
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(outputPath);
                if (di.Exists == false)
                {
                    di.Create();
                }
                original_image.Save($@"{outputPath}\OCR_ImageOriginal.png");
            }

            outputImage = ImageEffects.ConvertToGrayscale(original_image, bSave, outputPath);
            outputImage = ImageEffects.Brighten(outputImage, brightness, bSave, outputPath);

            //-- OCR Incorrect Text Bug - e.g. Slash Jr III is read Slash )r III
            //-- Fix: Upscaling input image large enough to read properly.
            outputImage = ImageEffects.Resize(outputImage, 2, Emgu.CV.CvEnum.Inter.Cubic, bSave, outputPath);
            outputImage = ImageEffects.ThresholdBinaryInv(outputImage, threshold, maxThreshold, bSave, outputPath);
            //-- if it is null or empty somehow, we update it.
            if (String.IsNullOrEmpty(ocrSettings.PreviewImage))
            {
                outputImage.Save($@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png");
                ocrSettings.PreviewImage = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png";
            }

            var ocrResult = OCREngine.Read(outputImage);

            outputImage.Dispose();
            original_image.Dispose();

            return Task.FromResult(ocrResult);
        }

        /*
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
                    Thread.Sleep(SettingsManager.Instance.Settings.AutomationSettings.AutomationScreenshotsAfterClicks); //-- could prevent the "From:" bug.

                    CaptureRegion();

                    while (ClanManager.Instance.ClanChestManager.ChestProcessingState != ChestProcessingState.COMPLETED)
                    {
                    }
                }

                if (clanChestProcessResult.Result == ClanChestProcessEnum.SUCCESS)
                {
                    while (automatorClicks != SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks)
                    {
                        Automator.LeftClick(claim_button.X, claim_button.Y);
                        automatorClicks++;
                        Thread.Sleep(SettingsManager.Instance.Settings.AutomationSettings.AutomationDelayBetweenClicks);
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
        */

        #endregion

        private void StartAutomationTask(CancellationToken token)
        {
            isRunning = true;
            AppContext.Instance.AutomationRunning = true;
            Task.Run(() => PerformAutomation(token));
        }

        private async Task PerformAutomation(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var claim_button = SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons[0];
            AppContext.Instance.canCaptureAgain = true;

            while (isRunning)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    int automatorClicks = 0;
                    if (AppContext.Instance.canCaptureAgain)
                    {
                        Thread.Sleep(SettingsManager.Instance.Settings.AutomationSettings.AutomationScreenshotsAfterClicks); //-- could prevent the "From:" bug.

                        CaptureRegion();

                        while (ClanManager.Instance.ClanChestManager.ChestProcessingState != ChestProcessingState.COMPLETED)
                        {
                        }
                    }

                    if (clanChestProcessResult.Result == ClanChestProcessEnum.SUCCESS)
                    {
                        while (automatorClicks != SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks)
                        {
                            Automator.LeftClick(claim_button.X, claim_button.Y);
                            automatorClicks++;
                            Thread.Sleep(SettingsManager.Instance.Settings.AutomationSettings.AutomationDelayBetweenClicks);
                        }

                        //-- canCaptureAgain not being switched back on.
                        AppContext.Instance.canCaptureAgain = true;
                    }
                }
                catch (OperationCanceledException o_ex)
                {

                }
                catch (Exception e)
                {
                    //-- real exception.
                }
            }

            StopAutomation();
        }
        public void StartAutomation()
        {
            onAutomationStarted(new AutomationEventArguments(true, false));
            CancellationToken = new CancellationTokenSource();
            StartAutomationTask(CancellationToken.Token);
        }

        public void StopAutomation()
        {
            if(CancellationToken != null)
            {
                CancellationToken.Cancel();
                isRunning = false;
                isCancelled = true;
                isFaulted = false;

                onAutomationStopped(new AutomationEventArguments(false, true));
            }
        }

        public void Release()
        {
            if(isRunning)
            {
                StopAutomation();
            }
            if(CancellationToken != null)
            {
                CancellationToken.Dispose();
            }
            if (clanChestProcessResult != null)
            {
                clanChestProcessResult = null;
            }
            if(Snapture != null)
            {
                Snapture.Dispose();
            }
        }
    }
}
