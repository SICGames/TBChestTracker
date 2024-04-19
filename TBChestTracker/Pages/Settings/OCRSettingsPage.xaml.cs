using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TBChestTracker.Managers;
using TBChestTracker.UI;

namespace TBChestTracker.Pages.Settings
{
    /// <summary>
    /// Interaction logic for OCRSettingsPage.xaml
    /// </summary>
    public partial class OCRSettingsPage : Page
    {
        public OCRSettingsPage()
        {
            InitializeComponent();
            this.DataContext = SettingsManager.Instance.Settings;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
                       
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Settings.OCRSettings.CaptureMethod = CaptureMethodBox.Text;
        }

        private void FancyNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness = ((FancyNumericValue)sender).Value;
        }
    }
}
