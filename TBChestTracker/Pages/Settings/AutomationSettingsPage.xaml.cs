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

namespace TBChestTracker.Pages.Settings
{
    /// <summary>
    /// Interaction logic for AutomationSettingsPage.xaml
    /// </summary>
    public partial class AutomationSettingsPage : Page
    {
        private bool bIsEditing = false;

        public AutomationSettingsPage()
        {
            InitializeComponent();
            this.DataContext = SettingsManager.Instance.Settings.AutomationSettings;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (bIsEditing)
            {
                TextBox tb = (TextBox)sender;
                
                if (!String.IsNullOrEmpty(tb.Text) && tb.Text.IsNumber())
                {
                    if (tb.Tag.ToString() == "ScreenshotDelay")
                    {
                        var t = tb.Text;
                        SettingsManager.Instance.Settings.AutomationSettings.AutomationScreenshotsAfterClicks = Int32.Parse(t);
                    }
                    if (tb.Tag.ToString() == "Clicks")
                    {
                        var t = tb.Text;
                        SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks = Int32.Parse(t);
                    }
                    if (tb.Tag.ToString() == "ClicksDelay")
                    {
                        var t = tb.Text;
                        SettingsManager.Instance.Settings.AutomationSettings.AutomationDelayBetweenClicks = Int32.Parse(t);
                    }
                }
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(!bIsEditing)
            {
                bIsEditing = true;
            }
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (bIsEditing)
            {
                bIsEditing = false;
            }
        }
    }
}
