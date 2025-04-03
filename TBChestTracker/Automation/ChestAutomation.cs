using System;
using System.Threading.Tasks;
using Emgu.CV;
using com.HellStormGames.Imaging.ScreenCapture;
using System.Windows;
using TBChestTracker.Engine;
using TBChestTracker.Managers;
using Emgu.CV.Structure;
using TBChestTracker.Effects;
using System.Diagnostics;
using com.HellStormGames.Diagnostics;
using com.HellStormGames.Imaging;
using com.HellStormGames.Imaging.Extensions;
using System.IO;
using System.Threading;


namespace TBChestTracker.Automation
{
    public class ChestAutomation
    {
        public OCREngine OCREngine { get; private set; }
        private ClanChestProcessResult clanChestProcessResult { get; set; }
        
        public bool isInitialized { get; private set; }
        public bool isRunning { get; private set; }
        public bool isCancelled { get; private set; }
        public bool isCancelling { get; private set; }
        public bool isCompleted { get; private set; }
        public bool canCaptureAgain { get; private set; }
        public bool isStoppingAutomation { get; private set; }
        private int clicksTotal = 0;
        public int OcrTextLinesRead {  get; private set; }
        private object ClicksLock = new object();
        public event EventHandler<AutomationEventArguments> AutomationStarted = null;
        public event EventHandler<AutomationEventArguments> AutomationStopped = null;
        public event EventHandler<AutomationErrorEventArguments> AutomationError = null;
        private event EventHandler<AutomationTextProcessedEventArguments> TextProcessed = null;
        private event EventHandler<AutomationChestProcessedEventArguments> ChestProcessed = null;
        private event EventHandler<AutomationClicksEventArguments> ProcessingClicks = null;
        private event EventHandler<AutomationClicksEventArguments> ProcessedClicks = null;  
        private event EventHandler<AutomationChestProcessingFailedEventArguments> ChestProcessingFailed = null;
        private CancellationTokenSource CancellationSource {  get; set; }

