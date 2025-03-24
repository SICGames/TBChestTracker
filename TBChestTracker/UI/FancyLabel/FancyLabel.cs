using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBChestTracker.UI
{
    public class FancyLabel : ButtonBase
    {
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty TextAlignProperty;
        public static readonly DependencyProperty SelectedBrushProperty;
        public static readonly DependencyProperty HoverBrushProperty;
        public static readonly DependencyProperty CornersProperty;

        public static readonly RoutedEvent SelectedEvent;
        public static readonly RoutedEvent UnselectedEvent;

        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }
        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value); }
            remove { RemoveHandler(UnselectedEvent, value); }
        }

        public String Text
        {
            get => (String)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public TextAlignment TextAlign
        {
            get => (TextAlignment)GetValue(TextAlignProperty);  
            set => SetValue(TextAlignProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public Brush SelectedBrush
        {
            get => (Brush)GetValue(SelectedBrushProperty); set => SetValue(SelectedBrushProperty, value);
        }
        public Brush HoverBrush
        {
            get => ( Brush)GetValue(HoverBrushProperty); set => SetValue(HoverBrushProperty, value);    
        }
        public CornerRadius Corners
        {
            get => (CornerRadius)GetValue(CornersProperty); 
            set => SetValue(CornersProperty, value);
        }
        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FancyLabel f)
            {
                var state = (bool?)e.NewValue;
                if (state == true)
                {
                    f.OnSelected(new RoutedEventArgs(SelectedEvent));
                }
                else if (state == false)
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

        static FancyLabel()
        {
            SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyLabel));
            UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyLabel));

            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(FancyLabel), new PropertyMetadata(false, IsSelectedPropertyChanged));

            SelectedBrushProperty = DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(FancyLabel),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));

            HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancyLabel),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));

            TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(FancyLabel), new PropertyMetadata(""));
            CornersProperty = DependencyProperty.Register("Corners", typeof(CornerRadius), typeof(FancyLabel), new PropertyMetadata(new CornerRadius(0, 0, 0, 0)));
            TextAlignProperty = DependencyProperty.Register("TextAlign", typeof(TextAlignment), typeof(FancyLabel), new PropertyMetadata(TextAlignment.Center));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyLabel), new FrameworkPropertyMetadata(typeof(FancyLabel)));
        }
    }
}
