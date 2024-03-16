using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBChestTracker.UI
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TBChestTracker.UI"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TBChestTracker.UI;assembly=TBChestTracker.UI"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:FancyButton/>
    ///
    /// </summary>
    /// 
    public class FancyButton : ButtonBase, INotifyPropertyChanged
    {

        //--- ImageSource reference
        private BitmapSource PreviousBitmapSource {  get; set; }

        #region Static FancyButton()
        static FancyButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyButton), new FrameworkPropertyMetadata(typeof(FancyButton)));
            IsEnabledProperty.AddOwner(typeof(FancyButton), new FrameworkPropertyMetadata(IsEnabledPropertyChanged));
        }

        private static void IsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fb = (FancyButton)d;
            if(fb != null)
            {
                fb.OnEnabledPropertyChanged(fb, e);
            }
        }
        public void OnEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fb = (FancyButton)sender;
            var v = (bool)e.NewValue;
            var image = fb.ImageSource;
            if (image != null)
            {
                if (v)
                {
                    fb.ImageSource = image;
                }
                else
                {
                    fb.ImageSource = ConvertToGreyscale(image);
                }
            }
        }
        #endregion
        #region RenderModeEnum
        public enum RenderModeEnum
        {
            IMAGE_TEXT,
            IMAGE,
            TEXT
        }
        #endregion

        #region Dependancy Properties

        public static readonly DependencyProperty HilightBrushProperty = DependencyProperty.Register("HilightBrush",  typeof(Brush), typeof(FancyButton), 
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.SkyBlue)));
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(FancyButton), null);
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(FancyButton), 
            new PropertyMetadata(""));
        public static readonly DependencyProperty CornersProperty = DependencyProperty.Register("Corners", typeof(CornerRadius), typeof(FancyButton), 
            new FrameworkPropertyMetadata(new CornerRadius(0,0,0,0)));
        public static readonly DependencyProperty RenderModeProperty = DependencyProperty.Register("RenderMode", typeof(RenderModeEnum), typeof(FancyButton), 
            new PropertyMetadata(RenderModeEnum.IMAGE_TEXT, OnRenderModeChangedCallBack));

        public static readonly DependencyProperty ShowSeperatorProperty = DependencyProperty.Register("ShowSeperator", 
            typeof(bool), typeof(FancyButton), new FrameworkPropertyMetadata(false));

        public bool ShowSeperator
        {
            get
            {
                return (bool)GetValue(ShowSeperatorProperty);
            }
            set
            {
                SetValue(ShowSeperatorProperty, value);
            }
        }
        public RenderModeEnum RenderMode
        {
            get
            {
                return (RenderModeEnum)GetValue(RenderModeProperty);    
            }
            set
            {
                SetValue(RenderModeProperty, value);    
            }
        }
        public CornerRadius Corners
        {
            get
            {
                return (CornerRadius)GetValue(CornersProperty);
            }
            set { SetValue(CornersProperty, value); }
        }
        public Brush HilightBrush
        {
            get
            {
                return GetValue(HilightBrushProperty) as Brush;
            }
            set
            {
                SetValue(HilightBrushProperty, value);
            }
        }
        
        public BitmapSource ImageSource
        {
            get
            {
                return (BitmapSource)GetValue(ImageSourceProperty);
            }
            set
            {
                SetValue(ImageSourceProperty, value);
                PreviousBitmapSource = value;
            }
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        #endregion


        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region PropertyChangedEvents 
        private static void OnRenderModeChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FancyButton fb = sender as FancyButton;
            if (fb != null)
            {
                fb.OnRenderModeChanged(e);
            }
        }

        protected virtual void OnRenderModeChanged(DependencyPropertyChangedEventArgs e)
        {
            // Grab related data.
            // Raises INotifyPropertyChanged.PropertyChanged
           
            OnPropertyChanged(nameof(RenderModeProperty));
        }
        #endregion

        private BitmapSource ConvertToGreyscale(BitmapSource source)
        {
            var stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[stride * source.PixelHeight];

            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // this works for PixelFormats.Bgra32
                var blue = pixels[i];
                var green = pixels[i + 1];
                var red = pixels[i + 2];
                var gray = (byte)(0.5 * red + 0.5 * green + 0.5 * blue);
                pixels[i] = gray;
                pixels[i + 1] = gray;
                pixels[i + 2] = gray;
            }

            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                source.Format, null, pixels, stride);
        }

    }
}
