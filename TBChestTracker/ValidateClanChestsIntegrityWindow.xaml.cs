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
using System.Windows.Shapes;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ValidateClanChestsIntegrityWindow.xaml
    /// </summary>
    public partial class ValidateClanChestsIntegrityWindow : Window
    {
        public IntegrityResult IntegrityResult { get; set; }
        public ValidateClanChestsIntegrityWindow()
        {
            InitializeComponent();
        }

        public void NavigateTo(string page)
        {
            ValidateClanChestIntegrityFrame.Navigate(new Uri(Uri.UnescapeDataString(page.ToString()), UriKind.RelativeOrAbsolute));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IntegrityResult != null)
            {
                IntegrityResult.Dispose();
            }
        }
    }
}
