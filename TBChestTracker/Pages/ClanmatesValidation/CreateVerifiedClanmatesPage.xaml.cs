using com.KonquestUI.Controls;
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
    /// Interaction logic for CreateVerifiedClanmatesPage.xaml
    /// </summary>
    public partial class CreateVerifiedClanmatesPage : Page
    {

        public CreateVerifiedClanmatesPage()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ClanmateEditorWindow clanmateEditorWindow = new ClanmateEditorWindow();
            var window = Window.GetWindow(this) as ClanmateValidationWindow;
            
            window.WindowState = WindowState.Minimized;
            clanmateEditorWindow.ParentWindow = window; 
            clanmateEditorWindow.ShowDialog();

        }

        private void SimiliarityPercentageNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            var nv = (FancyNumericValue)sender;
            wnd.ClanmateSimilarity = nv.Value;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ClanmateValidationWindow;
            SimiliarityPercentageNumericValue.Value = wnd.ClanmateSimilarity;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
