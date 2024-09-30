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

namespace TBChestTracker.Pages.ChestDataIntegrity
{
    /// <summary>
    /// Interaction logic for ErrorsFound.xaml
    /// </summary>
    public partial class ErrorsFound : Page
    {
        public ErrorsFound()
        {
            InitializeComponent();
        }

        private void RepairButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as ValidateClanChestsIntegrityWindow;
            wnd.NavigateTo("Pages/ChestDataIntegrity/RepairingClanChestData.xaml");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private async Task BuildErrorListView(IntegrityResult result)
        {
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var errorParent = ERROR_LIST as Grid;
                var rowCount = 0;
                foreach (var error in result.Errors)
                {
                    var rowDef = new RowDefinition();
                    errorParent.RowDefinitions.Add(rowDef);

                    var img = new Image();
                    img.Source = new BitmapImage(new Uri(@"Images/errorIcon.png", UriKind.Relative));
                    img.Width = 16;
                    img.Height = 16;
                    img.VerticalAlignment = VerticalAlignment.Center;
                    img.Margin = new Thickness(5, 5, 5, 5);

                    img.SetValue(Grid.RowProperty, rowCount);
                    img.SetValue(Grid.ColumnProperty, 0);

                    errorParent.Children.Add(img);

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = error.Key;
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.Margin = new Thickness(3, 0, 0, 0);
                    textBlock.SetValue(Grid.ColumnProperty, 1);
                    textBlock.SetValue(Grid.RowProperty, rowCount);

                    errorParent.Children.Add(textBlock);
                    rowCount++;
                }
            }));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //-- get the Integrity Result and display the user. 
            var wnd = Window.GetWindow(this) as ValidateClanChestsIntegrityWindow;
            var result = wnd.IntegrityResult;

            Task.Run(() => BuildErrorListView(result));
            
        }
    }
}
