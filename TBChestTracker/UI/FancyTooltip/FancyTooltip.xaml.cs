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
using WpfAnimatedGif;

namespace TBChestTracker.UI
{
    /// <summary>
    /// Interaction logic for FancyTooltip.xaml
    /// </summary>
    public partial class FancyTooltip : UserControl
    {
        public static readonly DependencyProperty TooltipImageSourceProperty = null;
        public static readonly DependencyProperty TooltipAnimatedSourceProperty = null;
        public static readonly DependencyProperty TooltipTitleProperty = null;
        public static readonly DependencyProperty TooltipDescriptionProperty = null;
        
        public ImageSource Source
        {
            get => (ImageSource)GetValue(TooltipImageSourceProperty);
            set => SetValue(TooltipImageSourceProperty, value);
        }
        
        public ImageSource AnimatedSource
        {
            get => (ImageSource)GetValue(TooltipAnimatedSourceProperty);
            set => SetValue(TooltipAnimatedSourceProperty, value);
        }

        public String Title
        {
            get => (String)GetValue(TooltipTitleProperty);
            set => SetValue(TooltipTitleProperty, value);
        }
        public String Description
        {
            get => (String)GetValue(TooltipDescriptionProperty); 
            set => SetValue(TooltipDescriptionProperty, value);
        }

        static FancyTooltip()
        {
            TooltipImageSourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(FancyTooltip), new PropertyMetadata(null));
            TooltipAnimatedSourceProperty = DependencyProperty.Register("AnimatedSource", typeof(ImageSource), typeof(FancyTooltip), new PropertyMetadata(null));
            TooltipTitleProperty = DependencyProperty.Register("Title", typeof(String), typeof(FancyTooltip), new PropertyMetadata("[Title]"));
            TooltipDescriptionProperty = DependencyProperty.Register("Description", typeof(String), typeof(FancyTooltip), new PropertyMetadata("Description"));
        }
        public FancyTooltip()
        {
            InitializeComponent();
        }
    }
}