        public static ChestAutomation Instance { get; private set; }
        private void Cancel()
        {
            try
            {
                if (CancellationSource != null)
                {
                    lock (this)
                    {
                        CancellationSource.Cancel();
                        isCancelling = true;
                        isStoppingAutomation = true;
                        onAutomationStopped(new AutomationEventArguments(false, true));
                    }
                }
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Chest Automation", "Couldn't Cancel Automation.");
            }
        }
        protected virtual void onAutomationStarted(AutomationEventArguments args)
        {
            EventHandler<AutomationEventArguments> handler = AutomationStarted;
            if (handler != null)
            {
                isStoppingAutomation = false;
                isCancelled = false;
                isCancelling = false;
                isCompleted = false;
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
                isCancelling = false;
                isCompleted = true;
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
            isCancelling = false;
            isCancelled = false;
            isStoppingAutomation = false;
            Instance = this;
        }

        public bool Initialize(OCRSettings ocrSettings)
        {
            OCREngine = new OCREngine();
            Loggio.Info("Checking to see if Tesseract Data Folder exists.");

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
            isInitialized = true;

            TextProcessed += ChestAutomation_TextProcessed;
            ChestProcessed += ChestAutomation_ChestProcessed;
            ChestProcessingFailed += ChestAutomation_ChestProcessingFailed;
            ProcessingClicks += ChestAutomation_ProcessingClicks;
            ProcessedClicks += ChestAutomation_ProcessedClicks;
            return true;
        }

        public bool Reinitialize(OCRSettings ocrSettings)
        {
            OCREngine.Destroy();
            isInitialized = false;
            TextProcessed -= ChestAutomation_TextProcessed;
            ChestProcessed -= ChestAutomation_ChestProcessed;
            ChestProcessingFailed -= ChestAutomation_ChestProcessingFailed;
            ProcessingClicks -= ChestAutomation_ProcessingClicks;
            ProcessedClicks -= ChestAutomation_ProcessedClicks;
            return Initialize(ocrSettings);

        }
        private void ChestAutomation_ProcessedClicks(object sender, AutomationClicksEventArguments e)
        {
            if (isCancelling == false || isCancelling == false)
            {
                if (e.ProcessedClicks == true)
                {
                    this.canCaptureAgain = true;
                }
            }
        }

        private void ChestAutomation_ProcessingClicks(object sender, AutomationClicksEventArguments e)
        {
            lock (ClicksLock)
            {
                if (isCancelling == false || isCancelled == false)
                {
                    if (e.CurrentClick >= e.MaxClicks)
                    {
                        clicksTotal++;

                        AutomationClicksEventArguments arg = new AutomationClicksEventArguments(true, e.CurrentClick, e.MaxClicks);
                        onProcessedClicks(arg);
                    }
                }
            }
        }

        private async void ChestAutomation_ChestProcessed(object sender, AutomationChestProcessedEventArguments e)
        {
            if (isCancelled == false || isCancelling == false)
            {
                var result = e.ProcessResult;

                if (result.Result == ClanChestProcessEnum.NO_GIFTS)
                {
                    StopAutomation();
                    Loggio.Info("There are no more chests to collect.");
                }

                if (result.Result == ClanChestProcessEnum.SUCCESS)
                {
                    int automationClicks = 0;
                    int maxClicks = SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks;
                    var claim_button = ClanManager.Instance.GetCurrentOcrProfile().ClickTarget;
                    claim_button.X += 32 / 2; //-- (ImageSize / 2). Referencing ClickMarkerIcon.
                    claim_button.Y += 32 / 2;

                    while (automationClicks != maxClicks)
                    {
                        if (isCancelled || isCancelling)
                        {
                            Loggio.Warn("Chest Automation - Chest Processed", "Automation task is being or is cancelled...");
                            break;
                        }

                        var monitorbounds = Snapster.MonitorConfiguration.Monitor.ScreenBounds;

                        Automator.LeftClick(monitorbounds.X + (int)claim_button.X, monitorbounds.Y + (int)claim_button.Y);
                        automationClicks++;
                        await Task.Delay(SettingsManager.Instance.Settings.AutomationSettings.AutomationDelayBetweenClicks);

                        onProcessingClicks(new AutomationClicksEventArguments(false, automationClicks, maxClicks));
                    }
                }
            }
        }

        private void ChestAutomation_TextProcessed(object sender, AutomationTextProcessedEventArguments e)
        {
            if (isCancelled || isCancelling)
            {
                Loggio.Warn("Chest Automation - Text Process", "Automation task is being or is cancelled...");
                return;
            }

            var ocrResult = e.TessResult;
            if(ocrResult != null)
            {
                ClanManager.Instance.ClanChestManager.ProcessChestsAsRaw(ocrResult.Words, this);
            }
        }

        private void ChestAutomation_ChestProcessingFailed(object sender, AutomationChestProcessingFailedEventArguments e)
        {
            if (isCancelled || isCancelling)
            {
                Loggio.Warn("Chest Automation - Chest Processing Failed ", "Automation task is being or is cancelled...");
                return;
            }

            var errormessage = e.ErrorMessage;
            if (!String.IsNullOrEmpty(errormessage))
            {
                canCaptureAgain = false;
                if (MessageBox.Show(errormessage, "Ocr Results had read issues", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    Loggio.Warn("Automation Stopped and saved chest data. Reason for issue was => " + errormessage);
                    StopAutomation();
                }
            }
        }

        #region New Process Capture Frame for Snapture
        private void ProcessCapturedFrame(ImageData image)
        {
            try
            {
                if (isCancelled || isCancelling)
                {
                    Loggio.Warn("Chest Automation - Process Captured Frame", "Automation task is being or is cancelled...");
                    return;
                }

                if (isRunning == true && isStoppingAutomation == false)
                {

                    var bitmap = image.ToBitmap();
                    var ocrSettings = SettingsManager.Instance.Settings.OCRSettings;

                    var brightness = ocrSettings.GlobalBrightness;
                    var threshold = new Gray(ocrSettings.Threshold); //-- 85
                    var maxThreshold = new Gray(ocrSettings.MaxThreshold); //--- 255

                    bool bSave = false;
                    string outputPath = String.Empty;

                    Mat outputImage = new Mat();
                    Mat original_image = bitmap.ToImage<Bgr, Byte>().Mat;
                    string prefix = "Original";
                    string screencaptureImagePath = String.Empty;
                    string screencaptureDir = String.Empty;
                    string timeStr = String.Empty;
                    if (ocrSettings.SaveScreenCaptures)
                    {
                        var dateobj = DateTime.Now;
                        timeStr = $"{dateobj.ToString("HH-mm-ss-ffff")}";
                        var dateStr = $"{dateobj.ToString("yyyy-MM-dd")}";
                        screencaptureDir = $"{ClanManager.Instance.CurrentProjectDirectory}\\ScreenCaptures\\{dateStr}";
                        var d = new DirectoryInfo(screencaptureDir);
                        if (d.Exists == false)
                        {
                            d.Create();
                        }
                        screencaptureImagePath = $"{screencaptureDir}\\{prefix}ScreenCapture_{timeStr}.png";
                        original_image.Save(screencaptureImagePath);
                    }

                    outputImage = ImageEffects.ConvertToGrayscale(original_image, bSave, outputPath);
                    outputImage = ImageEffects.Brighten(outputImage, brightness, bSave, outputPath);
                    outputImage = ImageEffects.Resize(outputImage, 3, Emgu.CV.CvEnum.Inter.Cubic, bSave, outputPath);
                    outputImage = ImageEffects.ThresholdBinaryInv(outputImage, threshold, maxThreshold, bSave, outputPath);
                    outputImage = 255 - outputImage;
                    if (ocrSettings.SaveScreenCaptures)
                    {
                        prefix = "Filtered";
                        screencaptureImagePath = $"{screencaptureDir}\\{prefix}ScreenCapture_{timeStr}.png";
                        outputImage.Save(screencaptureImagePath);
                    }

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
                            var outImage = outputImage.ToBitmap();
                            if (outImage != null)
                            {
                                var ocrResult = OCREngine.Read(outImage);

                                if (ocrResult != null)
                                {
                                    AutomationTextProcessedEventArguments arg = new AutomationTextProcessedEventArguments(ocrResult);

                                    outputImage?.Dispose();
                                    outputImage = null;
                                    original_image?.Dispose();
                                    original_image = null;
                                    bitmap?.Dispose();
                                    bitmap = null;
                                    image.Dispose();
                                    onTextProcessed(arg);

                                    
                                    return;
                                }
                                else
                                {
                                    //-- there's an issue regarding Tesseract.
                                    //-- need to cancel.

                                    outputImage?.Dispose();
                                    outputImage = null;
                                    original_image?.Dispose();
                                    original_image = null;
                                    bitmap?.Dispose();
                                    bitmap = null;
                                    image.Dispose();

                                    throw new Exception("Issue with Tessy. Returned NULL. Possible empty page.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Loggio.Error(ex, "Chest Automation", "An issue occurred while running chest automation.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Chest Automation", "An issue occurred while running chest automation.");
                //-- it should pull ripcord.

                return;
            }
        }
        #endregion
        private void CaptureRegion(CancellationToken token)
        {
            //var capture_region = SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest;
            try
            {
                if(token.IsCancellationRequested)
                {
                     token.ThrowIfCancellationRequested();  
                }

                if (isCancelling == false || isCancelled == false)
                {
                    var capture_region = ClanManager.Instance.GetCurrentOcrProfile();
                    int ca_x = (int)capture_region.x;
                    int ca_y = (int)capture_region.y;
                    int ca_width = (int)capture_region.width;
                    int ca_height = (int)capture_region.height;
                    com.HellStormGames.Imaging.Region region = new Region(ca_x, ca_y, ca_width, ca_height);
                    var image = Snapster.CaptureRegion(region);
                    ProcessCapturedFrame(image);
                    this.canCaptureAgain = false;
                }
            }
            catch(Exception ex)
            {

            }

        }

        private async Task StartAutomationTask(CancellationToken token)
        {
            if (isRunning == false)
            {
                isRunning = true;
                AppContext.Instance.AutomationRunning = true;
                await PerformAutomation(token);
            }
        }

        object AutomationLock = new object();

        private async Task PerformAutomation(CancellationToken token)
        {
            //-- load OcrCorrectionWords
            ClanManager.Instance.ClanChestManager.GetChestProcessor().LoadOcrCorrectionWords();
            var claim_button = ClanManager.Instance.GetCurrentOcrProfile().ClickTarget;
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
                        if(token.IsCancellationRequested)
                        {
                            break;
                        }
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
                            CaptureRegion(token);

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
            // isRunning = false;
        }

        public async Task StartAutomation()
        {
            Loggio.Info("Chest Automation is being started...");
            CancellationSource = new CancellationTokenSource();
            onAutomationStarted(new AutomationEventArguments(true, false));
            OcrTextLinesRead = 0;
            await Task.Run(() => StartAutomationTask(CancellationSource.Token));
        }

        public void StopAutomation()
        {
            if (isStoppingAutomation == false)
            {
                //-- to prevent from being called twice or more times.
                Cancel();
                
            }
        }
        public void UpdateOcrLinesRead(int lineCount)
        {
            OcrTextLinesRead += lineCount;
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

            Loggio.Info("Shutting down Snapster");

            Snapster.Release();

            if (OCREngine != null)
            {
                Loggio.Info("Destroying OCR Engine.");
                OCREngine.Destroy();
            }
        }
    }
}
