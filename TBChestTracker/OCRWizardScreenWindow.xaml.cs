using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using com.HellstormGames.ScreenCapture;
using com.HellstormGames.Imaging.DirectX;
using com.HellstormGames.Imaging.Extensions;
using Emgu;
using Emgu.CV;
using System.Diagnostics;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using sys_drawing = System.Drawing;
using System.Threading;
using com.HellstormGames.Imaging;

using TBChestTracker.Helpers;
using Emgu.CV.OCR;
using System.ComponentModel;
using TBChestTracker.UI;
using System.Windows.Controls.Primitives;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OCRWizardScreenWindow.xaml
    /// </summary>
    public partial class OCRWizardScreenWindow : Window, INotifyPropertyChanged
    {

        #region Global Declarations
        private Snapture Snapture = null;
        BitmapSource PreviewImageSource { get; set; }
        List<System.Drawing.Rectangle> CornerRectangles = new List<System.Drawing.Rectangle>();
        Image<Bgr, Byte> ChestWindowChestsActiveRefImage { get; set; }
        Image<Bgr, byte> OpenChestRefImage { get; set; }
        private List<string> ref_image_files = null;
        private List<Image<Bgr, Byte>> ref_images = null;
        System.Drawing.Bitmap CapturedBitmap = null;
        private WriteableBitmap writeableBitmap = null;
        private Image WritableImage = null;
        private Dpi dpi = null;
        private AOIRect AreaOfInterest = null;
        private AOIRect SuggestedAreaOfInterest = null;
        private List<sys_drawing.Point> OpenChestButtonPositions;
        private double Thickness = 5;
        private bool canExit = true;
        bool isPlayerOnChestsTab = false;
        bool canRender = false;
        private CancellationTokenSource cancellationTokenSource = null;
        private Tesseract.Character[] Letters {  get; set; }

        private int max_retry_attempts = 5;
        private int retry_attempts = 0;

        private double min_treshold_gifts_tab = 0.0;
        private double max_treshold_gifts_tab = 0.9;

        private String pStatusMessage = "Processing...";

        public OCRWizardWindow ocrWindow { get; set; }

        public String StatusMessage
        {
            get
            {
                return pStatusMessage;
            }
            set
            {
                pStatusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private bool isCompleted = false;

        #endregion

        #region Events
        private event EventHandler onOCRWizardCancelled;
        protected virtual void OCRWizardCanceled(EventArgs e)
        {
            onOCRWizardCancelled?.Invoke(this, e);
        }

        public event EventHandler onOCRWizardCompleted;
        protected virtual void OCRWizardCompleted(EventArgs e)
        {
            onOCRWizardCompleted?.Invoke(this, e);
        }

        public event EventHandler<OCRWizardEventArgs> onOCRWizardFailed;
        protected virtual void OCRWizardFailed(OCRWizardEventArgs e)
        {
            onOCRWizardFailed?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

      

        #endregion

        #region OCRWizardScreenWindow Constructor
        public OCRWizardScreenWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region Window Functions - PreviewKeyDown, Loaded, Closing....
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && canExit == true)
            {
                cancellationTokenSource.Cancel();
                OCRWizardCanceled(new EventArgs());
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;
            InitReferences();

            OpenChestButtonPositions = new List<sys_drawing.Point>();

            Snapture = new Snapture();
            Snapture.isDPIAware = true;
            Snapture.SetBitmapResolution((int)Snapture.MonitorInfo.Monitors[0].Dpi.X);
            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
            Snapture.Start(FrameCapturingMethod.GDI);

            CreateCanvasControls(CANVAS);

            this.onOCRWizardCancelled += OCRWizardScreenWindow_onOCRWizardCancelled;
            this.onOCRWizardCompleted += OCRWizardScreenWindow_onOCRWizardCompleted;
            this.onOCRWizardFailed += OCRWizardScreenWindow_onOCRWizardFailed;
            
            StatusMessage = $"Checking If player viewing Chests Tab...";
            
            Snapture.CaptureDesktop();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ref_image_files?.Clear();
            if (ref_images != null)
            {
                foreach (var refImage in ref_images)
                {
                    refImage?.Dispose();
                }
                ref_images?.Clear();
            }

            ChestWindowChestsActiveRefImage?.Dispose();
            OpenChestRefImage?.Dispose();
            OpenChestButtonPositions?.Clear();
            SuggestedAreaOfInterest?.Dispose();
            AreaOfInterest?.Dispose();
            WritableImage = null;
            writeableBitmap = null;
            Letters = null;
            Snapture?.Dispose();
            if(this != null && this.IsLoaded)
                this.DialogResult = isCompleted;
        }
        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            var source = (FancyButton)e.OriginalSource;
            if(source != null)
            {
                var tag = source.Tag;
                switch (tag)
                {
                    case "Yes":
                        PrepareClose();
                        break;
                }
            }
        }
        #endregion

        #region Wizard onWizardCancelled & onWizardCompleted functions
        private void OCRWizardScreenWindow_onOCRWizardCancelled(object sender, EventArgs e)
        {
            isCompleted = false;
            Cleanup();
            this.Close();
        }
        private void OCRWizardScreenWindow_onOCRWizardCompleted(object sender, EventArgs e)
        {
            //-- be sure to save OCR Settings.
            var ocrSettings = SettingsManager.Instance.Settings.OCRSettings;
            ocrSettings.AreaOfInterest = AreaOfInterest;
            ocrSettings.SuggestedAreaOfInterest = SuggestedAreaOfInterest;
            if(ocrSettings.ClaimChestButtons.Count > 0)
                ocrSettings.ClaimChestButtons.Clear();

            ocrSettings.ClaimChestButtons.AddRange(OpenChestButtonPositions);

            SettingsManager.Instance.Save();

            try
            {
                if (System.IO.File.Exists(".FIRSTRUN"))
                {
                    System.IO.File.Delete(".FIRSTRUN");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($@"Attempted to delete .FIRSTRUN file but couldn't. Please manually delete the file from the installation folder: Program Files\SicGames\TotalBattle Chest Tracker.");
            }
            isCompleted = true;

            if(ocrWindow != null)
            {
                ocrWindow.OCRWizardSuccesful();
                ocrWindow.Show();
            }
        }

        private void OCRWizardScreenWindow_onOCRWizardFailed(object sender, OCRWizardEventArgs e)
        {
            isCompleted = false;
            //--- must let user into manual OCR configuration.
            if(ocrWindow != null)
            {
                this.DialogResult = false;
                ocrWindow.OCRWizardFailed();
                this.Close();
            }

        }


        #endregion
        #region Snapture onFrameCaptured function
        private void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {

            if (e.ScreenCapturedBitmap != null)
            {
                var bitmap = e.ScreenCapturedBitmap;

                CapturedBitmap = (sys_drawing.Bitmap)bitmap.Clone();

                CloseCanvas.Visibility = Visibility.Visible;
                this.Opacity = 1;
                cancellationTokenSource = new CancellationTokenSource();

                StartOCRWizard(ref CapturedBitmap, cancellationTokenSource.Token);
            }
            else
            {
                Debug.WriteLine($"Major boo boo happened...");
            }

            PreviewImageSource = null;
            e.ScreenCapturedBitmap.Dispose();
        }
        #endregion


        #region Wizardary Stuff
        private Task<System.Drawing.Rectangle> performTemplateMatchingAsync(sys_drawing.Bitmap bitmap, Image<Bgr, byte> refImage, double minThreshold, double threshold, double sizeX = 32, double sizeY = 32) =>
           Task.Run(() => performTemplateMatching(bitmap, refImage, minThreshold, threshold, sizeX, sizeY));

        private System.Drawing.Rectangle performTemplateMatching(sys_drawing.Bitmap bitmap, Image<Bgr, byte> refImage, double minThreshold, double threshold,  double sizeX = 32, double sizeY = 32)
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
                return new sys_drawing.Rectangle();

            if (bitmap == null)
                return new System.Drawing.Rectangle(0, 0, 0, 0);

            Image<Bgr, byte> src = bitmap.ToImage<Bgr, byte>();

            System.Drawing.Rectangle match_rectangle = new System.Drawing.Rectangle();

            while (true)
            {
                double[] minValues, maxValues;
                System.Drawing.Point[] minLocations, maxLocations;

                using (Image<Gray, float> resultImage = src.MatchTemplate(refImage, TemplateMatchingType.CcoeffNormed))
                {
                    if(minThreshold > 0.0)
                        CvInvoke.Threshold(resultImage, resultImage, minThreshold, 1, ThresholdType.ToZero);
                    
                    resultImage.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if (maxValues[0] > threshold)
                    {
                        System.Windows.Point maxLocationsPoint = new System.Windows.Point(maxLocations[0].X,
                                maxLocations[0].Y);

                        var rectangle = new System.Drawing.Rectangle((int)maxLocationsPoint.X, (int)maxLocationsPoint.Y, (int)sizeX, (int)sizeY);
                        match_rectangle = rectangle;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            src.Dispose();
            src = null;
            return match_rectangle;
        }

        private Task<List<sys_drawing.Rectangle>> PerformMultipleTemplateMatchingAsync(sys_drawing.Bitmap bitmap, Image<Bgr, byte> refImage, double threshold) =>
            Task.Run(() => PerformMultipleTemplateMatching(bitmap, refImage, threshold));

        private List<System.Drawing.Rectangle> PerformMultipleTemplateMatching(sys_drawing.Bitmap bitmap, Image<Bgr, Byte> refImage, double threshold)
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
                return null;

            List<sys_drawing.Rectangle> matches = new List<sys_drawing.Rectangle>();

            if (bitmap == null)
                return matches;

            Image<Gray, byte> image_source = bitmap.ToImage<Gray, byte>();
            sys_drawing.Bitmap refImage_gray_bmp = refImage.ToBitmap();
            Image<Gray, byte> refImage_gray = refImage_gray_bmp.ToImage<Gray, byte>();
            Mat image_Out = new Mat();

            while (true)
            {
                double min_values, max_values;
                min_values = max_values = 0;
                sys_drawing.Point min_Locations = new sys_drawing.Point();
                sys_drawing.Point max_locations = new sys_drawing.Point();

                CvInvoke.MatchTemplate(image_source, refImage_gray, image_Out, TemplateMatchingType.CcoeffNormed);
                CvInvoke.Threshold(image_Out, image_Out, 0.8, 1.0, ThresholdType.ToZero);

                CvInvoke.MinMaxLoc(image_Out, ref min_values, ref max_values, ref min_Locations, ref max_locations);
                if (max_values > threshold)
                {

                    sys_drawing.Rectangle match_rect = new sys_drawing.Rectangle(max_locations, refImage.Size);
                    matches.Add(match_rect);

                    //--- block out that area
                    CvInvoke.Rectangle(image_source, match_rect, new MCvScalar(0, 255, 0), -1);
                }
                else
                    break;

            }

            image_Out.Dispose();
            refImage_gray.Dispose();
            refImage_gray_bmp.Dispose();
            image_source.Dispose();

            return matches;
        }

        private Task DetectOpenChestButtonAsync(sys_drawing.Bitmap bitmap, double threshold)
            => Task.Run(() => DetectOpenChestButtonAsync(bitmap, threshold));
        private Task<List<sys_drawing.Rectangle>> DetectOpenChestButton(System.Drawing.Bitmap bitmap, double threshold)
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
                return null;

            List<sys_drawing.Rectangle> matches = new List<sys_drawing.Rectangle>();
            Task t = new Task(() =>
            {
                matches = PerformMultipleTemplateMatching(bitmap, OpenChestRefImage, threshold);
            });
            t.Start();
            t.Wait();

            return Task.FromResult(matches);
        }

        private Task<bool> CheckIfPlayerOnChestsWindowTaskAsync(System.Drawing.Bitmap bitmap, double minThreshold, double threshold)
            => Task.Run(() => CheckIfPlayerOnChestsWindowTask(bitmap, minThreshold, threshold));


        private bool CheckIfPlayerOnChestsWindowTask(System.Drawing.Bitmap bitmap, double minThreshold, double threshold)
        {
            //-- should detect if player is on Chests Window.
            if (cancellationTokenSource.Token.IsCancellationRequested)
                return false;

            bool isActive = false;
            System.Drawing.Rectangle rect_result = performTemplateMatching(bitmap, ChestWindowChestsActiveRefImage, minThreshold, threshold);
            if (rect_result.X > 0 && rect_result.Y > 0)
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            return isActive;
        }

        private Task BeginRenderingTaskAsync() => Task.Run(() => BeginRenderingTask());
        private void BeginRenderingTask()
        {
            if (canRender)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        OCRWizardCanceled(new EventArgs());
                    }

                    try
                    {
                        using (writeableBitmap.GetBitmapContext())
                        {
                            writeableBitmap.Clear();

                            //-- Area Of Interest
                            /*
                            writeableBitmap.DrawRectangle((int)AreaOfInterest.x, (int)AreaOfInterest.y, (int)AreaOfInterest.width, (int)AreaOfInterest.height, Colors.DarkGray);
                            for (var i = 0; i < Thickness; i++)
                            {
                                writeableBitmap.DrawRectangle((int)AreaOfInterest.x--, (int)AreaOfInterest.y--,
                                    (int)AreaOfInterest.width++, (int)AreaOfInterest.height++, Colors.DarkGray);
                            }

                            foreach (Marker marker in AreaOfInterest.Markers)
                            {
                                writeableBitmap.FillRectangle((int)marker.x, (int)marker.y, (int)marker.x + 32, (int)marker.y + 32, Colors.White);
                            }

                            //-- Recommended Area Of Interest
                            writeableBitmap.DrawRectangle((int)SuggestedAreaOfInterest.x, (int)SuggestedAreaOfInterest.y, (int)SuggestedAreaOfInterest.width, (int)SuggestedAreaOfInterest.height, Colors.DarkGray);
                            for (var i = 0; i < Thickness; i++)
                            {
                                writeableBitmap.DrawRectangle((int)SuggestedAreaOfInterest.x--, (int)SuggestedAreaOfInterest.y--,
                                    (int)SuggestedAreaOfInterest.width++, (int)SuggestedAreaOfInterest.height++, Colors.DarkGreen);
                            }

                            foreach (Marker marker in SuggestedAreaOfInterest.Markers)
                            {
                                writeableBitmap.FillRectangle((int)marker.x, (int)marker.y, (int)marker.x + 32, (int)marker.y + 32, Colors.Green);
                            }

                            
                            */

                            foreach (var OpenChestButtonPosition in OpenChestButtonPositions)
                            {
                                writeableBitmap.FillEllipseCentered((int)OpenChestButtonPosition.X, (int)OpenChestButtonPosition.Y, 25, 25, Colors.Red);
                            }
                            //--- render the letters obtained from Tesseract
                            foreach (var letter in Letters)
                            {
                                var region = letter.Region;
                                region.Offset(8, 8);

                                var letterX = region.X + SuggestedAreaOfInterest.x;
                                var letterY = region.Y + SuggestedAreaOfInterest.y;
                                var letterWidth = (int)letterX + region.Width + 2;
                                var letterHeight = (int)letterY + region.Height + 2;

                                for (var stoke_thickness = 0; stoke_thickness < 3; stoke_thickness++)
                                {
                                    writeableBitmap.DrawRectangle((int)letterX--, (int)letterY--, letterWidth++, letterHeight++, Colors.Red);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        this.Hide();
                        throw new Exception(ex.Message);

                    }
                }));
            }
        }

        private void retry()
        {
            Snapture.CaptureDesktop();
        }
        private async void PerformORCWizard(sys_drawing.Bitmap bitmap, CancellationToken cancellationToken)
        {
            bool mustRetry = false;
            try
            {
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ErrorDialogCanvas.Visibility = Visibility.Hidden;
                    ProgressCanvas.Visibility = Visibility.Visible;
                }));


                if (cancellationToken.IsCancellationRequested)
                {
                    OCRWizardCanceled(new EventArgs());
                    return;
                }
                //var result = await CheckIfPlayerOnChestsWindowTaskAsync(bitmap, 0.3, 0.9);
                var result = await CheckIfPlayerOnChestsWindowTaskAsync(bitmap, min_treshold_gifts_tab, max_treshold_gifts_tab);
                if (result)
                {
                    isPlayerOnChestsTab = true;
                    Debug.WriteLine($"Player on Gifts Window Tab");
                }
                else
                {
                    isPlayerOnChestsTab= false;
                }

                if(!isPlayerOnChestsTab)
                {
                    Debug.WriteLine($"Player is not on Gifts Tab");
                    
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        retry_attempts++;
                        StatusMessage = $"Retrying ({retry_attempts} out of {max_retry_attempts}) times.";

                        if (retry_attempts == max_retry_attempts)
                        {
                            ErrorDialogCanvas.Visibility = Visibility.Visible;
                            ProgressCanvas.Visibility = Visibility.Hidden;
                            OCRWizardFailed(new OCRWizardEventArgs());
                            return;
                        }
                        
                        Thread.Sleep(5000);
                        //-- retry again.
                        mustRetry = true;
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }

                if (isPlayerOnChestsTab)
                {

                    //-- let's find chest window corners.
                    if (CornerRectangles.Count() > 0)
                        CornerRectangles.Clear();

                    StatusMessage = $"Performing Jedi Magic...";
                    foreach (var refImage in ref_images)
                    {

                        var rect = await performTemplateMatchingAsync(bitmap, refImage, 0.0, 0.6);
                        if (rect.Height != 0 && rect.Width != 0)
                        {
                            CornerRectangles.Add(rect);
                        }
                    }

                    var claim_buttons = await DetectOpenChestButton(bitmap, 0.9);

                    if (claim_buttons.Count() > 0 && claim_buttons != null)
                    {
                        foreach (var claim_button in claim_buttons)
                        {
                            claim_button.Offset(8, 8);
                            if (!claim_button.IsEmpty)
                            {
                                var claim_button_rect = claim_button;
                                var claim_center_x = (claim_button_rect.Width / 2) + claim_button_rect.X;
                                var claim_center_y = (claim_button_rect.Height / 2) + claim_button_rect.Y;
                                OpenChestButtonPositions.Add(new sys_drawing.Point(claim_center_x, claim_center_y));
                            }
                        }
                        Debug.WriteLine($"Detected Claim Chest Buttons");
                    }

                    if (CornerRectangles.Count() > 0)
                    {
                        canRender = true;
                        //--- configure AOI Rectangle
                        var offset = 16;
                        var rect1 = CornerRectangles[0]; //--- top left
                        var rect4 = CornerRectangles[3]; //-- bottom right

                        //-- find centers of markers
                        var cx1 = rect1.X + (rect1.Width / 2);
                        var cy1 = rect1.Y + (rect1.Height / 2);
                        var cx4 = rect4.X + (rect4.Width / 2);
                        var cy4 = rect4.Y + (rect4.Height / 2);

                        var cx = cx1 + offset;
                        var cy = cy1 + offset;
                        var width = cx4 - (offset * 0.5);
                        var height = cy4 - (offset * 0.5);
                        AreaOfInterest = new AOIRect(cx, cy, width, height);

                        //--- Now we can give a Recommended Area Of Interest.
                        double recommended_x = cx + 230;
                        double recommended_width = width - 400;
                        SuggestedAreaOfInterest = new AOIRect(recommended_x, cy + 1, recommended_width, height - 1);

                        Debug.WriteLine($"Creating Area Of Interest Markers");
                    }

                    //-- We should give a quick test and have user confirm everything.
                    CroppedBitmap croppedBitmap = new CroppedBitmap(bitmap.ToBitmapSource(),
                        new Int32Rect((int)SuggestedAreaOfInterest.x, (int)SuggestedAreaOfInterest.y, (int)SuggestedAreaOfInterest.width - (int)SuggestedAreaOfInterest.x, (int)SuggestedAreaOfInterest.height - (int)SuggestedAreaOfInterest.y));

                    var bmp = croppedBitmap.ToBitmap();
                    Image<Gray, byte> tessy_image = bmp.ToImage<Gray, byte>();

                    var Tessy = TesseractHelper.GetTesseract();
                    Tessy.SetImage(tessy_image);
                    if (Tessy.Recognize() == 0)
                    {
                        Letters = Tessy.GetCharacters();
                    }

                    //--- clean up on isle 5
                    Tessy = null;
                    tessy_image.Dispose();
                    bmp.Dispose();
                    croppedBitmap = null;

                    ProgressCanvas.Visibility = Visibility.Hidden;

                    await BeginRenderingTaskAsync();

                    Debug.WriteLine($"OCR Wizard Completed");

                    PreviewImageSource = null;
                    CapturedBitmap.Dispose();
                    CornerRectangles.Clear();
                    
                    CloseCanvas.Visibility = Visibility.Hidden;
                    //ResultsDialog.Visibility = Visibility.Visible;
                    canExit = false;
                    PrepareClose();
                    return;
                }

                if(mustRetry)
                {
                    min_treshold_gifts_tab += 0.1;
                    PreviewImageSource = null;
                   // CapturedBitmap.Dispose();
                    retry();
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    OCRWizardCanceled(new EventArgs());
                    return;
                }
            }
        }

        private void StartOCRWizard(ref sys_drawing.Bitmap bitmap, CancellationToken token)
        {
            PerformORCWizard(bitmap, token);
        }

        private void RetryIsPlayerOnGiftsTab()
        {

        }
        #endregion

        #region Functions & Stuff
        private void InitReferences()
        {
            ref_image_files = new List<string>(new string[] {
                "Images/Refs/chest_window_ref_top_left.png",
                "Images/Refs/chest_window_ref_top_right.png",
                "Images/Refs/chest_window_ref_bottom_left.png",
                "Images/Refs/chest_window_ref_bottom_right.png"
            });

            var ref_file_not_found = false;
            foreach(var refFile in ref_image_files)
            {
                if (System.IO.File.Exists(refFile))
                    continue;
                else
                {
                    ref_file_not_found = true;
                    break;
                }
            }

            if(ref_file_not_found)
            {
                if(MessageBox.Show("Reference Images are not found inside Images/Refs folders.", "OCR Wizard Critical Error") == MessageBoxResult.OK)
                {
                    ref_image_files.Clear();
                    ref_image_files = null;
                    this.Close();
                }
            }

            ref_images = new List<Image<Bgr, byte>>();

            ChestWindowChestsActiveRefImage = new Image<Bgr, Byte>("Images/Refs/chest_window_chests_active.png");
            OpenChestRefImage = new Image<Bgr, Byte>("Images/Refs/chest_window_claim_button.png");
            foreach (var ref_file in ref_image_files)
            {
                var image = new Image<Bgr, Byte>(ref_file);
                if (image != null)
                {
                    ref_images.Add(image);
                }
            }
        }

        private void CreateCanvasControls(Canvas canvas)
        {
            dpi = Snapture.MonitorInfo.Monitors[0].Dpi;
            var scaleFactor = dpi.ScaleFactor;

            var canvasheight = canvas.ActualHeight * scaleFactor;// * dpiY;
            var canvaswidth = canvas.ActualWidth * scaleFactor; // * dpiX;
            writeableBitmap = new WriteableBitmap((int)canvaswidth, (int)canvasheight, dpi.X, dpi.Y, PixelFormats.Pbgra32, null);
            WritableImage = new System.Windows.Controls.Image();
            WritableImage.Source = writeableBitmap;
            canvas.Children.Add(WritableImage);
        }

        private void PrepareClose()
        {
            OCRWizardCompleted(new EventArgs());
        }

        private void ClearWritableBitmap()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                using (var wb = writeableBitmap.GetBitmapContext())
                {
                    wb.Clear();
                }
            }));
        }
        private void Cleanup()
        {
            if (PreviewImageSource != null)
                PreviewImageSource = null;

            if (CapturedBitmap != null)
                CapturedBitmap.Dispose();

            if (CornerRectangles.Count() > 0 || CornerRectangles != null)
                CornerRectangles.Clear();

            cancellationTokenSource = null;
            canExit = true;
        }
        #endregion

    }
}
