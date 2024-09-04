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
    ///     <MyNamespace:FancyPicker/>
    ///
    /// </summary>
    public class FancyPicker : Button
    {

        private TextBox pTextBox;

        static FancyPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyPicker), new FrameworkPropertyMetadata(typeof(FancyPicker)));
        }
        public static readonly DependencyProperty HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancyPicker),
            new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));
        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }

        public static readonly DependencyProperty CornersProperty =  DependencyProperty.Register("Corners", typeof(CornerRadius), typeof(FancyPicker), 
            new PropertyMetadata(new CornerRadius(0,0,0,0)));
        public CornerRadius Corners
        {
            get => (CornerRadius)GetValue(CornersProperty); set => SetValue(CornersProperty, value);
        }
        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register("Source", typeof(String), typeof(FancyPicker), 
            new PropertyMetadata(String.Empty, SourcePathChanged));
    
        public string Source
        {
            get => (String)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }
        private static void SourcePathChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var o = (FancyPicker)dp;
            o.onSourcePathChanged(dp, e);
        }
        public void onSourcePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var o = (FancyPicker)dependencyObject;
            if (pTextBox == null) return;
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
    }
}
