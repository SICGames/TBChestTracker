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

namespace TBChestTracker.Automation
{
    public class ChestAutomation
    {
        public Snapture Snapture { get; private set; }
        private ClanChestProcessResult clanChestProcessResult { get; set; }

        public ChestAutomation() 
        {
            
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
            Snapture.Start(FrameCapturingMethod.GDI);

            com.HellStormGames.Logging.Console.Write("Snapture Started.", com.HellStormGames.Logging.LogType.INFO);
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

        #endregion
        public async Task StartAutomation()
        {

        }
        public async Task StopAutomation()
        {

        }
    }
}
