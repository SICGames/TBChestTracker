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

namespace TBChestTracker.Pages.ClanmatesValidation
{
    /// <summary>
    /// Interaction logic for ClanmatesValidationStartPage.xaml
    /// </summary>
    public partial class ClanmatesValidationStartPage : Page
    {
        public ClanmatesValidationStartPage()
        {
            InitializeComponent();
        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            wnd.NavigateTo("Pages/ClanmatesValidation/CreateVerifiedClanmatesPage.xaml");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
