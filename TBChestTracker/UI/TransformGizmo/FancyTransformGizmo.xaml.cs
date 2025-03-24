using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBChestTracker.UI.TransformGizmo.RoutedEvents;

namespace TBChestTracker.UI
{
    /// <summary>
    /// Interaction logic for FancyTransformGizmo.xaml
    /// </summary>
    public partial class FancyTransformGizmo : UserControl
    {
        private static readonly DependencyProperty ShowTransformPointsProperty = 
            DependencyProperty.Register("ShowTransformPoints",
            typeof(bool),
            typeof(FancyTransformGizmo),
            new PropertyMetadata(true, new PropertyChangedCallback(ShowTransformPointsPropertyChanged)));

        private static readonly DependencyProperty SpacingProperty = 
            DependencyProperty.Register("Spacing", typeof(Int32),
            typeof(FancyTransformGizmo),
            new PropertyMetadata(50, null));

        public Int32 Spacing
        {
            get => (Int32)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public bool ShowTransformPoints
        {
            get => (bool)GetValue(ShowTransformPointsProperty);
            set => SetValue(ShowTransformPointsProperty, value);
        }

        private bool _isDragging = false;
        public bool IsDragging
        {
            get => _isDragging;
            set => _isDragging = value;
        }

        private bool _isMouseDown = false;
        private bool _isResizing = false;
        private bool _lockXY = false;
        private Canvas canvas;
        private Point OriginatingPoint, StartingPoint, EndingPoint;
        private double OriginalHeight, OriginalWidth;

        //-- routed events
        private static readonly RoutedEvent TransformMovedEvent = EventManager.RegisterRoutedEvent("TransformGizmoMoved",
            RoutingStrategy.Bubble,
            typeof(GizmoMovedEventHandler),
            typeof(FancyTransformGizmo));

        public event GizmoMovedEventHandler TransformGizmoMoved
        {
            add { AddHandler(TransformMovedEvent, value); }
            remove { AddHandler(TransformMovedEvent, value); }
        }

        private static readonly RoutedEvent TransformGizmoSizeChangedEvent = 
            EventManager.RegisterRoutedEvent("TransformGizmoSizeChanged",
            RoutingStrategy.Bubble,
            typeof(GizmoSizeChangedEventHandler),
            typeof(FancyTransformGizmo));

        public event GizmoSizeChangedEventHandler TransformGizmoSizeChanged
        {
            add { AddHandler(TransformGizmoSizeChangedEvent, value); }
            remove {  RemoveHandler(TransformGizmoSizeChangedEvent, value); }
        }

        public FancyTransformGizmo()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isDragging = false;
            _isMouseDown = false;
            _isResizing = false;

        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (LB_CORNER.IsMouseOver)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else if (RB_CORNER.IsMouseOver)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (RT_CORNER.IsMouseOver)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else if (LT_CORNER.IsMouseOver)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else
                this.Cursor = Cursors.SizeAll;

        }
        public void ReleaseResizing()
        {
            _isResizing = false;
            _isDragging = false;
            _isMouseDown = false;
            canvas = null;
        }

