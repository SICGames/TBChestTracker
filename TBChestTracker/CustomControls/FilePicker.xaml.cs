using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for FilePicker.xaml
    /// </summary>
    public partial class FilePicker : System.Windows.Controls.UserControl
    {

        public string DefaultFolder { get; set; }
        public int ExtensionFilterIndex { get; set; }
        #region Dependency Properties
        public static readonly DependencyProperty FileProperty =
        DependencyProperty.Register(
            name: "File",
            propertyType: typeof(string),
            ownerType: typeof(FilePicker),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: ""));

        public static readonly DependencyProperty ExtensionsProperty =
       DependencyProperty.Register(
           name: "Extensions",
           propertyType: typeof(string),
           ownerType: typeof(FilePicker),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: "*.* | All Files"));

        public static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register(
        name: "Title",
        propertyType: typeof(string),
        ownerType: typeof(FilePicker),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: "Choose File"));

        public static readonly DependencyProperty HoverColorProperty =
    DependencyProperty.Register(
        name: "HoverColor",
        propertyType: typeof(System.Windows.Media.Brush),
        ownerType: typeof(FilePicker),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: System.Windows.Media.Brushes.Blue));

        public static readonly DependencyProperty DefaultColorProperty =
   DependencyProperty.Register(
       name: "DefaultColor",
       propertyType: typeof(System.Windows.Media.Brush),
       ownerType: typeof(FilePicker),
       typeMetadata: new FrameworkPropertyMetadata(defaultValue: System.Windows.Media.Brushes.Gray));

        public string File
        {
            get 
            { 
                return (string)GetValue(FileProperty); 
            } 
            set {
                SetValue(FileProperty, value);
            }
        }
        public string Extensions
        {
            get
            {
                return ((string)GetValue(ExtensionsProperty));
            }
            set
            {
                SetValue(ExtensionsProperty, value);
            }
        }
        public string Title
        {
            get
            {
                return (((string)GetValue(TitleProperty)));
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public System.Windows.Media.Brush HoverColor
        {
            get
            {
                return (System.Windows.Media.Brush)GetValue(HoverColorProperty);
            }
            set
            {
                SetValue(HoverColorProperty, value);
            }
        }
        public System.Windows.Media.Brush DefaultColor
        {
            get
            {
                return (System.Windows.Media.Brush)GetValue(DefaultColorProperty);
            }
            set
            {
                SetValue(DefaultColorProperty, value);
            }
        }
        #endregion

        #region Events
        public static readonly RoutedEvent FileAcceptedEvent =
       EventManager.RegisterRoutedEvent("FileAccepted", RoutingStrategy.Bubble,
       typeof(RoutedEventHandler), typeof(FilePicker));

        public event RoutedEventHandler FileAccepted
        {
            add { AddHandler(FileAcceptedEvent, value); }
            remove { RemoveHandler(FileAcceptedEvent, value); }
        }
        public static readonly RoutedEvent FileRejectedEvent =
EventManager.RegisterRoutedEvent("FileRejected", RoutingStrategy.Bubble,
typeof(RoutedEventHandler), typeof(FilePicker));

        public event RoutedEventHandler FileRejected
        {
            add { AddHandler(FileRejectedEvent, value); }
            remove { RemoveHandler(FileRejectedEvent, value); }
        }

        #endregion
        public FilePicker()
        {
            InitializeComponent();
            this.DataContext = this;
            BORDER_PART.BorderBrush = DefaultColor;
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = this.Title;
            dialog.Filter = this.Extensions;
            dialog.RestoreDirectory = true;
            dialog.DefaultExt = ".txt";
            dialog.InitialDirectory = DefaultFolder;

            if (dialog.ShowDialog() == true)
            {
                this.ExtensionFilterIndex = dialog.FilterIndex;

                this.File = dialog.FileName;
                RoutedEventArgs args = new RoutedEventArgs(FilePicker.FileAcceptedEvent);
                RaiseEvent(args);
            }
           
        }
        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            BORDER_PART.BorderBrush = HoverColor;
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            BORDER_PART.BorderBrush = DefaultColor;
        }
    }
}
