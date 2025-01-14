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
using Newtonsoft.Json.Linq;
using System.Drawing;
using CefSharp.DevTools.Database;
using com.HellStormGames.Diagnostics;


namespace TBChestTracker.Automation
{
    public class ChestAutomation
    {
        public Snapture Snapture { get; private set; }
        public OCREngine OCREngine { get; private set; }
        private ClanChestProcessResult clanChestProcessResult { get; set; }
        
        public bool isInitialized { get; private set; }
        public bool isRunning { get; private set; }
        public bool isCancelled { get; private set; }
        public bool isCompleted { get; private set; }
        
        public bool canCaptureAgain { get; private set; }
        public bool isStoppingAutomation { get; private set; }

        private int clicksTotal = 0;

        public event EventHandler<AutomationEventArguments> AutomationStarted = null;
        public event EventHandler<AutomationEventArguments> AutomationStopped = null;
        public event EventHandler<AutomationErrorEventArguments> AutomationError = null;

        private event EventHandler<AutomationTextProcessedEventArguments> TextProcessed = null;
        private event EventHandler<AutomationChestProcessedEventArguments> ChestProcessed = null;

        private event EventHandler<AutomationClicksEventArguments> ProcessingClicks = null;
        private event EventHandler<AutomationClicksEventArguments> ProcessedClicks = null;  

        private event EventHandler<AutomationChestProcessingFailedEventArguments> ChestProcessingFailed = null;
        
        protected virtual void onAutomationStarted(AutomationEventArguments args)
        {
            EventHandler<AutomationEventArguments> handler = AutomationStarted;
            if (handler != null)
            {
                isStoppingAutomation = false;
                handler(this, args);
            }
        }
        protected virtual void onAutomationStopped(AutomationEventArguments args) {
            EventHandler<AutomationEventArguments> handler = AutomationStopped;
            if(handler != null)
            {
                clicksTotal = 0;
                isRunning = false;
                isCancelled = true;
                AppContext.Instance.AutomationRunning = false;
                Loggio.Info("Chest Automation is stopped...");
                handler(this, args);
            }
        }

        protected virtual void onAutomationError(AutomationErrorEventArguments args) 
        {
            EventHandler<AutomationErrorEventArguments> handler = AutomationError;
            if(handler != null)
            {
                clicksTotal = 0;
                handler (this, args);   
            }
        }

