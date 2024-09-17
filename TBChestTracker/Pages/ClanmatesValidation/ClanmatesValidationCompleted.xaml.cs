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
    /// Interaction logic for ClanmatesValidationCompleted.xaml
    /// </summary>
    public partial class ClanmatesValidationCompleted : Page
    {
        public ClanmatesValidationCompleted()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            var affectedClanmates = wnd.affectedClanmates;
            this.DataContext = wnd;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyFixButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            wnd.NavigateTo("Pages/ClanmatesValidation/RepairClanmatesPage.xaml");
        }
    }
}
