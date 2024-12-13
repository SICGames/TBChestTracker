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
using TBChestTracker.Managers;

namespace TBChestTracker.Pages.ChestDataIntegrity
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
       
        public StartPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void FancyButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ValidateClanChestsIntegrityWindow;
            var result = ClanManager.Instance.ClanChestManager.CheckIntegrity(); // DoesChestDataNeedsRepairs();
            if(result == null)
            {
                wnd.NavigateTo("Pages/ChestDataIntegrity/NoRepairsNeeded.xaml");
            }
            else
            {
                wnd.IntegrityResult = result;
                wnd.NavigateTo("Pages/ChestDataIntegrity/ErrorsFound.xaml");
            }
        }
    }
}
