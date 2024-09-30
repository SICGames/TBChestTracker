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
    /// Interaction logic for RepairingClanChestData.xaml
    /// </summary>
    public partial class RepairingClanChestData : Page
    {
        public RepairingClanChestData()
        {
            InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task PerformClanChestDataRepair()
        {
            ClanManager.Instance.ClanChestManager.RepairChestData();
            await Task.Delay(3000);
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var wnd = Window.GetWindow(this) as ValidateClanChestsIntegrityWindow;
                wnd.NavigateTo("Pages/ChestDataIntegrity/RepairsCompleted.xaml");
            }));
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => PerformClanChestDataRepair());   
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
