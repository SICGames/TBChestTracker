using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using com.HellstormGames.ScreenCapture;
using com.CaptainHook;
using com.HellstormGames.Imaging;
using com.HellstormGames.Imaging.Extensions;
using System.Data.SqlTypes;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using CefSharp.DevTools.Runtime;
using TBChestTracker.Engine;

using TBChestTracker.Effects;
using System.Linq;
using TBChestTracker.ViewModels;

namespace TBChestTracker
{
    public enum EditorMode
    {
        NONE,
        SELECTION
    };

    public partial class ClanmateEditorWindow : Window
    {
        public EditorMode editorMode = EditorMode.NONE;    

        private ClanmatesFloatingWindow _clanmatesFloatingWindow;

        private UIElement DraggableElement { get; set; }
        private System.Windows.Point dragOffset;
        private bool bCanDragElement = false;
        private bool bIsDraggingElement = false;

        private System.Windows.Point startPoint, endPoint;
        private Rectangle SelectionRect;
        private bool isDrawingSelectionRect = false;
        private bool bEnableSelection = false;

        private Snapture Snapture { get; set; }
        private Dpi dpi { get; set; }
        private float scaleFactor { get; set; }

        private Image PreviewImage { get; set; }
        private CaptainHook captainHook { get; set; }

        public Window ParentWindow { get; set; }

        public bool bCanDraw = false;

        public ClanmateEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _clanmatesFloatingWindow = new ClanmatesFloatingWindow();
            _clanmatesFloatingWindow.Owner = this;
            //_clanmatesFloatingWindow.DataContext = ((ClanmateValidationWindow)ParentWindow);
            _clanmatesFloatingWindow.ParentWindow = this;

            _clanmatesFloatingWindow.Show();
            Debug.WriteLine($"Window Dimension: {this.ActualWidth}x{this.ActualHeight}");
            UI_TOOLBAR.Width = (this.ActualWidth / 5);
            var windowWidth = this.ActualWidth;
            var windowCenteredHori = (windowWidth / 2) - (UI_TOOLBAR.ActualWidth /2);
            Canvas.SetLeft(UI_TOOLBAR, windowCenteredHori);

            Snapture = new Snapture();
            Snapture.isDPIAware = true;
            dpi = Snapture.MonitorInfo.Monitors[0].Dpi;
            scaleFactor = dpi.ScaleFactor;
            var dpiSetting = SettingsManager.Instance.Settings.OCRSettings.Dpi;
            Snapture.SetBitmapResolution((int)dpiSetting);

            Snapture.onFrameCaptured += Snapture_onFrameCaptured;
            Snapture.Start(FrameCapturingMethod.GDI);

            captainHook = new CaptainHook();

            if(ParentWindow is ClanmateValidationWindow clanmateValidationWindow)
            {
                if(clanmateValidationWindow.VerifiedClanmatesViewModel.VerifiedClanmates == null)
                {
                    clanmateValidationWindow.VerifiedClanmatesViewModel.VerifiedClanmates = new System.Collections.ObjectModel.ObservableCollection<VerifiedClanmate>();
                }
            }
        }


