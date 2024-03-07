using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Milestone.xaml
    /// </summary>
    public partial class Milestone : UserControl
    {
        public Milestone()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public static readonly DependencyProperty ProgressHeightProperty = DependencyProperty.Register("ProgressHeight", typeof(Int32), typeof(Milestone), new PropertyMetadata(10));
        public static readonly DependencyProperty ProgressFillProperty = DependencyProperty.Register("ProgressFill", typeof(SolidColorBrush), 
            typeof(Milestone), new PropertyMetadata(new SolidColorBrush(Colors.SkyBlue)));

        public static readonly DependencyProperty ProgressBackFillProperty = DependencyProperty.Register("ProgressBackFill", typeof(SolidColorBrush),
            typeof(Milestone), new PropertyMetadata(new SolidColorBrush(Colors.SkyBlue)));

        public static readonly DependencyProperty MinStepProperty = DependencyProperty.Register("MinStep", typeof(Int32), typeof(Milestone), new PropertyMetadata(0));
        public static readonly DependencyProperty MaxStepProperty = DependencyProperty.Register("MaxStep", typeof(Int32), typeof(Milestone), new PropertyMetadata(5));
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(Int32), typeof(Milestone), new PropertyMetadata(0));
        public static readonly DependencyProperty MarkerSizeProperty = DependencyProperty.Register("MarkerSize", typeof(Int32), typeof(Milestone), new PropertyMetadata(24));

        public static readonly DependencyProperty StepsProperty = DependencyProperty.Register("Steps", typeof(ObservableCollection<String>), typeof(Milestone), 
            new PropertyMetadata(new ObservableCollection<String>()));

        public ObservableCollection<String> Steps
        {
            get
            {
                return (ObservableCollection<String>)GetValue(StepsProperty);
            }
            set
            {
                SetValue(StepsProperty, value);
            }
        }
        public Int32 MinStep
        {
            get
            {
                return (Int32)GetValue(MinStepProperty);
            }
            set { SetValue(MinStepProperty, value);}
        }
        public Int32 MaxStep
        {
            get
            {
                return (Int32)GetValue(MaxStepProperty);
            }
            set { SetValue(MaxStepProperty, value);}
        }
        public Int32 Step
        {
            get
            {
                return (Int32)GetValue(StepProperty);
            }
            set { SetValue(StepProperty, value);}
        }

        public Int32 MarkerSize
        {
            get
            {
                return (Int32)GetValue(MarkerSizeProperty);
            }
            set
            {
                SetValue(MarkerSizeProperty, value);
            }
        }

        public SolidColorBrush ProgressFill
        {
            get
            {
                return (SolidColorBrush)GetValue(ProgressFillProperty);
            }
            set
            {
                SetValue(ProgressFillProperty, value);
            }
        }
        public SolidColorBrush ProgressBackFill
        {
            get
            {
                return (SolidColorBrush)GetValue(ProgressBackFillProperty);
            }
            set
            {
                SetValue(ProgressBackFillProperty, value);
            }
        }

        public Int32 ProgressHeight
        {
            get
            {
                return (Int32)GetValue(ProgressHeightProperty);
            }
            set
            {
                SetValue(ProgressHeightProperty, value);
            }
        }

        private void InitSteps()
        {
            //-- init markers
            var x = 0;
            do
            {
                Ellipse el = new Ellipse();
                el.Width = MarkerSize;
                el.Height = MarkerSize;
                if (x <= Step)
                    el.Fill = ProgressFill;
                else
                    el.Fill = ProgressBackFill;

                el.StrokeThickness = 0;

                double max = (double)MaxStep;
                double markerHalf = (double)MarkerSize / 2;

                double stepProgress = (double)x / max;

                var xPos = this.ActualWidth * (stepProgress) - markerHalf;
                var yPos = this.ActualHeight / 2 - markerHalf;

                if (x == 0)
                    Canvas.SetLeft(el, -2);
                else if (x == MaxStep)
                {
                    xPos = this.ActualWidth - (markerHalf * 1.5);
                    Canvas.SetLeft(el, xPos);
                }
                else
                    Canvas.SetLeft(el, xPos);

                Canvas.SetTop(el, yPos);
                PARENT.Children.Add(el);

                //-- text under the markers

                TextBlock t = new TextBlock();
                t.Text = Steps[x];
                t.Foreground = new SolidColorBrush(Colors.Black);
                t.FontWeight = FontWeights.Bold;
                t.Width = 150;
                t.TextWrapping = TextWrapping.Wrap;

                PARENT.Children.Add(t);

                var tPos = this.ActualWidth * (stepProgress) - (markerHalf * 2) + 1;
                Canvas.SetLeft(t, tPos);
                Canvas.SetTop(t, yPos + (t.FontSize * 2));
                x++;

            } while (x <= MaxStep);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitSteps();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