        private static void ShowTransformPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FancyTransformGizmo gizmo = (FancyTransformGizmo)d;
            gizmo.enableTransformPoints((bool)e.NewValue);
        }
        private void enableTransformPoints(bool visible)
        {
            foreach(var child in GIZMO_GRID.Children)
            {
                if(child is FancyTransformPoint ftp)
                {
                    ftp.Visibility = visible == true ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }
    
        private void LT_CORNER_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = this.Parent; //-- returns as Canvas
            if(parent is Canvas c)
            {
                canvas = c;
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                var r = this.ActualWidth + x;
                var b = this.ActualHeight + y;
                OriginalHeight = b;
                OriginalWidth = r;

                StartingPoint = new Point(r, b);

                _isResizing = true;
                _isMouseDown = true;
                _isDragging = false;
                _lockXY = false;

                OriginatingPoint = e.GetPosition(canvas);
                OriginatingPoint.X -= x;
                OriginatingPoint.Y -= y;

                canvas.PreviewMouseMove += Canvas_LT_PreviewMouseMove;
                canvas.PreviewMouseLeftButtonUp += Canvas_PreviewMouseLeftButtonUp;
                e.Handled = true;
            }
        }

        private void LT_CORNER_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_isResizing &&  _isMouseDown)
            {
                _isResizing  = false;
                _isMouseDown = false;
                canvas.PreviewMouseMove -= Canvas_LT_PreviewMouseMove;
                canvas = null;
            }
        }

        private void RT_CORNER_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = this.Parent; //-- returns as Canvas
            if (parent is Canvas c)
            {
                canvas = c;
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                var r = this.ActualWidth + x;
                var b = this.ActualHeight + y;

                StartingPoint = new Point(x, b);

                _isResizing = true;
                _isMouseDown = true;
                _isDragging = false;
                _lockXY = true;
                OriginatingPoint = e.GetPosition(canvas);
                OriginatingPoint.X -= r;
                OriginatingPoint.Y -= y;

                canvas.PreviewMouseMove += Canvas_RT_PreviewMouseMove;
                canvas.PreviewMouseLeftButtonUp += Canvas_PreviewMouseLeftButtonUp;
                e.Handled = true;
            }
        }


        private void RT_CORNER_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing && _isMouseDown)
            {
                _isResizing = false;
                _isMouseDown = false;
                canvas.PreviewMouseMove -= Canvas_RT_PreviewMouseMove;
                canvas = null;
            }
        }

        private void RB_CORNER_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = this.Parent; //-- returns as Canvas
            if (parent is Canvas c)
            {
                canvas = c;
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                var r = this.ActualWidth + x;
                var b = this.ActualHeight + y;
                
                StartingPoint = new Point(x, y);
                
                _isResizing = true;
                _isMouseDown = true;
                _isDragging = false;
                _lockXY = true;
                OriginatingPoint = e.GetPosition(canvas);
                OriginatingPoint.X -= r;
                OriginatingPoint.Y -= b;

                canvas.PreviewMouseMove += Canvas_RB_PreviewMouseMove;
                canvas.PreviewMouseLeftButtonUp += Canvas_PreviewMouseLeftButtonUp;
                e.Handled = true;
            }
        }

        private void RB_CORNER_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing && _isMouseDown)
            {
                _isResizing = false;
                _isMouseDown = false;
                canvas.PreviewMouseMove -= Canvas_RB_PreviewMouseMove;
                canvas = null;
            }
        }

        private void LB_CORNER_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = this.Parent; //-- returns as Canvas
            if (parent is Canvas c)
            {
                canvas = c;
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                var r = this.ActualWidth + x;
                var b = this.ActualHeight + y;

                StartingPoint = new Point(r, y);

                _isResizing = true;
                _isMouseDown = true;
                _isDragging = false;
                _lockXY = false;
                OriginatingPoint = e.GetPosition(canvas);
                OriginatingPoint.X -= x;
                OriginatingPoint.Y -= b;

                canvas.PreviewMouseMove += Canvas_LB_PreviewMouseMove;
                canvas.PreviewMouseLeftButtonUp += Canvas_PreviewMouseLeftButtonUp;
                e.Handled = true;
            }
        }

        private void LB_CORNER_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing && _isMouseDown)
            {
                _isResizing = false;
                _isMouseDown = false;
                canvas.PreviewMouseMove -= Canvas_LB_PreviewMouseMove;
                canvas = null;
            }
        }

        private void Canvas_LT_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && _isResizing && canvas != null)
            {
                EndingPoint = e.GetPosition(canvas);

                this.Width = Math.Abs(StartingPoint.X - EndingPoint.X);
                this.Height = Math.Abs(StartingPoint.Y - EndingPoint.Y);

                Canvas.SetLeft(this, Math.Min(EndingPoint.X, StartingPoint.X) - OriginatingPoint.X);
                Canvas.SetTop(this, Math.Min(EndingPoint.Y, StartingPoint.Y) - OriginatingPoint.Y);

                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                GizmoSizeChangedRoutedEventArgs sizeChangedArgs = new(TransformGizmoSizeChangedEvent, x, y, Width, Height, Spacing);
                RaiseEvent(sizeChangedArgs);

                e.Handled = true;
            }
        }
        private void Canvas_LB_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && _isResizing && canvas != null)
            {
                EndingPoint = e.GetPosition(canvas);

                Canvas.SetLeft(this, Math.Min(EndingPoint.X, StartingPoint.X) - OriginatingPoint.X);
                this.Width = Math.Abs(StartingPoint.X - EndingPoint.X);
                this.Height = Math.Abs(EndingPoint.Y - StartingPoint.Y);

                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                GizmoSizeChangedRoutedEventArgs sizeChangedArgs = new(TransformGizmoSizeChangedEvent, x, y, Width, Height, Spacing);
                RaiseEvent(sizeChangedArgs);


                e.Handled = true;
            }
        }
        private void Canvas_RT_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && _isResizing && canvas != null)
            {
                EndingPoint = e.GetPosition(canvas);

                Canvas.SetTop(this, Math.Min(EndingPoint.Y, StartingPoint.Y) - OriginatingPoint.Y);
                this.Width = Math.Abs(EndingPoint.X - StartingPoint.X);
                this.Height = Math.Abs(StartingPoint.Y - EndingPoint.Y);

                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                GizmoSizeChangedRoutedEventArgs sizeChangedArgs = new(TransformGizmoSizeChangedEvent, x, y, Width, Height, Spacing);
                RaiseEvent(sizeChangedArgs);


                e.Handled = true;
            }
        }
        private void Canvas_RB_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && _isResizing && canvas != null)
            {
                EndingPoint = e.GetPosition(canvas);

                this.Width = Math.Abs(EndingPoint.X - StartingPoint.X);
                this.Height = Math.Abs(EndingPoint.Y - StartingPoint.Y);

                if (_lockXY == false)
                {
                    var l = EndingPoint.X < StartingPoint.X ?
                        EndingPoint.X - OriginatingPoint.X :
                        StartingPoint.X + OriginatingPoint.X;

                    var t = EndingPoint.Y < StartingPoint.Y ?
                        EndingPoint.Y - OriginatingPoint.Y :
                        StartingPoint.Y + OriginatingPoint.Y;

                    Canvas.SetLeft(this, l);
                    Canvas.SetTop(this, t);
                }
                var x = Canvas.GetLeft(this);
                var y = Canvas.GetTop(this);
                GizmoSizeChangedRoutedEventArgs sizeChangedArgs = new(TransformGizmoSizeChangedEvent, x, y, Width, Height, Spacing);
                RaiseEvent(sizeChangedArgs);


                e.Handled = true;
            }
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(_isMouseDown && IsDragging && canvas != null)
            {
                var mousepos = e.GetPosition(canvas);
                var x = mousepos.X - OriginatingPoint.X;
                var y = mousepos.Y - OriginatingPoint.Y;
                Canvas.SetLeft(this, x);
                Canvas.SetTop(this, y);
                TransformGizmoMovedEventArgs args = new(TransformMovedEvent, x, y, Spacing);
                RaiseEvent(args);
                e.Handled = true;
            }
        }

        private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing && _isMouseDown && canvas != null)
                ReleaseResizing();
        }

        private void TRANSFORM_RECT_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDown == false)
            {
                _isMouseDown = true;
                IsDragging = true;
                canvas = this.Parent as Canvas;
                OriginatingPoint = e.GetPosition(canvas);
                OriginatingPoint.X -= Canvas.GetLeft(this);
                OriginatingPoint.Y -= Canvas.GetTop(this);
                canvas.PreviewMouseMove += Canvas_PreviewMouseMove;
                e.Handled = true;
            }
        }

        private void TRANSFORM_RECT_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDown && IsDragging)
            {
                _isMouseDown = false;
                IsDragging = false;
            }
        }
    }
}
