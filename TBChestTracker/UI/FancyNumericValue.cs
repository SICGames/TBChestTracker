using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    ///     <MyNamespace:FancyNumericValue/>
    ///
    /// </summary>

    [TemplatePart(Name = "CONTENT_MAIN",  Type = typeof(TextBox))]
    [TemplatePart(Name ="NUMERIC_UP",  Type = typeof(Button))]
    [TemplatePart(Name = "NUMERIC_DOWN", Type =typeof(Button))]
    public class FancyNumericValue : Control
    {
        private TextBox textbox;
        private Button numericIncreaseButton, numericDecreaseButton;
        
        public static readonly DependencyProperty NumericValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(FancyNumericValue), new PropertyMetadata(0.0, NumericValueChanged));
        public static readonly DependencyProperty MinNumericValueProperty = DependencyProperty.Register("Minimal", typeof(double), typeof(FancyNumericValue), new PropertyMetadata(0.0));
        public static readonly DependencyProperty MaxNumericValueProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(FancyNumericValue), new PropertyMetadata(double.MaxValue));
        public static readonly DependencyProperty StepNumericValueProperty = DependencyProperty.Register("Steps", typeof(double), typeof(FancyNumericValue), new PropertyMetadata(1.0));
        public static readonly DependencyProperty HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(FancyNumericValue), 
            new PropertyMetadata(new SolidColorBrush(Colors.SkyBlue)));
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(FancyNumericValue),
            new PropertyMetadata(new CornerRadius(0, 0, 0, 0)));

        public static readonly RoutedEvent NumericValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyNumericValue));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(NumericValueChangedEvent, value); }
            remove { RemoveHandler(NumericValueChangedEvent, value); }
        }
        private void RaiseValueChangedEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(FancyNumericValue.NumericValueChangedEvent);
            RaiseEvent(args);
        }

        public double Value
        {
            get => (double)GetValue(NumericValueProperty);
            set => SetValue(NumericValueProperty, value);
        }
        public double Minimal
        {
            get => (double)GetValue(MinNumericValueProperty);
            set => SetValue(MinNumericValueProperty, value);
        }
        public double Maximum
        {
            get => (double)GetValue(MaxNumericValueProperty);
            set => SetValue(MaxNumericValueProperty, value);
        }
        public double Steps
        {
            get => (double)GetValue(StepNumericValueProperty);
            set => SetValue(StepNumericValueProperty, value);
        }
        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue (HoverBrushProperty, value);
        }
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static void NumericValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericbutton = (FancyNumericValue)d;
            if (numericbutton != null)
                numericbutton.onNumericValueChanged(numericbutton, (double)e.NewValue);

            
        }
        public void onNumericValueChanged(object sender, double value)
        {
            if(sender is FancyNumericValue numericvalue)
            {
                if (numericvalue.textbox != null)
                {
                    numericvalue.textbox.Text = $"{value.ToString()}";
                    RaiseValueChangedEvent();
                }
            }
        }

        static FancyNumericValue()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FancyNumericValue), new FrameworkPropertyMetadata(typeof(FancyNumericValue)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            textbox = (TextBox)base.Template.FindName("CONTENT_MAIN", this);
            textbox.TextChanged += Textbox_TextChanged;
            numericIncreaseButton = (Button)base.Template.FindName("NUMERIC_UP", this);
            numericDecreaseButton = (Button)base.Template.FindName("NUMERIC_DOWN", this);

            if (numericIncreaseButton != null && numericDecreaseButton != null)
            {
                numericIncreaseButton.Click += NumericIncreaseButton_Click;
                numericDecreaseButton.Click += NumericDecreaseButton_Click;
            }
            if (textbox != null)
            {
                textbox.Text = $"{this.Value.ToString()}";
            }
        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            double d;
            if (Double.TryParse(tb.Text, out d))
                Value = d;

        }

        private void NumericDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.Value > Minimal)
                this.Value -= Steps;
            else 
                this.Value = Minimal;
            
        }

        private void NumericIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.Value < Maximum)
                this.Value += Steps;
            else 
                this.Value = Maximum;
        }
    }
}
