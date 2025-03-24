using com.KonquestUI.Controls;
using Emgu.CV.Mcc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBChestTracker.UI
{
    public class FancyToggleButton : ButtonBase
    {
       
        [Category("Appearance")]
        [TypeConverter(typeof(NullableBoolConverter))]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public static readonly DependencyProperty IsSelectedProperty;

        [Category("Appearance")]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public static readonly DependencyProperty SelectedBrushProperty;
        
        [Category("Appearance")]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public static readonly DependencyProperty HoverBrushProperty;
        
        [Category("Appearance")]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public static readonly DependencyProperty SourceProperty;

        protected bool IsGrayscaled => Source is FormatConvertedBitmap;

        public bool? IsSelected
        {
            get
            {
                return (bool?)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly RoutedEvent SelectedEvent;
        public static readonly RoutedEvent UnselectedEvent;

        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }
        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value);  }
            remove { RemoveHandler(UnselectedEvent, value); }
        }

        public Brush SelectedBrush
        {
            get => (Brush)GetValue(SelectedBrushProperty);
            set => SetValue(SelectedBrushProperty, value);
        }
        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        static FancyToggleButton()
        {
            SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyToggleButton));
            UnselectedEvent =  EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler),typeof(FancyToggleButton));

            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(FancyToggleButton), new PropertyMetadata(false, IsSelectedPropertyChanged));

            SelectedBrushProperty = DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(FancyToggleButton),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue))); 

            HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancyToggleButton),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));

            SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(FancyToggleButton),
            new PropertyMetadata(null));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyToggleButton), new FrameworkPropertyMetadata(typeof(FancyToggleButton)));
            //-- IsEnabledProperty not triggering IsEnabledPropertyChanged due to ToggleButton. Odd.
            IsEnabledProperty.OverrideMetadata(typeof(FancyToggleButton), new FrameworkPropertyMetadata(true, IsEnabledPropertyChanged));
        }

        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is FancyToggleButton f)
            {
                var state = (bool?)e.NewValue;
                if(state == true)
                {
                    f.OnSelected(new RoutedEventArgs(SelectedEvent));
                }
                else if(state == false)
                {
                    f.OnSelected(new RoutedEventArgs(UnselectedEvent));
                }
            }
        }
     
        protected virtual void OnSelected(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        protected virtual void OnUnselected(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        protected override void OnClick()
        {
            OnToggle();
            base.OnClick();
        }
        protected virtual void OnToggle()
        {
            IsSelected = !IsSelected;
        }

        public FancyToggleButton()
        {

        }

        private static void IsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FancyToggleButton s && s.IsEnabled == s.IsGrayscaled)
            {
                s.UpdateImageSource();
            }
        }
        protected void UpdateImageSource()
        {
            if (Source == null)
                return;

            if (IsEnabled)
            {
                if (IsGrayscaled)
                {
                    Source = ((FormatConvertedBitmap)Source).Source;
                    OpacityMask = null;
                }
            }
            else
            {
                if (!IsGrayscaled)
                {
                    if (Source is BitmapSource bitmapImage)
                    {
                        FormatConvertedBitmap fcb = new FormatConvertedBitmap();
                        fcb.BeginInit();
                        fcb.Source = bitmapImage;
                        fcb.DestinationFormat = PixelFormats.Gray32Float;
                        fcb.EndInit();
                        Source = fcb;
                        var alphaMask = new ImageBrush();
                        alphaMask.ImageSource = bitmapImage;
                        alphaMask.Stretch = Stretch.None;

                        OpacityMask = alphaMask;
                    }
                }
            }
        }

        
    }
}
