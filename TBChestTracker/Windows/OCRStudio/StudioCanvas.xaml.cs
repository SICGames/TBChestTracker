using com.HellStormGames.Diagnostics;
using com.HellStormGames.Imaging;
using com.HellStormGames.Imaging.Extensions;
using com.HellStormGames.Imaging.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using TBChestTracker.Extensions;
using TBChestTracker.Managers;
using TBChestTracker.UI;


namespace TBChestTracker.Windows.OCRStudio
{
    /// <summary>
    /// Interaction logic for StudioCanvas.xaml
    /// </summary>
    public partial class StudioCanvas : Window, INotifyPropertyChanged
    {

        #region Declarations
        private StudioToolbarWindow toolbarWindow;
        public StudioCanvasTools SelectedCanvasTool { get; set; }

        private WriteableBitmap WriteableBitmap { get; set; }
        private System.Windows.Point startpoint, endpoint;
        private System.Windows.Shapes.Rectangle selection_rectangle { get; set; }
        private AOIRect AreaOfInterest { get; set; }
        private BitmapSource _PreviewSource;


        public CroppedBitmap AreaOfInterestImage { get; private set; }

        private Image WriteableImage { get; set; }
        private DPI Dpi => Snapster.MonitorConfiguration.Monitor.Dpi;
        private float DpiScaleFactor => Dpi.X / 96.0f;
        private System.Windows.Point WindowLocation { get; set; }

        private bool isMouseDown = false;
        private bool isDraggingObject = false;
        private UIElement SelectedCanvasObject { get; set; }

        private Int32Rect AreaOfInterestCroppedRegion { get; set; }

        private bool isCanvasObjectSelected;
        private Point OriginSelectedClick;
        private Point ClickMarkerPoint;
        private Image ClickMarkerImage;

        private Ellipse[] TransformCorners { get; set; }

        private Brush OriginalRegionColor { get; set; }
        private FancyTransformGizmo TransformGizmo { get; set; }

        public bool UserHasAppliedChanges { get; set; }

        public BitmapSource PreviewSource
        {
            get { return _PreviewSource; }
            set
            {
                _PreviewSource = value;
                OnPropertyChanged(nameof(PreviewSource));
            }
        }
        #endregion

        #region OnPropertyChanged Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region StudioCanvas Constructor
        public StudioCanvas()
        {
            InitializeComponent();
        }
        #endregion

        #region Window Loaded Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;
            this.DataContext = this;
            AreaOfInterest = new AOIRect();

            using (var image = Snapster.CaptureDesktop())
            {
                var bitmap = image.ToBitmap();
                PreviewSource = bitmap.AsBitmapSource();
                this.Opacity = 1;
                CreateCanvasControls();
            }

            toolbarWindow = new StudioToolbarWindow();
            toolbarWindow.Top = 10;
            //var left = (this.ActualWidth * 0.5) - ((toolbarWindow.ActualWidth * 0.5) * 2);
            var left = (this.ActualWidth * 0.5f) - ((toolbarWindow.Width * 0.5f));
            toolbarWindow.Left = left;
            toolbarWindow.Owner = this;
            toolbarWindow.Show();

            //-- need to check to see if there's an existing OCR profile.
            var ocrprofile = ClanManager.Instance.GetOcrProfile(ClanManager.Instance.ClanSettings.OcrProfileManager.CurrentOcrProfileName);
            if (ocrprofile != null)
            {
                AreaOfInterest = ocrprofile;
                startpoint = new Point(AreaOfInterest.x, AreaOfInterest.y);
                endpoint = new Point(AreaOfInterest.width, AreaOfInterest.height);
                startpoint = DrawingCanvas.PointFromScreen(startpoint);
                endpoint = DrawingCanvas.PointFromScreen(endpoint);
                /*
                AreaOfInterest.x /= DpiScaleFactor;
                AreaOfInterest.y /= DpiScaleFactor;
                AreaOfInterest.width /= DpiScaleFactor;
                AreaOfInterest.height /= DpiScaleFactor;

                //AreaOfInterest.Markers[0].x *= DpiScaleFactor; 
                //AreaOfInterest.Markers[0].y *= DpiScaleFactor;
                */


                selection_rectangle = new Rectangle();
                selection_rectangle.Name = "AreaOfInterest";
                selection_rectangle.StrokeThickness = 5;
                selection_rectangle.Height = endpoint.Y;
                selection_rectangle.Width = endpoint.X;
                selection_rectangle.Stroke = Brushes.Red;
                OriginalRegionColor = selection_rectangle.Stroke;

                selection_rectangle.Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));
                selection_rectangle.UseLayoutRounding = true;
                selection_rectangle.SnapsToDevicePixels = true;

