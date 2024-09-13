using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanmateEditorWindow.xaml
    /// </summary>
    public partial class ClanmateEditorWindow : Window
    {
        private ClanmatesFloatingWindow _clanmatesFloatingWindow;

        private UIElement DraggableElement { get; set; }
        private Point dragOffset;
        private bool bCanDragElement = false;
        private bool bIsDraggingElement = false;

        public ClanmateEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
           // var hwnd = new WindowInteropHelper(this).Handle;
            //WindowHelper.SetWindowExTransparent(hwnd);
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
            _clanmatesFloatingWindow.Show();
            Debug.WriteLine($"Window Dimension: {this.ActualWidth}x{this.ActualHeight}");
            UI_TOOLBAR.Width = (this.ActualWidth / 5);
            var windowWidth = this.ActualWidth;
            var windowCenteredHori = (windowWidth / 2) - (UI_TOOLBAR.ActualWidth /2);
            Canvas.SetLeft(UI_TOOLBAR, windowCenteredHori);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_Click(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void CLANMATE_SELECTION_BUTTON_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GRIP_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeAll;
            bCanDragElement = true;
        }

        private void GRIP_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            bCanDragElement = false;
        }

        private void GRIP_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void MAIN_CANVAS_PreviewMouseMove(object sender, MouseEventArgs e)
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

        private void MAIN_CANVAS_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
