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

namespace TBChestTracker.Pages
{
    /// <summary>
    /// Interaction logic for ClanPage.xaml
    /// </summary>
    public partial class ClanPage : Page
    {
        public ClanWizardContext Context { get; set; }
        public ClanPage()
        {
            InitializeComponent();
            Context = ClanWizardContext.Instance;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Context.NavigateForward();
            ClanWizardWindow.FrameViewer.Navigate(new Uri(Context.ClanWizardStep.CurrentPageUri.ToString(), UriKind.RelativeOrAbsolute));
        }
    }
}