                selection_rectangle.MouseEnter += Selection_rectangle_MouseEnter;
                selection_rectangle.MouseLeave += Selection_rectangle_MouseLeave;
                selection_rectangle.PreviewMouseLeftButtonDown += Selection_rectangle_PreviewMouseLeftButtonDown;
                selection_rectangle.PreviewMouseLeftButtonUp += Selection_rectangle_PreviewMouseLeftButtonUp;

                DrawingCanvas.Children.Add(selection_rectangle);

                Canvas.SetLeft(selection_rectangle, startpoint.X);
                Canvas.SetTop(selection_rectangle, startpoint.Y);

                startpoint = new Point(Canvas.GetLeft(selection_rectangle), Canvas.GetTop(selection_rectangle));    
                endpoint = new Point(selection_rectangle.Width, selection_rectangle.Height);

                if (endpoint.X < 0) endpoint.X = 0;
                if (endpoint.X >= DrawingCanvas.ActualWidth) endpoint.X = DrawingCanvas.ActualWidth - 1;
                if (endpoint.Y < 0) endpoint.Y = 0;
                if (endpoint.Y >= DrawingCanvas.ActualHeight) endpoint.Y = DrawingCanvas.ActualHeight - 1;

                Point ScreenStart = DrawingCanvas.PointToScreen(startpoint);
                Point ScreenEnd = DrawingCanvas.PointToScreen(endpoint);
                int rx = (int)ScreenStart.X;
                int ry = (int)ScreenStart.Y;
                int rx1 = (int)ScreenEnd.X;
                int ry1 = (int)ScreenEnd.Y;
                AreaOfInterest.x = rx;
                AreaOfInterest.y = ry;
                AreaOfInterest.width = rx1;
                AreaOfInterest.height = ry1;
                AreaOfInterestCroppedRegion = new Int32Rect((int)AreaOfInterest.x, (int)AreaOfInterest.y, (int)AreaOfInterest.width, (int)AreaOfInterest.height);

                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.UriSource = new Uri("pack://application:,,,/Images/ui/ClickMarkerIcon.png", UriKind.RelativeOrAbsolute);
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                if (ClickMarkerImage == null)
                {
                    ClickMarkerImage = new Image();
                    ClickMarkerImage.Width = 32;
                    ClickMarkerImage.Height = 32;
                    ClickMarkerImage.Stretch = Stretch.UniformToFill;
                    ClickMarkerImage.Name = "ClickMarker";
                    ClickMarkerImage.Source = bitmapimage;
                    ClickMarkerImage.PreviewMouseLeftButtonDown += ClickMarkerImage_PreviewMouseLeftButtonDown;
                    ClickMarkerImage.PreviewMouseLeftButtonUp += ClickMarkerImage_PreviewMouseLeftButtonUp;
                    ClickMarkerImage.MouseEnter += ClickMarkerImage_MouseEnter;
                    ClickMarkerImage.MouseLeave += ClickMarkerImage_MouseLeave;
                    DrawingCanvas.Children.Add(ClickMarkerImage);

                    ClickMarkerPoint = new Point(AreaOfInterest.ClickTarget.X, AreaOfInterest.ClickTarget.Y);
                    ClickMarkerPoint = DrawingCanvas.PointFromScreen(ClickMarkerPoint);

                    //ClickMarkerPoint.X += ClickMarkerImage.Width / 2;
                    //ClickMarkerPoint.Y += ClickMarkerImage.Height / 2;

                    Canvas.SetLeft(ClickMarkerImage, ClickMarkerPoint.X);
                    Canvas.SetTop(ClickMarkerImage, ClickMarkerPoint.Y);

                    isMouseDown = false;
                    toolbarWindow.CanRestartCanvas = true;
                    toolbarWindow.CanApplyChanges = true;
                    toolbarWindow.EnableOcrPreview = true;
                    ClickMarkerImage = null;
                }

            }
        }
        #endregion

        #region Windows Closed & Closing Events
        private void Window_Closed(object sender, EventArgs e)
        {
            WriteableImage = null;
            WriteableBitmap = null;
            PreviewSource = null;

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = UserHasAppliedChanges;
        }
        #endregion

        #region TransformGizmo Events

        private void TransformGizmo_TransformGizmoMoved(object sender, TransformGizmoMovedEventArgs e)
        {
            var mousepos = Mouse.GetPosition(DrawingCanvas);
            var rect = SelectedCanvasObject;
            Canvas.SetLeft(rect, e.X + (e.Spacing / 2));
            Canvas.SetTop(rect, e.Y + (e.Spacing / 2));
            if (rect is Rectangle r)
            {
                startpoint = new Point(Canvas.GetLeft(r), Canvas.GetTop(r));
                endpoint = new Point(r.Width, r.Height);

                if (endpoint.X < 0) endpoint.X = 0;
                if (endpoint.X >= DrawingCanvas.ActualWidth) endpoint.X = DrawingCanvas.ActualWidth - 1;
                if (endpoint.Y < 0) endpoint.Y = 0;
                if (endpoint.Y >= DrawingCanvas.ActualHeight) endpoint.Y = DrawingCanvas.ActualHeight - 1;

                Point ScreenStart = DrawingCanvas.PointToScreen(startpoint);
                Point ScreenEnd = DrawingCanvas.PointToScreen(endpoint);
                int rx = (int)ScreenStart.X;
                int ry = (int)ScreenStart.Y;
                int rx1 = (int)ScreenEnd.X;
                int ry1 = (int)ScreenEnd.Y;
                AreaOfInterest.x = rx;
                AreaOfInterest.y = ry;
                AreaOfInterest.width = rx1;
                AreaOfInterest.height = ry1;
                AreaOfInterestCroppedRegion = new Int32Rect((int)AreaOfInterest.x, (int)AreaOfInterest.y, (int)AreaOfInterest.width, (int)AreaOfInterest.height);
            }
            if (rect is Image img)
            {
                ClickMarkerPoint = new Point(Canvas.GetLeft(img), Canvas.GetTop(img));
                ClickMarkerPoint = DrawingCanvas.PointToScreen(ClickMarkerPoint);
                //ClickMarkerPoint.X -= (ClickMarkerImage?.Width / 2) ?? 32 / 2;
                //ClickMarkerPoint.Y -= (ClickMarkerImage?.Height / 2) ?? 32 / 2;

                AreaOfInterest.ClickTarget = new System.Drawing.Point((int)ClickMarkerPoint.X, (int)ClickMarkerPoint.Y);

            }
        }
        private void TransformGizmo_TransformGizmoSizeChanged(object sender, UI.TransformGizmo.RoutedEvents.GizmoSizeChangedRoutedEventArgs e)
        {
            try
            {
                var rect = SelectedCanvasObject;
                Canvas.SetLeft(rect, e.X + (e.Spacing / 2));
                Canvas.SetTop(rect, e.Y + (e.Spacing / 2));
                ((Rectangle)rect).Height = e.Height - e.Spacing;
                ((Rectangle)rect).Width = e.Width - e.Spacing;
                if (rect is Rectangle r)
                {
                    startpoint = new Point(Canvas.GetLeft(r), Canvas.GetTop(r));
                    endpoint = new Point(r.Width, r.Height);
                    if (endpoint.X < 0) endpoint.X = 0;
                    if (endpoint.X >= DrawingCanvas.ActualWidth) endpoint.X = DrawingCanvas.ActualWidth - 1;
                    if (endpoint.Y < 0) endpoint.Y = 0;
                    if (endpoint.Y >= DrawingCanvas.ActualHeight) endpoint.Y = DrawingCanvas.ActualHeight - 1;

                    Point ScreenStart = DrawingCanvas.PointToScreen(startpoint);
                    Point ScreenEnd = DrawingCanvas.PointToScreen(endpoint);
                    int rx = (int)ScreenStart.X;
                    int ry = (int)ScreenStart.Y;
                    int rx1 = (int)ScreenEnd.X;
                    int ry1 = (int)ScreenEnd.Y;
                    AreaOfInterest.x = rx;
                    AreaOfInterest.y = ry;
                    AreaOfInterest.width = rx1;
                    AreaOfInterest.height = ry1;
                    AreaOfInterestCroppedRegion = new Int32Rect((int)AreaOfInterest.x, (int)AreaOfInterest.y, (int)AreaOfInterest.width, (int)AreaOfInterest.height);
                }
            }
            catch (Exception ex)
            {
                //-- chances are it's the width < 0
            }
        }
        private void TransformGizmo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool && SelectedCanvasObject != null && isDraggingObject)
            {
                isDraggingObject = false;
                e.Handled = true;
            }
        }

        private void TransformGizmo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool && SelectedCanvasObject != null && isDraggingObject == false)
            {
                var gizmo = (FancyTransformGizmo)sender;
                isDraggingObject = true;
                OriginSelectedClick = e.GetPosition(DrawingCanvas);
                OriginSelectedClick.X -= Canvas.GetLeft(SelectedCanvasObject);
                OriginSelectedClick.Y -= Canvas.GetTop(SelectedCanvasObject);
                e.Handled = true;
            }
        }
        #endregion

        #region Selection Rectangle Events
        private void Selection_rectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                isDraggingObject = false;
                var t = SelectedCanvasTool.GetType();
                if (t == typeof(Rectangle))
                {
                    var rect = (Rectangle)SelectedCanvasObject;
                    if (rect != null)
                    {
                        rect.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 160, 255));
                    }
                }
            }
        }

        private void Selection_rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool && isDraggingObject == false)
            {
                if (SelectedCanvasObject != null)
                {
                    if (DrawingCanvas.Children.Contains(TransformGizmo))
                    {
                        DrawingCanvas.Children.Remove(TransformGizmo);
                    }
                    SelectedCanvasObject = null;
                }

                if (SelectedCanvasObject == null)
                {
                    var rect = (Rectangle)sender;
                    SelectedCanvasObject = rect;
                    TransformGizmo = new UI.FancyTransformGizmo();
                    var spacing = 10;
                    TransformGizmo.Width = rect.ActualWidth + spacing;
                    TransformGizmo.Height = rect.ActualHeight + spacing;
                    TransformGizmo.Spacing = spacing;
                    TransformGizmo.TransformGizmoMoved += TransformGizmo_TransformGizmoMoved;
                    TransformGizmo.TransformGizmoSizeChanged += TransformGizmo_TransformGizmoSizeChanged;
                    DrawingCanvas.Children.Add(TransformGizmo);
                    var l = Canvas.GetLeft(SelectedCanvasObject);
                    var t = Canvas.GetTop(SelectedCanvasObject);
                    Canvas.SetLeft(TransformGizmo, l - (spacing / 2));
                    Canvas.SetTop(TransformGizmo, t - (spacing / 2));

                }
            }
        }

        private void Selection_rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                Rectangle r = (Rectangle)sender;
                if (SelectedCanvasObject == null)
                {
                    r.Stroke = OriginalRegionColor;
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Selection_rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                if (SelectedCanvasObject == null)
                {
                    Rectangle r = (Rectangle)sender;
                    r.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 160, 255));
                }
                this.Cursor = Cursors.Hand;
            }
        }
        #endregion

        #region Click Marker events
        private void ClickMarkerImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ClickMarkerImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                this.Cursor = Cursors.Hand;
            }
        }

        private void ClickMarkerImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                isDraggingObject = false;
            }
        }

        private void ClickMarkerImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool && isDraggingObject == false)
            {
                if (SelectedCanvasObject != null)
                {
                    if (DrawingCanvas.Children.Contains(TransformGizmo))
                    {
                        DrawingCanvas.Children.Remove(TransformGizmo);
                    }
                    SelectedCanvasObject = null;
                }

                if (SelectedCanvasObject == null)
                {
                    var img = (Image)sender;
                    SelectedCanvasObject = img;
                    if (TransformGizmo != null)
                    {
                        //-- we need to get rid of the old one.
                        DrawingCanvas.Children.Remove(TransformGizmo);
                    }

                    TransformGizmo = new UI.FancyTransformGizmo();
                    TransformGizmo.ShowTransformPoints = false;
                    TransformGizmo.TransformGizmoMoved += TransformGizmo_TransformGizmoMoved;

                    var spacing = 10;
                    TransformGizmo.Width = img.ActualWidth + spacing;
                    TransformGizmo.Height = img.ActualHeight + spacing;
                    TransformGizmo.Spacing = spacing;
                    DrawingCanvas.Children.Add(TransformGizmo);
                    var l = Canvas.GetLeft(SelectedCanvasObject);
                    var t = Canvas.GetTop(SelectedCanvasObject);
                    Canvas.SetLeft(TransformGizmo, l - (spacing / 2));
                    Canvas.SetTop(TransformGizmo, t - (spacing / 2));
                }
            }
        }
        #endregion

        #region DrawingCanvas Functions And Events
        private void DrawingCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (SelectedCanvasTool == StudioCanvasTools.DrawRegionTool && selection_rectangle != null && isMouseDown)
            {
                endpoint = Mouse.GetPosition(DrawingCanvas);
                selection_rectangle.Width = Math.Abs(endpoint.X - startpoint.X);
                selection_rectangle.Height = Math.Abs(endpoint.Y - startpoint.Y);

                Canvas.SetLeft(selection_rectangle, Math.Min(endpoint.X, startpoint.X));
                Canvas.SetTop(selection_rectangle, Math.Min(endpoint.Y, startpoint.Y));

                this.Cursor = Cursors.Cross;
            }
            else if (SelectedCanvasTool == StudioCanvasTools.DrawMarkerTool)
            {
                this.Cursor = Cursors.Cross;
            }
        }

        private void DrawingCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.Hide();

            if (SelectedCanvasTool == StudioCanvasTools.DrawRegionTool)
            {
                var aoirectangles = DrawingCanvas.Children.Contains(selection_rectangle);
                if (aoirectangles)
                {
                    return;
                }

                startpoint = Mouse.GetPosition(DrawingCanvas);
                endpoint = startpoint;

                selection_rectangle = new Rectangle();
                selection_rectangle.Name = "AreaOfInterest";
                selection_rectangle.StrokeThickness = 5;
                selection_rectangle.Height = 1;
                selection_rectangle.Width = 1;
                selection_rectangle.Stroke = Brushes.Red;
                OriginalRegionColor = selection_rectangle.Stroke;

                selection_rectangle.Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));
                selection_rectangle.UseLayoutRounding = true;
                selection_rectangle.SnapsToDevicePixels = true;

                selection_rectangle.MouseEnter += Selection_rectangle_MouseEnter;
                selection_rectangle.MouseLeave += Selection_rectangle_MouseLeave;
                selection_rectangle.PreviewMouseLeftButtonDown += Selection_rectangle_PreviewMouseLeftButtonDown;
                selection_rectangle.PreviewMouseLeftButtonUp += Selection_rectangle_PreviewMouseLeftButtonUp;

                DrawingCanvas.Children.Add(selection_rectangle);

                Canvas.SetLeft(selection_rectangle, startpoint.X * DpiScaleFactor);
                Canvas.SetTop(selection_rectangle, startpoint.Y * DpiScaleFactor);

                if (isMouseDown == false)
                {
                    DrawingCanvas.PreviewMouseMove += DrawingCanvas_PreviewMouseMove;
                    DrawingCanvas.PreviewMouseLeftButtonUp += DrawingCanvas_PreviewMouseLeftButtonUp;
                    DrawingCanvas.CaptureMouse();
                    isMouseDown = true;
                }
            }
            else if (SelectedCanvasTool == StudioCanvasTools.DrawMarkerTool)
            {
                foreach (var child in DrawingCanvas.Children)
                {
                    if (child is Image img)
                    {
                        if (img.Name.Equals("ClickMarker"))
                        {
                            return;
                        }
                    }
                }

                ClickMarkerPoint = Mouse.GetPosition(DrawingCanvas);

                if (isMouseDown == false)
                {
                    DrawingCanvas.PreviewMouseLeftButtonUp += DrawingCanvas_PreviewMouseLeftButtonUp;
                    DrawingCanvas.CaptureMouse();
                    isMouseDown = true;
                }
            }
            else if (SelectedCanvasTool == StudioCanvasTools.SelectObjectTool)
            {
                var source = e.OriginalSource;

                if (source != null)
                {
                    var sourceType = source.GetType();
                    if (sourceType == typeof(Image))
                    {
                        var sourceName = ((Image)source).Name;
                        if (sourceName.Equals("ClickMarker") == false)
                        {
                            //-- we clicked on something image
                            if (SelectedCanvasObject != null)
                            {
                                if (DrawingCanvas.Children.Contains(TransformGizmo))
                                {
                                    DrawingCanvas.Children.Remove(TransformGizmo);
                                    var t = SelectedCanvasObject.GetType();
                                    if (t != null)
                                    {
                                        if (t == typeof(Rectangle))
                                        {
                                            var r = SelectedCanvasObject as Rectangle;
                                            r.Stroke = OriginalRegionColor;

                                        }
                                    }
                                    SelectedCanvasObject = null;
                                }
                            }
                        }
                    }
                    else if (sourceType == typeof(Ellipse))
                    {
                        //-- we clicked on a transform gizmo corner.
                        var sourceName = ((Ellipse)source).Name;
                        if (sourceName.Equals("TRANSFORM_POINT"))
                        {
                            //-- we need to grab the location of this gizmo transform point.

                        }
                    }

                }
            }
        }

        private void DrawingCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                if (SelectedCanvasTool == StudioCanvasTools.DrawRegionTool)
                {
                    DrawingCanvas.ReleaseMouseCapture();
                    DrawingCanvas.PreviewMouseMove -= DrawingCanvas_PreviewMouseMove;
                    DrawingCanvas.PreviewMouseLeftButtonUp -= DrawingCanvas_PreviewMouseLeftButtonUp;
                    selection_rectangle = null;
                    isMouseDown = false;
                    toolbarWindow.EnableOcrPreview = true;
                    toolbarWindow.CanRestartCanvas = true;
                    toolbarWindow.CanApplyChanges = true;

                    if (endpoint.X < 0) endpoint.X = 0;
                    if (endpoint.X >= DrawingCanvas.ActualWidth) endpoint.X = DrawingCanvas.ActualWidth - 1;
                    if (endpoint.Y < 0) endpoint.Y = 0;
                    if (endpoint.Y >= DrawingCanvas.ActualHeight) endpoint.Y = DrawingCanvas.ActualHeight - 1;

                    Point ScreenStart = DrawingCanvas.PointToScreen(startpoint);
                    Point ScreenEnd = DrawingCanvas.PointToScreen(endpoint);
                    int rx = (int)ScreenStart.X;
                    int ry = (int)ScreenStart.Y;
                    int rx1 = (int)ScreenEnd.X;
                    int ry1 = (int)ScreenEnd.Y;
                    AreaOfInterest.x = rx;
                    AreaOfInterest.y = ry;
                    AreaOfInterest.width = rx1 - rx;
                    AreaOfInterest.height = ry1 - ry;
                    AreaOfInterestCroppedRegion = new Int32Rect((int)AreaOfInterest.x,(int) AreaOfInterest.y, (int)AreaOfInterest.width, (int)AreaOfInterest.height);
                }
                else if (SelectedCanvasTool == StudioCanvasTools.DrawMarkerTool)
                {
                    DrawingCanvas.ReleaseMouseCapture();
                    DrawingCanvas.PreviewMouseLeftButtonUp -= DrawingCanvas_PreviewMouseLeftButtonUp;
                    //this.Hide();

                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.UriSource = new Uri("Images/ui/ClickMarkerIcon.png", UriKind.RelativeOrAbsolute);
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    if (ClickMarkerImage == null)
                    {
                        ClickMarkerImage = new Image();
                        ClickMarkerImage.Width = 32;
                        ClickMarkerImage.Height = 32;
                        ClickMarkerImage.Stretch = Stretch.UniformToFill;
                        ClickMarkerImage.Name = "ClickMarker";
                        ClickMarkerImage.Source = bitmapimage;
                        ClickMarkerImage.PreviewMouseLeftButtonDown += ClickMarkerImage_PreviewMouseLeftButtonDown;
                        ClickMarkerImage.PreviewMouseLeftButtonUp += ClickMarkerImage_PreviewMouseLeftButtonUp;
                        ClickMarkerImage.MouseEnter += ClickMarkerImage_MouseEnter;
                        ClickMarkerImage.MouseLeave += ClickMarkerImage_MouseLeave
                            ;
                        DrawingCanvas.Children.Add(ClickMarkerImage);

                        var cpX = ClickMarkerPoint.X - (ClickMarkerImage.Width / 2);
                        var cpY = ClickMarkerPoint.Y - (ClickMarkerImage.Height / 2);

                        Canvas.SetLeft(ClickMarkerImage, cpX);
                        Canvas.SetTop(ClickMarkerImage, cpY);

                        var clickpoint = new Point(Canvas.GetLeft(ClickMarkerImage), Canvas.GetTop(ClickMarkerImage));
                        clickpoint = DrawingCanvas.PointToScreen(clickpoint);

                        AreaOfInterest.ClickTarget = new System.Drawing.Point((int)clickpoint.X, (int)clickpoint.Y);

                        isMouseDown = false;
                        toolbarWindow.CanRestartCanvas = true;
                        toolbarWindow.CanApplyChanges = true;
                        ClickMarkerImage = null;
                    }


                }
            }
        }

        #endregion

        #region CreateCanvasControls()
        private void CreateCanvasControls()
        {
            var canvasheight = DrawingCanvas.ActualHeight * DpiScaleFactor;
            var canvaswidth = DrawingCanvas.ActualWidth * DpiScaleFactor;

            WriteableBitmap = new WriteableBitmap((int)canvaswidth, (int)canvasheight, Dpi.X, Dpi.Y, PixelFormats.Pbgra32, null);
            WriteableImage = new System.Windows.Controls.Image();
            WriteableImage.Source = WriteableBitmap;
            DrawingCanvas.Children.Add(WriteableImage);
            WindowLocation = new Point(this.Left, this.Top);
        }
        #endregion

        #region InvokeMouseMove()
        public void InvokeMouseMove()
        {
            DrawingCanvas.PreviewMouseMove += DrawingCanvas_PreviewMouseMove;
        }
        #endregion

        #region DestroyMouseMoveEventListener()
        public void DestroyMouseMoveEventListener()
        {
            DrawingCanvas.PreviewMouseMove -= DrawingCanvas_PreviewMouseMove;
        }
        #endregion

        #region ApplyAndSave()
        public void ApplyAndSave()
        {
            //-- grab fresh copy of region created.
            try
            {
                if (!String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.PreviewImage))
                    SettingsManager.Instance.Settings.OCRSettings.PreviewImage = String.Empty;

                var previewbitmap = GetRegionOfInterestImage();
                System.Drawing.Bitmap b = null;
                if (previewbitmap != null)
                {
                    b = previewbitmap.AsBitmap();
                    b.Save($@"{AppContext.Instance.LocalApplicationPath}\preview_image.png");
                    b.Dispose();
                    previewbitmap = null;
                }

                SettingsManager.Instance.Settings.OCRSettings.PreviewImage = $@"{AppContext.Instance.LocalApplicationPath}\preview_image.png";
                ClanManager.Instance.UpdateOcrProfile(ClanManager.Instance.GetCurrentOcrProfileName(), AreaOfInterest);

                SettingsManager.Instance.Save();
                ClanManager.Instance.ClanSettings.Save();
                
                this.Cursor = Cursors.Arrow;
                this.Close();

            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "OCR Wizard Closing", "Issue with OCR Wizard while closing window.  See log for more information.");
            }
        }
        #endregion

        #region ExitStudio();
        public void ExitStudio()
        {
            this.Close();
        }
        #endregion

        #region GetRetionOfInterestImage
        public BitmapSource GetRegionOfInterestImage(bool saveImage = false)
        {
            try
            {
                /*
                if (endpoint.X < 0) endpoint.X = 0;
                if (endpoint.X >= DrawingCanvas.ActualWidth) endpoint.X = DrawingCanvas.ActualWidth - 1;
                if (endpoint.Y < 0) endpoint.Y = 0;
                if (endpoint.Y >= DrawingCanvas.ActualHeight) endpoint.Y = DrawingCanvas.ActualHeight - 1;

                Point ScreenStart = DrawingCanvas.PointToScreen(startpoint);
                Point ScreenEnd = DrawingCanvas.PointToScreen(endpoint);
                int rx = (int)ScreenStart.X;
                int ry = (int)ScreenStart.Y;
                int rx1 = (int)ScreenEnd.X;
                int ry1 = (int)ScreenEnd.Y;

                var width = ScreenEnd.X - ScreenStart.X;
                var height = ScreenEnd.Y - ScreenStart.Y;

                bool expected = false;
                expected = height == ry1 && width == rx1;

                if (!expected)
                {
                    AreaOfInterestCroppedRegion = new Int32Rect(rx, ry, (int)width, (int)height);
                }
                else
                {
                    AreaOfInterestCroppedRegion = new Int32Rect(rx, ry, rx1, ry1);
                }
                               
                AreaOfInterestCroppedRegion = new Int32Rect((int)Math.Min(ScreenEnd.X, ScreenStart.X),
                   (int)Math.Min(ScreenEnd.Y, ScreenStart.Y),
                   (int)Math.Abs(ScreenEnd.X - ScreenStart.X),
                    (int)Math.Abs(ScreenEnd.Y - ScreenStart.Y));
                */
                AreaOfInterestImage = new CroppedBitmap(PreviewSource, AreaOfInterestCroppedRegion);

                if (AreaOfInterestImage == null)
                {
                    Loggio.Warn("Unable to create cropped image from region of interest.");
                    return null;
                }
                if (saveImage)
                {
                    var imagePath = $"{AppContext.Instance.LocalApplicationPath}OCR-Preview-Image.png";
                    AreaOfInterestImage.AsBitmap().Save(imagePath);
                }

                return AreaOfInterestImage;
            }
            catch (Exception e)
            {
                Loggio.Error(e, "OCR Studio", "Failed to create cropped image.");
                return null;
            }
        }
        #endregion
    }
}