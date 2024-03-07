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
    public class FancyButton : ButtonBase
    {
        static FancyButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyButton), new FrameworkPropertyMetadata(typeof(FancyButton)));
        }
        #region Dependancy Properties

        public static readonly DependencyProperty HilightBrushProperty = DependencyProperty.Register("HilightBrush",  typeof(Brush), typeof(FancyButton), null);
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(FancyButton), null);
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(FancyButton), null);

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
    }
}
