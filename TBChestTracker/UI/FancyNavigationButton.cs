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
    ///     <MyNamespace:FancyNavigationButton/>
    ///
    /// </summary>
    public class FancyNavigationButton : Control
    {
        static FancyNavigationButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyNavigationButton), new FrameworkPropertyMetadata(typeof(FancyNavigationButton)));
        }
        public static readonly DependencyProperty isActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(FancyNavigationButton), new PropertyMetadata(false));
        public static readonly DependencyProperty ActiveBrushProperty = DependencyProperty.Register("ActiveBrush", 
            typeof(Brush), typeof(FancyNavigationButton), new PropertyMetadata(Brushes.LightGreen));

        public bool IsActive
        {
            get => (Boolean)GetValue(isActiveProperty);
            set => SetValue(isActiveProperty, value);
        }
        public Brush ActiveBrush
        {
            get => (Brush)GetValue(ActiveBrushProperty);
            set => SetValue(ActiveBrushProperty, value);
        }

    }
}
