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

namespace TBChestTracker.UI
{
    
    public class FancySearchBox : TextBox
    {
        public static readonly DependencyProperty HoverBrushProperty;
        public static readonly DependencyProperty CornersProperty;
        public static readonly DependencyProperty PlaceholderTextProperty;

        private TextBox SearchBox;
        private TextBlock Placeholder;
        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }
        public CornerRadius Corners
        {
            get => (CornerRadius)GetValue(CornersProperty);
            set => SetValue(CornersProperty, value);
        }
        public String PlaceholderText
        {
            get => (String)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        static FancySearchBox()
        {
            HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancySearchBox), new PropertyMetadata(new SolidColorBrush(Colors.CornflowerBlue)));
            CornersProperty = DependencyProperty.Register("Corners", typeof(CornerRadius), typeof(FancySearchBox), new PropertyMetadata(new CornerRadius(0, 0, 0, 0)));
            PlaceholderTextProperty = DependencyProperty.Register("PlaceholderText", typeof(String), typeof(FancySearchBox), new PropertyMetadata(""));
            
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancySearchBox), new FrameworkPropertyMetadata(typeof(FancySearchBox)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SearchBox = base.Template.FindName("SEARCH_BOX", this) as TextBox;
            SearchBox.TextChanged += TextBox_TextChanged;
            Placeholder = base.Template.FindName("PLACEHOLDER", this) as TextBlock;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = e.Source as TextBox;
            this.Text = textbox.Text;
            if(String.IsNullOrEmpty(textbox?.Text))
            {
                if (Placeholder == null) return;
                Placeholder.Visibility = Visibility.Visible;
            }
            else
            {
                if (Placeholder == null) return;
                Placeholder.Visibility = Visibility.Hidden;
            }

        }
    }
}