        protected virtual void onTextProcessed(AutomationTextProcessedEventArguments args)
        {
            EventHandler<AutomationTextProcessedEventArguments> handler = TextProcessed;
            if(handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void onChestProcessed(AutomationChestProcessedEventArguments args)
        {
            EventHandler<AutomationChestProcessedEventArguments> handler = ChestProcessed;
            if( handler != null)
            {
                handler(this, args);
            }
        }
        protected virtual void onProcessingClicks(AutomationClicksEventArguments args)
        {
            EventHandler<AutomationClicksEventArguments> handler = ProcessingClicks;
            if(handler != null)
            {
                handler(this, args);
            }
        }
        protected virtual void onProcessedClicks(AutomationClicksEventArguments args)
        {
            EventHandler<AutomationClicksEventArguments> handler = ProcessedClicks;
            if(handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void onChestProcessingFailed(AutomationChestProcessingFailedEventArguments args)
        {
            EventHandler<AutomationChestProcessingFailedEventArguments> handler = ChestProcessingFailed;
            if(handler!= null)
            {
                handler (this, args);
            }
        }
        public void InvokeChestProcessingFailed(AutomationChestProcessingFailedEventArguments args)
        {
            onChestProcessingFailed (args); 
        }
        public void InvokeChestProcessed(AutomationChestProcessedEventArguments args)
        {
            onChestProcessed (args);
        }
     
        public ChestAutomation() 
        {
            isCompleted = false;
            isRunning = false;
            isCompleted = true;
            isStoppingAutomation = false;
        }

        public bool Initialize(OCRSettings ocrSettings)
        {
            OCREngine = new OCREngine();
            
            if (System.IO.Directory.Exists(ocrSettings.TessDataFolder))
            {
                AppContext.Instance.TessDataExists = true;
                var languages = ocrSettings.Languages;
                Loggio.Info("Initializing OCR Engine...");

                if(OCREngine.Init(ocrSettings) == false)
                {
                    if(MessageBox.Show("OCR Engine failed to be initialized. Check recent log file for more information.", 
                        "OCR Initialization.", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error, 
                        MessageBoxResult.OK, 
                        MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                    {
                        return false;
                    }
                }
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

            Loggio.Info("Snapture Started...");
            
            isInitialized = true;

            TextProcessed += ChestAutomation_TextProcessed;
            ChestProcessed += ChestAutomation_ChestProcessed;
            ChestProcessingFailed += ChestAutomation_ChestProcessingFailed;
            ProcessingClicks += ChestAutomation_ProcessingClicks;
            ProcessedClicks += ChestAutomation_ProcessedClicks;
            return true;
        }

        private void ChestAutomation_ProcessedClicks(object sender, AutomationClicksEventArguments e)
        {
            if(e.ProcessedClicks == true)
            {
                this.canCaptureAgain = true;
            }
        }

        private void ChestAutomation_ProcessingClicks(object sender, AutomationClicksEventArguments e)
        {

            if(e.CurrentClick >= e.MaxClicks)
            {
                clicksTotal++;

                AutomationClicksEventArguments arg = new AutomationClicksEventArguments(true, e.CurrentClick, e.MaxClicks);
                onProcessedClicks(arg);
            }
        }

        private async void ChestAutomation_ChestProcessed(object sender, AutomationChestProcessedEventArguments e)
        {
            var result = e.ProcessResult;

            if(result.Result == ClanChestProcessEnum.NO_GIFTS)
            {
                StopAutomation();
                Loggio.Info("There are no more chests to collect.");
            }

            if (result.Result == ClanChestProcessEnum.SUCCESS)
            {
                int automationClicks = 0;
                int maxClicks = SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks;
                var claim_button = SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons[0];

                while (automationClicks != maxClicks)
                {
                    Automator.LeftClick(claim_button.X, claim_button.Y);
                    automationClicks++;
                    await Task.Delay(SettingsManager.Instance.Settings.AutomationSettings.AutomationDelayBetweenClicks);
                    
                    onProcessingClicks(new AutomationClicksEventArguments(false, automationClicks, maxClicks));
                }
            }
        }

        private void ChestAutomation_TextProcessed(object sender, AutomationTextProcessedEventArguments e)
        {
            var ocrResult = e.TessResult;
            if(ocrResult != null)
            {
                ClanManager.Instance.ClanChestManager.ProcessChestsAsRaw(ocrResult.Words, this);
            }
        }

        private void ChestAutomation_ChestProcessingFailed(object sender, AutomationChestProcessingFailedEventArguments e)
        {
            var errormessage = e.ErrorMessage;
            if (!String.IsNullOrEmpty(errormessage))
            {
                canCaptureAgain = false;
                var result = MessageBox.Show($"Stopping automation and saving chest data. Reason: {errormessage}", "OCR Error", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    Loggio.Warn("Automation Stopped and saved chest data. Reason for issue was => " + errormessage);
                    StopAutomation();
                }
            }
        }
        #region Snapture onFrameCaptured Event
        private void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            try
            {
                if (isRunning && isStoppingAutomation == false)
                {
                    Debug.WriteLine($"Automation is running.");
                    var bitmap = e.ScreenCapturedBitmap;
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
                    outputImage = ImageEffects.Resize(outputImage, 3, Emgu.CV.CvEnum.Inter.Cubic, bSave, outputPath);
                    outputImage = ImageEffects.ThresholdBinaryInv(outputImage, threshold, maxThreshold, bSave, outputPath);
                    //-- if it is null or empty somehow, we update it.
                    if (String.IsNullOrEmpty(ocrSettings.PreviewImage))
                    {
                        // outputImage.Save($@"{AppContext.Instance.LocalApplicationPath}\preview_image.png");
                        ocrSettings.PreviewImage = $@"{AppContext.Instance.LocalApplicationPath}\preview_image.png";
                    }

                    try
                    {
                        if (bitmap != null)
                        {
                            Debug.WriteLine("Preparing Bitmap to be read by OCR");
                            var finalMat = outputImage.ToImage<Bgr, byte>();
                            var ocrResult = OCREngine.Read(finalMat.ToBitmap());

                            if (ocrResult != null)
                            {
                                AutomationTextProcessedEventArguments arg = new AutomationTextProcessedEventArguments(ocrResult);
                                
                                outputImage.Dispose();
                                outputImage = null;
                                original_image.Dispose();
                                original_image = null;
                                
                                bitmap.Dispose();
                                bitmap = null;
                                e.ScreenCapturedBitmap.Dispose();
                                e.ScreenCapturedBitmap = null;
                                onTextProcessed(arg);
                            }
                            else
                            {
                                //-- there's an issue regarding Tesseract.
                                //-- need to cancel.
                                throw new Exception("Issue with Tessy. Returned NULL. Possible empty page.");
                            }
                          
                        }
                    }
                    catch (Exception ex)
                    {
                        
                            if (ex is AccessViolationException)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                    }

                    Debug.WriteLine("Automation Is Stopping....");

                }
            }
            catch(Exception ex)
            {

            }

        }
        #endregion
        private void CaptureRegion()
        {
            var capture_region = SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest;

            int ca_x = (int)capture_region.x;
            int ca_y = (int)capture_region.y;
            int ca_width = (int)capture_region.width;
            int ca_height = (int)capture_region.height;

            Snapture.CaptureRegion(ca_x, ca_y, ca_width, ca_height);
            this.canCaptureAgain = false;
        }

        private async Task StartAutomationTask()
        {
            if (isRunning == false)
            {
                isRunning = true;
                AppContext.Instance.AutomationRunning = true;
                await PerformAutomation();
            }
        }

        private async Task PerformAutomation()
        {
            var claim_button = SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons[0];
            this.canCaptureAgain = true;
            try
            {
                // token.ThrowIfCancellationRequested();
                clicksTotal = 0;
                Loggio.Info("Chest Automation is beginning to run...");
                var automationSettings = SettingsManager.Instance.Settings.AutomationSettings;
                var maxClicksReached = false;
                while (isRunning)
                {
                    try
                    {
                        if (automationSettings.StopAutomationAfterClicks > 0)
                        {
                            if (clicksTotal >= automationSettings.StopAutomationAfterClicks)
                            {
                                maxClicksReached = true;
                                break;
                            }
                        }

                        if (this.canCaptureAgain)
                        {
                            await Task.Delay(SettingsManager.Instance.Settings.AutomationSettings.AutomationScreenshotsAfterClicks);
                            CaptureRegion();

                            //--- ChestProcessingState causes an null object reference exception. 
                            var cp = ClanManager.Instance.ClanChestManager.GetChestProcessor();
                            if (cp != null)
                            {
                                while (cp.ChestProcessingState != ChestProcessingState.COMPLETED)
                                {
                                }
                            }
                        }
                    }
                    catch(OperationCanceledException e)
                    {
                        Loggio.Info("Chest Automation is being cancelled...");
                    }
                }
                if(maxClicksReached)
                {
                    StopAutomation();
                }
            }
            catch (Exception e)
            {

            }
            //-- isRnnning = false;

        }
        public async Task StartAutomation()
        {
            Loggio.Info("Chest Automation is being started...");
            onAutomationStarted(new AutomationEventArguments(true, false));
            await Task.Run(() => StartAutomationTask());
        }

        public void StopAutomation()
        {
            if (isStoppingAutomation == false)
            {
                //-- to prevent from being called twice or more times.
                isStoppingAutomation = true;
                onAutomationStopped(new AutomationEventArguments(false, true));
            }
        }

        public void Release()
        {

            if(isRunning)
            {
                StopAutomation();
            }
            if (clanChestProcessResult != null)
            {
                clanChestProcessResult = null;
            }
            if(Snapture != null)
            {
                Snapture.Dispose();
            }
            
            if(OCREngine != null)
                OCREngine.Destroy();
        }
    }
}
