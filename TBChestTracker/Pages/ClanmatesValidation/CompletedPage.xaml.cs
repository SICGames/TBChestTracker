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

namespace TBChestTracker.Pages.ClanmatesValidation
{
    /// <summary>
    /// Interaction logic for CompletedPage.xaml
    /// </summary>
    public partial class CompletedPage : Page
    {
        public CompletedPage()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //--- save clanmates and clanchest data.
            ClanManager.Instance.ClanmateManager.Save();
            ClanManager.Instance.ClanChestManager.Save(); //SaveData();
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            wnd.Close();
        }
    }
}
