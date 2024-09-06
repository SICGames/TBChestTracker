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
    public class Notification : Button
    {
        private Ellipse alertDot { get; set; }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(Notification), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowNotificationProperty = DependencyProperty.Register("ShowNotification", typeof(bool), typeof(Notification), new FrameworkPropertyMetadata(false, ShowNotificationPropertyChanged));
        public static readonly DependencyProperty AlertColorProperty = DependencyProperty.Register("AlertColor", typeof(Brush), typeof(Notification), new PropertyMetadata(new SolidColorBrush(Colors.Red)));   
        
        public Brush AlertColor
        {
            get => (Brush)GetValue(AlertColorProperty);
            set => SetValue(AlertColorProperty, value);
        }
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty); 
            set => SetValue(SourceProperty, value);
        }
        public bool ShowNotification
        {
            get => (bool)GetValue(ShowNotificationProperty);
            set => SetValue(ShowNotificationProperty, value);
        }

        private static void ShowNotificationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var notification = (Notification)d;
            notification.UpdateAlertVisibility((bool)e.NewValue);
        }
        private void UpdateAlertVisibility(bool value)
        {
            if (base.Template == null)
                return;

            alertDot = (Ellipse)base.Template.FindName("ALERT", this);
            if(alertDot == null) return;

            alertDot.Visibility = value == true ? Visibility.Visible : Visibility.Hidden;

        }
        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Notification), new FrameworkPropertyMetadata(typeof(Notification)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            alertDot = (Ellipse)base.Template.FindName("ALERT", this);
            alertDot.Visibility = (bool)GetValue(ShowNotificationProperty) == true ? Visibility.Visible : Visibility.Hidden;  
        }
    }
}
