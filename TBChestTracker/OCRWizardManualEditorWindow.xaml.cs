using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.HellstormGames.Imaging.Extensions;
using com.HellstormGames.ScreenCapture;
using Emgu.CV.Structure;
using Emgu.CV;
using TBChestTracker.Helpers;
using TBChestTracker.Managers;
using Emgu.CV.OCR;
using System.Security;
using com.HellstormGames.Imaging;
using TBChestTracker.Engine;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OCRWizardManualEditorWindow.xaml
    /// </summary>
    /// 

    public enum OCRManualMode
    {
        REGION_SELECTION = 0,
        MARKER_PLACEMENT = 1
    }

    public partial class OCRWizardManualEditorWindow : Window
    {
        private bool isDragging = false;
        Point startPoint, endPoint, markerPoint;
        Rectangle SelectionRectangle {  get; set; } 
        AOIRect SuggestedAreaOfInterest { get; set; }

        Int32Rect CroppedRect { get; set; }
        Brush SelectionRectangleColor = Brushes.Red;
        private int lineThickness = 3;

        private Snapture snapture;
        public OCRManualMode OCRManualMode { get; set; }
        private BitmapSource preview {  get; set; }
        Tesseract.Word[] tessy_characters { get; set; }

        private WriteableBitmap writeableBitmap { get; set; }
        private Image WriteableImage { get; set; }

        private bool mouseDown = false;

        private Dpi dpi { get; set; }
        private double scaleFactor = 0.0;


        public OCRWizardManualEditorWindow()
        {
            InitializeComponent();
        }
       
        private void CreateCanvasControls()
        {
         
            var canvasheight = PreviewCanvas.ActualHeight * scaleFactor;
            var canvaswidth = PreviewCanvas.ActualWidth * scaleFactor; 

            writeableBitmap = new WriteableBitmap((int)canvaswidth, (int)canvasheight, dpi.X, dpi.Y, PixelFormats.Pbgra32, null);
            WriteableImage = new System.Windows.Controls.Image();
            WriteableImage.Source = writeableBitmap;
            PreviewCanvas.Children.Add(WriteableImage);

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;

            snapture = new Snapture();
            snapture.isDPIAware = true;
            dpi = snapture.MonitorInfo.Monitors[0].Dpi;  
            scaleFactor = dpi.ScaleFactor;

            snapture.SetBitmapResolution((int)dpi.X);
            snapture.onFrameCaptured += Snapture_onFrameCaptured;
            snapture.Start(FrameCapturingMethod.GDI);
            snapture.CaptureDesktop();

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();

        }

        private void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            var bitmap = e.ScreenCapturedBitmap;
            PreviewImage.Source = bitmap.ToBitmapSource();
            this.Opacity = 1;
            CreateCanvasControls();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            //-- grab fresh copy of region created.
            if (!String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.PreviewImage))
                SettingsManager.Instance.Settings.OCRSettings.PreviewImage = String.Empty;

            var aoi = SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest;

            var cropped_bitmap = new CroppedBitmap(PreviewImage.Source as BitmapSource, new Int32Rect((int)aoi.x, (int)aoi.y, (int)aoi.width, (int)aoi.height));
            
            var bmp = cropped_bitmap.ToBitmap();

            bmp.Save($@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png");
            SettingsManager.Instance.Settings.OCRSettings.PreviewImage = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}\preview_image.png";
            Image<Gray, byte> image = bmp.ToImage<Gray, byte>();
            var imageBrightened = image.Mul(SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness) + SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness;
            var imageScaled = image.Resize(5, Emgu.CV.CvEnum.Inter.Cubic);

            var threshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.Threshold);
            var maxThreshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);

            var imageThreshold = image.ThresholdBinaryInv(threshold, maxThreshold);
            
            SettingsManager.Instance.Save();

            imageThreshold.Dispose();
            imageScaled.Dispose();
            imageBrightened.Dispose();
            image.Dispose();
            bmp.Dispose();
            cropped_bitmap = null;

            this.DialogResult = true;
            this.Cursor = Cursors.Arrow;
        }

        private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void PreviewCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OCRManualMode == OCRManualMode.REGION_SELECTION)
            {
                startPoint = Mouse.GetPosition(PreviewCanvas);
                endPoint = startPoint;
                
                SelectionRectangle = new Rectangle();
                SelectionRectangle.StrokeThickness = 5;
                SelectionRectangle.Height = 1;
                SelectionRectangle.Width = 1;
                SelectionRectangle.Stroke = Brushes.Red;
                PreviewCanvas.Children.Add(SelectionRectangle);

                Canvas.SetLeft(SelectionRectangle, startPoint.X * dpi.X);
                Canvas.SetTop(SelectionRectangle, startPoint.Y * dpi.Y);

                if (mouseDown == false)
                {
                    PreviewCanvas.PreviewMouseMove += PreviewCanvas_PreviewMouseMove;
                    PreviewCanvas.PreviewMouseLeftButtonUp += PreviewCanvas_PreviewMouseLeftButtonUp;
                    PreviewCanvas.CaptureMouse();
                    mouseDown = true;
                }
            }
            else
            {
                markerPoint = Mouse.GetPosition(PreviewCanvas);
                markerPoint = PreviewCanvas.PointToScreen(markerPoint);
                if (mouseDown == false)
                {
                    PreviewCanvas.PreviewMouseLeftButtonUp += PreviewCanvas_PreviewMouseLeftButtonUp;
                    PreviewCanvas.CaptureMouse();
                    mouseDown = true;
                }
            }
        }

        private void PreviewCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDown)
            {
                if (OCRManualMode == OCRManualMode.REGION_SELECTION)
                {
                    PreviewCanvas.ReleaseMouseCapture();
                    PreviewCanvas.PreviewMouseMove -= PreviewCanvas_PreviewMouseMove;
                    PreviewCanvas.PreviewMouseLeftButtonUp -= PreviewCanvas_PreviewMouseLeftButtonUp;

                    PreviewCanvas.Children.Remove(SelectionRectangle);


                    if (endPoint.X < 0) endPoint.X = 0;
                    if (endPoint.X >= PreviewCanvas.Width) endPoint.X = PreviewCanvas.Width - 1;
                    if (endPoint.Y < 0) endPoint.Y = 0;
                    if (endPoint.Y >= PreviewCanvas.Height) endPoint.Y = PreviewCanvas.Height - 1;

                    Point ScreenStart = PreviewCanvas.PointToScreen(startPoint);
                    Point ScreenEnd = PreviewCanvas.PointToScreen(endPoint);

                    CroppedRect = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                        (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                        (int)Math.Abs(ScreenEnd.X - ScreenStart.X),
                         (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));


                    // Note that the CroppedBitmap object's SourceRect
                    // is immutable so we must create a new CroppedBitmap.
                    CroppedBitmap cropped_bitmap = null;
                    BitmapSource bms = null;

                    Tesseract.Word[] tessy_result = null;

                    try
                    {
                        bms = (BitmapSource)PreviewImage.Source;
                        cropped_bitmap = new CroppedBitmap(bms, CroppedRect);

                        SuggestedAreaOfInterest = new AOIRect(CroppedRect.X, CroppedRect.Y, CroppedRect.Width, CroppedRect.Height);

                        SelectionRectangle = null;

                        //-- Now we have Tesseract read the cropped image.
                        System.Drawing.Bitmap result = cropped_bitmap.ToBitmap();

                        if (result != null)
                        {
                            Image<Gray, byte> result_image = result.ToImage<Gray, byte>();
                            
                            //Image<Gray, byte> scaled_image = result_image.Resize(3, Emgu.CV.CvEnum.Inter.Cubic);
                            
                            var brightness = SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness;
                            var threshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.Threshold);
                            var maxThreshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);
                            
                            Image<Gray, byte> modified_image = result_image.Mul(brightness) + brightness;
                            var thresholdImage = modified_image.ThresholdBinaryInv(threshold, maxThreshold);
                            //var erodedImage = thresholdImage.Erode(1);6

                            //--- grab words from region 
                            //--- ensure to make sure user is happy as a gopher
                            //--- after confirmation, save rectangle and move onto Open button editor.

                            var tessy = OCREngine.OCR;
                            tessy.SetImage(thresholdImage);
                            if (tessy.Recognize() == 0)
                            {
                                tessy_result = tessy.GetWords();
                                tessy_characters = tessy_result;
                            }

                            //-- clean up
                            tessy = null;
                            //erodedImage.Dispose();
                            thresholdImage.Dispose();
                            modified_image.Dispose();
                            modified_image = null;
                            // scaled_image.Dispose();
                            // scaled_image = null;
                            result_image.Dispose();
                            result_image = null;
                            result.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    cropped_bitmap = null;
                    bms = null;

                    //--- let's render the rectangles around the words 
                    var hasRenderedResult = false;

                    if (tessy_characters != null)
                    {
                        //--- render the letters obtained from Tesseract
                        foreach (var letter in tessy_characters)
                        {
                            var region = letter.Region;
                            region.Offset(8, 8);

                            var letterX = region.X + SuggestedAreaOfInterest.x;
                            var letterY = region.Y + SuggestedAreaOfInterest.y;
                            var letterWidth = (int)letterX + region.Width + 2;
                            var letterHeight = (int)letterY + region.Height + 2;

                            for (var stoke_thickness = 0; stoke_thickness < 3; stoke_thickness++)
                            {
                                writeableBitmap.DrawRectangle((int)letterX--, (int)letterY--, letterWidth++, letterHeight++, Colors.Green);
                            }
                            hasRenderedResult = true;
                        }

                        if (hasRenderedResult)
                        {
                            //--- make sure user is happy as a duck
                            var result = MessageBox.Show($"Are you happy with the result?", "OCR Result", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                //-- we can close
                                this.DialogResult = true;
                                //-- be sure to save area of interest
                                SettingsManager.Instance.Settings.OCRSettings.SuggestedAreaOfInterest = SuggestedAreaOfInterest;
                                PreviewCanvas.PreviewMouseLeftButtonDown -= PreviewCanvas_PreviewMouseLeftButtonDown;
                                PreviewCanvas.PreviewMouseMove -= PreviewCanvas_PreviewMouseMove;
                                PreviewCanvas.PreviewMouseLeftButtonUp -= PreviewCanvas_PreviewMouseLeftButtonUp;
                                this.Close();
                            }
                            else
                            {
                                //--- we need to redo it
                                ResetCanvas();

                                mouseDown = false;
                            }
                        }
                    }
                }
                else if(OCRManualMode == OCRManualMode.MARKER_PLACEMENT)
                {
                    //-- now we render the marker 

                    if (markerPoint != null)
                    {
                        writeableBitmap.FillEllipseCentered((int)markerPoint.X + 8, (int)markerPoint.Y + 8, 25, 25, Colors.Red);
                        var messageresult = MessageBox.Show("Are you happy with the result?", "Marker Result", MessageBoxButton.YesNo);

                        if (messageresult == MessageBoxResult.Yes)
                        {
                            this.DialogResult = true;
                            if(SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons.Count() > 0)
                            {
                                SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons.Clear();
                            }

                            SettingsManager.Instance.Settings.OCRSettings.ClaimChestButtons.Add(new System.Drawing.Point((int)markerPoint.X, (int)markerPoint.Y));
                            PreviewCanvas.PreviewMouseLeftButtonDown -= PreviewCanvas_PreviewMouseLeftButtonDown;
                            PreviewCanvas.PreviewMouseMove -= PreviewCanvas_PreviewMouseMove;
                            PreviewCanvas.PreviewMouseLeftButtonUp -= PreviewCanvas_PreviewMouseLeftButtonUp;
                            this.Close();
                        }
                        else
                        {
                            ResetCanvas();
                            mouseDown = false;
                        }

                    }
                }
            }
        }

        private void ResetCanvas()
        {
            tessy_characters = null;
            PreviewCanvas.PreviewMouseLeftButtonUp -= PreviewCanvas_PreviewMouseLeftButtonUp;
            PreviewCanvas.PreviewMouseMove -= PreviewCanvas_PreviewMouseMove;

            writeableBitmap.Clear();

        }
        private void PreviewCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (OCRManualMode == OCRManualMode.REGION_SELECTION)
            {
                endPoint = Mouse.GetPosition(PreviewCanvas);
                SelectionRectangle.Width = Math.Abs(endPoint.X - startPoint.X);
                SelectionRectangle.Height = Math.Abs(endPoint.Y - startPoint.Y);

                Canvas.SetLeft(SelectionRectangle, Math.Min(endPoint.X, startPoint.X));
                Canvas.SetTop(SelectionRectangle, Math.Min(endPoint.Y, startPoint.Y));
                
                Point ScreenStart = PreviewCanvas.PointToScreen(startPoint);
                Point ScreenEnd = PreviewCanvas.PointToScreen(endPoint);

                CroppedRect = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                    (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                    (int)Math.Abs(ScreenEnd.X - ScreenStart.X),
                     (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));

            }
        }
    }
}
