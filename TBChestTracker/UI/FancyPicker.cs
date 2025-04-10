using System;
using System.Collections.Generic;
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

    public class FancyPicker : ButtonBase
    {
        private TextBox pTextBox;
        public static readonly DependencyProperty HoverBrushProperty;
        public static readonly DependencyProperty FileProperty;
        public static readonly DependencyProperty FiltersProperty;
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty CornersProperty;
        public static readonly DependencyProperty SourcePathProperty;

        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }
        public CornerRadius Corners
        {
            get => (CornerRadius)GetValue(CornersProperty); set => SetValue(CornersProperty, value);
        }
        public string Source
        {
            get => (String)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }
     
        public string Filters
        {
            get
            {
                return ((string)GetValue(FiltersProperty));
            }
            set
            {
                SetValue(FiltersProperty, value);
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

        #region Events
        public static readonly RoutedEvent AcceptedEvent;
        public event RoutedEventHandler Accepted
        {
            add { AddHandler(AcceptedEvent, value); }
            remove { RemoveHandler(AcceptedEvent, value); }
        }

        public static readonly RoutedEvent RejectedEvent;
        public event RoutedEventHandler Rejected
        {
            add { AddHandler(RejectedEvent, value); }
            remove { RemoveHandler(RejectedEvent, value); }
        }

        #endregion

        private static void SourcePathChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var o = (FancyPicker)dp;
            o.onSourcePathChanged(dp, e);
        }
        public void onSourcePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var o = (FancyPicker)dependencyObject;
            if (pTextBox == null) 
                return;
            else 
                pTextBox.Text = (string)e.NewValue;

        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            pTextBox = base.Template.FindName("TEXT_FIELD", this) as TextBox;
            pTextBox.Text = this.Source;
            pTextBox.PreviewMouseLeftButtonDown += PTextBox_PreviewMouseLeftButtonDown;
        }

        private void PTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnClick();
        }

        static FancyPicker()
        {
            HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancyPicker),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));

            AcceptedEvent = EventManager.RegisterRoutedEvent("Accepted", RoutingStrategy.Bubble,
                                typeof(RoutedEventHandler), typeof(FancyPicker));

            RejectedEvent = EventManager.RegisterRoutedEvent("Rejected", RoutingStrategy.Bubble,
                                typeof(RoutedEventHandler), typeof(FancyPicker));

            CornersProperty = DependencyProperty.Register("Corners", typeof(CornerRadius), typeof(FancyPicker),
           new PropertyMetadata(new CornerRadius(0, 0, 0, 0)));

            SourcePathProperty = DependencyProperty.Register("Source", typeof(String), typeof(FancyPicker),
            new PropertyMetadata(String.Empty, SourcePathChanged));

            TitleProperty = DependencyProperty.Register(
                            name: "Title",
                            propertyType: typeof(string),
                            ownerType: typeof(FancyPicker),
                            typeMetadata: new FrameworkPropertyMetadata(defaultValue: "Choose File"));

            FiltersProperty = DependencyProperty.Register(
                                name: "Filters",
                                propertyType: typeof(string),
                                ownerType: typeof(FilePicker),
                                typeMetadata: new FrameworkPropertyMetadata(defaultValue: "*.* | All Files"));


            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyPicker), new FrameworkPropertyMetadata(typeof(FancyPicker)));
        }
    }
}
