using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using com.HellStormGames.ScreenCapture;
using Emgu.CV;
using Emgu.CV.Shape;
using Emgu.CV.Structure;
using TBChestTracker.Helpers;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for PreviewScreenshotWindow.xaml
    /// </summary>
    public partial class PreviewScreenshotWindow : Window
    {
        Snapture Snapture { get; set; }
        public String ScreenshotImage { get; set; }

        private Rectangle SelectionRectangle { get; set; }
        private Rectangle FillRectangle { get; set; }
        private Point Start, End;
        private Int32Rect CroppedRect { get; set; }

        private int SelectionThickness = 3;
        BitmapSource PreviewImageSource { get; set; }

        PreviewCroppedImageViewer previewer { get; set; }

        public string clanmateName { get; set; }

        public PreviewScreenshotWindow()
        {
            InitializeComponent();
            this.DataContext = this;    
            previewer = new PreviewCroppedImageViewer();
            CroppedRect = Int32Rect.Empty;

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            previewer.Hide();
            this.Opacity = 0;
            //this.Hide();
            Snapture = new Snapture();
            Snapture.isDPIAware = true;
            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
            Snapture.Start(FrameCapturingMethod.GDI);
            
            Snapture.CaptureDesktop();
        }

        private void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            if(e.ScreenCapturedBitmap != null)
            {
                var bitmap = e.ScreenCapturedBitmap;
                var file = $@"{IOHelper.ApplicationFolder}\PreviewScreenShot.jpg";
                
                PreviewImageSource =  BitmapHelper.ConvertFromBitmap(bitmap, (Int32)144, (Int32)144);

                Debug.WriteLine($"Screenshot Bitmap DPI ({bitmap.HorizontalResolution}, {bitmap.VerticalResolution})");
                Debug.WriteLine($"Preview Image DPI ({PreviewImageSource.DpiX}, {PreviewImageSource.DpiY})");
                
                PreviewImage.Source = PreviewImageSource; // _image;
                this.Opacity = 1;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            previewer.Close();
            PreviewImageSource = null;

            Snapture.Release();
        }

        private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void PreviewCanvas_MouseMove(object sender, MouseEventArgs e)
        {
          
            End = Mouse.GetPosition(PreviewCanvas);
            
            SelectionRectangle.Width = Math.Abs(End.X - Start.X);
            SelectionRectangle.Height = Math.Abs(End.Y - Start.Y);

            FillRectangle.Width = SelectionRectangle.Width;
            FillRectangle.Height = SelectionRectangle.Height;   

            Canvas.SetLeft(SelectionRectangle, Math.Min(End.X, Start.X));
            Canvas.SetTop(SelectionRectangle, Math.Min(End.Y, Start.Y));
            Canvas.SetLeft(FillRectangle, Math.Min(End.X, Start.X));
            Canvas.SetTop(FillRectangle, Math.Min(End.Y, Start.Y));

            Point ScreenStart = PreviewCanvas.PointToScreen(Start);
            Point ScreenEnd = PreviewCanvas.PointToScreen(End);

            CroppedRect = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                (int)Math.Abs(ScreenEnd.X - ScreenStart.X),
                 (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));

            var bms = (BitmapSource)PreviewImage.Source;
            try
            {
                previewer.Left = End.X + 2;
                previewer.Top = End.Y + 2;
                previewer.Width =  CroppedRect.Width;
                previewer.Height = CroppedRect.Height;
                previewer.PreviewCroppedBitmap = new CroppedBitmap((BitmapSource)PreviewImage.Source, CroppedRect);

            }
            catch(Exception ex)
            {
                //-- do nothing
            }
        }

        private void PreviewCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            Start =Mouse.GetPosition(PreviewCanvas);
            End = Start;
            
            var dpi = Snapture.Instance.MonitorInfo.Monitors[0].Dpi;

            SelectionRectangle = new Rectangle();
            SelectionRectangle.StrokeThickness = SelectionThickness;
            SelectionRectangle.Height = 1;
            SelectionRectangle.Width = 1;
            SelectionRectangle.Stroke = Brushes.DarkGray;

            FillRectangle = new Rectangle();
            FillRectangle.Width = SelectionRectangle.Width;
            FillRectangle.Height = SelectionRectangle.Height;
            FillRectangle.Fill = Brushes.White;
            FillRectangle.Opacity = 0.5f;
            
            PreviewCanvas.Children.Add(SelectionRectangle);
            PreviewCanvas.Children.Add(FillRectangle);
            
            Canvas.SetLeft(SelectionRectangle, Start.X * dpi.X);
            Canvas.SetTop(SelectionRectangle, Start.Y * dpi.Y);

            Canvas.SetLeft(FillRectangle, Start.X * dpi.X);
            Canvas.SetTop(FillRectangle, Start.Y * dpi.Y);

            PreviewCanvas.MouseUp += PreviewCanvas_MouseUp;
            PreviewCanvas.MouseMove += PreviewCanvas_MouseMove;
            PreviewCanvas.CaptureMouse();

            previewer.Left = End.X;
            previewer.Top = End.Y;
            previewer.Width = 1;
            previewer.Height = 1;

            previewer.Show();
        }


        private void PreviewCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviewCanvas.ReleaseMouseCapture();
            PreviewCanvas.MouseMove -= PreviewCanvas_MouseMove;
            PreviewCanvas.MouseUp -= PreviewCanvas_MouseUp;
            PreviewCanvas.Children.Remove(SelectionRectangle);
            PreviewCanvas.Children.Remove(FillRectangle);

            FillRectangle = null;
            
            previewer.Hide();
           
            if (End.X < 0) End.X = 0;
            if (End.X >= PreviewCanvas.Width) End.X = PreviewCanvas.Width - 1;
            if (End.Y < 0) End.Y = 0;
            if (End.Y >= PreviewCanvas.Height) End.Y = PreviewCanvas.Height - 1;

            Point ScreenStart = PreviewCanvas.PointToScreen(Start);
            Point ScreenEnd = PreviewCanvas.PointToScreen(End);

            CroppedRect = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                (int)Math.Abs(ScreenEnd.X -ScreenStart.X),
                 (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));

          
            // Note that the CroppedBitmap object's SourceRect
            // is immutable so we must create a new CroppedBitmap.

            CroppedBitmap cropped_bitmap = null;
            BitmapSource bms = null;

            bool clanmateConfirmed = false;

            try
            {
                bms = (BitmapSource)PreviewImage.Source;
                cropped_bitmap = new CroppedBitmap(bms, CroppedRect);
                
                SelectionRectangle = null;

                //-- Now we have Tesseract read the cropped image.
                System.Drawing.Bitmap result = BitmapHelper.ConvertFromBitmapSource(cropped_bitmap);

                if (result != null)
                {
                    Image<Gray, byte> result_image = result.ToImage<Gray, byte>();
                    Image<Gray, byte> modified_image = result_image.Mul(0.75f) + 0.75f;
                    modified_image.Save($@"selected_area.jpg");
                    var ocrResult = TesseractHelper.Read(modified_image);
                    if (ocrResult != null)
                    {
                        clanmateName = ocrResult.Words[0];
                        var dialog = MessageBox.Show($"Clanmate detected as '{clanmateName}' is this correct?", "Add New Clanmate", MessageBoxButton.YesNo);
                        if(dialog == MessageBoxResult.Yes)
                        {
                            //-- clean up and add clanmate. 
                            clanmateConfirmed = true;
                        }
                        System.Diagnostics.Debug.WriteLine($"Clan mate name detected as: {ocrResult.Words[0]}");
                    }
                    ocrResult.Words.Clear();
                    modified_image.Dispose();
                    modified_image = null;
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

            if(clanmateConfirmed)
            {
                ClanManager.Instance.ClanmateManager.Add(clanmateName);
                this.Close();
            }
        }
    }
}
