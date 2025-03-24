using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TBChestTracker.NativeInterlop;
using TBChestTracker.UI;
using static System.Net.WebRequestMethods;

namespace TBChestTracker.Windows.OCRStudio
{
    /// <summary>
    /// Interaction logic for StudioToolbarWindow.xaml
    /// </summary>
    public partial class StudioToolbarWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool isDragging = false;
        public bool mouseDown = false;

        OCRResultsPreviewWindow OCRResultsPreviewWindow;
        private StudioCanvas ParentWindow = null;

        private bool? _enableOCRPreview;
        public bool EnableOcrPreview
        {
            get => _enableOCRPreview.GetValueOrDefault(false);
            set
            {
                _enableOCRPreview = value;
                OnPropertyChanged(nameof(EnableOcrPreview));
            }
        }
        private bool? _CanRestartCanvas;
        public bool CanRestartCanvas
        {
            get => _CanRestartCanvas.GetValueOrDefault(false);
            set
            {
                _CanRestartCanvas = value;
                OnPropertyChanged(nameof(CanRestartCanvas));
            }
        }

        private bool? _CanApplyChanges;
        public bool CanApplyChanges
        {
            get => _CanApplyChanges.GetValueOrDefault(false);
            set
            {
                _CanApplyChanges = value;
                OnPropertyChanged(nameof(CanApplyChanges));
            }
        }

        public StudioToolbarWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void GripBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeAll;
        }

        private void GripBar_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void ExitStudioButton_Click(object sender, RoutedEventArgs e)
        {
            StartOver();
            ParentWindow.UserHasAppliedChanges = false;
            ParentWindow.ExitStudio();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ParentWindow = (StudioCanvas)this.Owner;
            CanRestartCanvas = false;
            CanApplyChanges = false;
            EnableOcrPreview = false;
        }

        private void GripBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging =  true;
            mouseDown = true;
            this.DragMove();
        }

        private void GripBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mouseDown = false;
            
        }
       
        private void ClearToggleButtonsState()
        {
            //-- clear all toggle states.
            var toggleControls = StudioControls.Children;
            foreach (var toggleControl in toggleControls)
            {
                if (toggleControl is FancyToggleButton ft)
                {
                    ft.IsSelected = false;
                }
            }
        }
        private void SelectToggleButton(FancyToggleButton togglebutton)
        {
            //-- clear all toggle states.
            var toggleControls = StudioControls.Children;
            foreach (var toggleControl in toggleControls)
            {
                if (toggleControl is FancyToggleButton ft)
                {
                    if (ft != togglebutton)
                    {
                        ft.IsSelected = false;
                    }
                }
            }
        }

        private void StartOver()
        {
            var deleteList = new List<UIElement>();
            if (ParentWindow.DrawingCanvas.Children.Count > 0)
            {
                foreach (var child in ParentWindow.DrawingCanvas.Children)
                {
                    if (child is Ellipse el)
                    {
                        deleteList.Add(el);
                    }
                    if (child is Rectangle rect)
                    {
                        deleteList.Add(rect);
                    }
                    if (child is Image img)
                    {
                        if (img.Name.Contains("ClickMarker"))
                        {
                            deleteList.Add(img);
                        }
                    }
                    if(child is FancyTransformGizmo gizmo)
                    {
                        deleteList.Add((FancyTransformGizmo)gizmo);
                    }
                }
                for (var x = 0; x < deleteList.Count; x++)
                {
                    ParentWindow.DrawingCanvas.Children.Remove(deleteList[x]);
                }
            }
        }
        private void SaveApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.UserHasAppliedChanges = true;
            ParentWindow.ApplyAndSave();
        }

        private void SelectObjectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SelectObjectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tb = (FancyToggleButton)e.Source; 
            SelectToggleButton(tb);
            ParentWindow.SelectedCanvasTool = StudioCanvasTools.SelectObjectTool;
            ParentWindow.InvokeMouseMove();
        }

        private void DrawRegionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DrawRegionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tb = (FancyToggleButton)e.Source;
            SelectToggleButton(tb);
            ParentWindow.SelectedCanvasTool = StudioCanvasTools.DrawRegionTool;
        }

        private void DrawClickMarker_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DrawClickMarker_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tb = (FancyToggleButton)e.Source;
            SelectToggleButton(tb);
            ParentWindow.SelectedCanvasTool = StudioCanvasTools.DrawMarkerTool;
        }

        private void PreviewResultsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = EnableOcrPreview;
        }

        private void PreviewResultsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (OCRResultsPreviewWindow != null)
                return;

            OCRResultsPreviewWindow = new OCRResultsPreviewWindow();
            //-- needs to obtain cropped bitmap source.
            OCRResultsPreviewWindow.PreviewImage = ParentWindow.GetRegionOfInterestImage(true);
            OCRResultsPreviewWindow.Owner = ParentWindow;
            OCRResultsPreviewWindow.Show();
        }

        private void StartOverCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
           e.CanExecute = CanRestartCanvas;
        }

        private void StartOverCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClearToggleButtonsState();

            StartOver();
            EnableOcrPreview = false;
            CanRestartCanvas = false;
            CanApplyChanges = false;
        }
    }
}