        private void StartRenderingRectangle(int x, int y)
        {
            if (!isDrawingSelectionRect && editorMode == EditorMode.SELECTION && bCanDraw == true)
            {
                Debug.WriteLine($"Mouse Pos => x: {x} y: {y}");
                try
                {
                    startPoint = PointFromScreen(new System.Windows.Point(x, y));
                    endPoint = startPoint;
                    SelectionRect = new Rectangle();
                    SelectionRect.Name = $"SelectionRectangle";
                    SelectionRect.StrokeThickness = 5;
                    SelectionRect.Stroke = Brushes.Red;
                    SelectionRect.Height = 1;
                    SelectionRect.Width = 1;
                    MAIN_CANVAS.Children.Add(SelectionRect);

                    Canvas.SetLeft(SelectionRect, startPoint.X * dpi.ScaleFactor);
                    Canvas.SetTop(SelectionRect, startPoint.Y * dpi.ScaleFactor);

                    isDrawingSelectionRect = true;
                }
                catch (Exception ex)
                {

                }
            }
        }
        private void UpdateRenderingRectangle(int x, int y)
        {
            if (editorMode == EditorMode.SELECTION && bCanDraw == true)
            {
                if (SelectionRect != null)
                {
                    try
                    {
                        endPoint = PointFromScreen(new System.Windows.Point(x, y));

                        SelectionRect.Width = Math.Abs(endPoint.X - startPoint.X);
                        SelectionRect.Height = Math.Abs(endPoint.Y - startPoint.Y);

                        Canvas.SetLeft(SelectionRect, Math.Min(endPoint.X, startPoint.X));
                        Canvas.SetTop(SelectionRect, Math.Min(endPoint.Y, startPoint.Y));

                        System.Windows.Point ScreenStart = MAIN_CANVAS.PointToScreen(startPoint);
                        System.Windows.Point ScreenEnd = MAIN_CANVAS.PointToScreen(endPoint);
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
        }
        private void ClearRenderingRectangle()
        {
            if (editorMode == EditorMode.SELECTION)
            {
                if (isDrawingSelectionRect)
                {
                    isDrawingSelectionRect = false;
                    
                    MAIN_CANVAS.Children.Remove(SelectionRect);

                    if (endPoint.X < 0) endPoint.X = 0;
                    if (endPoint.X >= MAIN_CANVAS.Width) endPoint.X = MAIN_CANVAS.Width - 1;
                    if (endPoint.Y < 0) endPoint.Y = 0;
                    if (endPoint.Y >= MAIN_CANVAS.Height) endPoint.Y = MAIN_CANVAS.Height - 1;

                    System.Windows.Point ScreenStart = PointToScreen(startPoint);
                    System.Windows.Point ScreenEnd = PointToScreen(endPoint);

                    int offset = 10;
                    Int32Rect CroppedRect = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                        (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                        (int)Math.Abs(ScreenEnd.X - ScreenStart.X),
                         (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));

                    CroppedRect.X += offset;
                    CroppedRect.Y += offset;
                    CroppedRect.Width -= (offset * 2);
                    CroppedRect.Height -= (offset * 2);

                    //-- this sometimes complains and crashes. 
                    try
                    {
                        Snapture.CaptureRegion(CroppedRect.X, CroppedRect.Y, CroppedRect.Width, CroppedRect.Height);
                    }
                    catch 
                    { 

                    }

                }
            }
        }
        private void CaptainHookMouseHookMessageHandler(object sender, MouseHookMessageEventArgs e)
        {
            if (editorMode == EditorMode.SELECTION)
            {
                var mousePosition = e.Position;
                if (e.MessageType == MouseMessage.LButtonDown)
                {
                    if (bCanDraw)
                    {
                        StartRenderingRectangle(mousePosition.x, mousePosition.y);
                    }
                }
                else if (e.MessageType == MouseMessage.LButtonUp)
                {
                    ClearRenderingRectangle();
                }
                else if (e.MessageType == MouseMessage.MouseMove)
                {
                    if (bCanDraw)
                    {
                        UpdateRenderingRectangle(mousePosition.x, mousePosition.y);
                    }
                }
            }
        }

        private void Snapture_onFrameCaptured(object sender, FrameCapturedEventArgs e)
        {
            var bitmap = e.ScreenCapturedBitmap;
            bool save = false;
            var outputPath = $@"{AppContext.Instance.AppFolder}Output";
#if DEBUG
            save = true;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(outputPath);
            if (di.Exists == false)
            {
                di.Create();
            }
#endif
            var globalBrightness = SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness;
            var threshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.Threshold);
            var maxThreshold = new Gray(SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);

            var original_image = bitmap.ToImage<Bgr, byte>();

            //-- OCR imaging Manipulation Pipeline
            var outputMat = ImageEffects.ConvertToGrayscale(original_image.Mat, save, $@"{outputPath}");
            
            outputMat = ImageEffects.Brighten(outputMat, globalBrightness, save, $@"{outputPath}");
            outputMat = ImageEffects.Resize(outputMat, 5, Emgu.CV.CvEnum.Inter.Linear, save, $@"{outputPath}");
            outputMat = ImageEffects.ThresholdBinaryInv(outputMat, threshold, maxThreshold, save, $@"{outputPath}");
            outputMat = ImageEffects.Erode(outputMat, 2, save, outputPath);

            var ocrResult = OCREngine.Read(outputMat);
            
            if (ocrResult != null)
            {
                var results = ocrResult.Words.ToArray();
                foreach (var result in results)
                {
                    if(ParentWindow != null)
                    {
                        ((ClanmateValidationWindow)ParentWindow).VerifiedClanmatesViewModel.Add(result);
                    }
                }
            }

            //-- Tidy Up
            outputMat.Dispose();
            original_image.Dispose();
            bitmap.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (captainHook != null)
            {
                captainHook.Dispose();
            }

            MAIN_CANVAS.Children.Clear();
            PreviewImage = null;
            SelectionRect = null;
            Snapture.Dispose();

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {

            bool bHasVerifiedClanmates = false;
            foreach(var wnd in Application.Current.Windows.OfType<ClanmateValidationWindow>())
            {
                var window = wnd as ClanmateValidationWindow;
                window.WindowState = WindowState.Normal;
                if (VerifiedClanmatesViewModel.Instance.VerifiedClanmates.Count > 0)
                {
                    bHasVerifiedClanmates = true;
                }
                if (bHasVerifiedClanmates)
                {
                    window.NavigateTo("Pages/ClanmatesValidation/ClanmatesValidationProcessingPage.xaml");
                    this.DialogResult = true;
                }
            }

            this.Close();
        }

        private void Border_Click(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void CLANMATE_SELECTION_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            bEnableSelection = !bEnableSelection;

            editorMode = bEnableSelection == true ? EditorMode.SELECTION : EditorMode.NONE;
            if(editorMode == EditorMode.SELECTION)
            {
                bCanDraw = true;
                CLANMATE_SELECTION_BUTTON.Background = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                Cursor = Cursors.Cross;

                captainHook.onMouseHookMessage += CaptainHookMouseHookMessageHandler;
                captainHook.Install();
            }
            else
            {
                CLANMATE_SELECTION_BUTTON.Background = new SolidColorBrush(Colors.Transparent);
                Cursor = Cursors.Arrow;
                MAIN_CANVAS.Background = Brushes.Transparent;

                captainHook.onMouseHookMessage -= CaptainHookMouseHookMessageHandler;   
                captainHook.Uninstall();
            }

        }

        private void GRIP_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!bEnableSelection)
            {
                this.Cursor = Cursors.SizeAll;
                bCanDragElement = true;
            }
        }

        private void GRIP_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!bEnableSelection)
            {
                this.Cursor = Cursors.Arrow;
                bCanDragElement = false;
            }
        }
      
        private void MAIN_CANVAS_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (editorMode == EditorMode.NONE)
            {
                if (DraggableElement == null)
                {
                    return;
                }
                else
                {
                    bIsDraggingElement = true;
                    this.Cursor = Cursors.SizeAll;
                    var position = e.GetPosition(sender as IInputElement);
                    Canvas.SetTop(DraggableElement, position.Y - dragOffset.Y);
                    Canvas.SetLeft(DraggableElement, position.X - dragOffset.X);
                }
            }

          
        }

        private void MAIN_CANVAS_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (editorMode == EditorMode.NONE)
            {
                if (DraggableElement != null)
                {
                    this.Cursor = Cursors.Arrow;
                    DraggableElement = null;
                    MAIN_CANVAS.ReleaseMouseCapture();
                    bCanDragElement = false;
                    bIsDraggingElement = false;
                }
            }
          
        }

        private void MAIN_CANVAS_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void UI_TOOLBAR_MouseEnter(object sender, MouseEventArgs e)
        {
            editorMode = EditorMode.NONE;
            Cursor = Cursors.Arrow;
            isDrawingSelectionRect = false;
        }

        private void UI_TOOLBAR_MouseLeave(object sender, MouseEventArgs e)
        {
            if(bEnableSelection)
            {
                editorMode = EditorMode.SELECTION;
                Cursor = Cursors.SizeAll;
            }
        }

        private void CLANMATE_TEXTBOX_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = (TextBox)sender;
            if (e.Key == Key.Enter)
            {
                var text = tb.Text;
                if (String.IsNullOrEmpty(text))
                {
                    return;
                }

                VerifiedClanmatesViewModel.Instance.Add(text);
                tb.Text = String.Empty;
            }
        }

        private void UI_TOOLBAR_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            if (bCanDragElement)
            {
                DraggableElement = (UIElement)sender;
                dragOffset = e.GetPosition(this.MAIN_CANVAS);
                dragOffset.X -= Canvas.GetLeft(DraggableElement);
                dragOffset.Y -= Canvas.GetTop(DraggableElement);
                MAIN_CANVAS.CaptureMouse();
            }
           
        }
    }
}
