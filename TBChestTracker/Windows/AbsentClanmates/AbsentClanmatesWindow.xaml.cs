using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for AbsentClanmatesWindow.xaml
    /// </summary>
    public partial class AbsentClanmatesWindow : Window
    {
        public AbsentClanmatesViewModel ViewModel { get; set; }
        
        public AbsentClanmatesWindow()
        {
            InitializeComponent();
            ViewModel = new AbsentClanmatesViewModel();
        }
        public void NavigateTo(string page)
        {
            Frame01.Navigate(new Uri(Uri.UnescapeDataString(page.ToString()), UriKind.RelativeOrAbsolute));
        }
        public void SetAbsentDuration(string duration)
        {
           this.ViewModel.AbsentDuration = duration;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            ViewModel.AbsentClanmateList.Clear();
            //this.DataContext = ViewModel;
            Frame01.LoadCompleted += Frame01_LoadCompleted;
            NavigateTo("Pages/AbsentClanmates/ProcessingPage.xaml");
            Debug.WriteLine("I AM LOADED!!!!!!");
        }

        private void Frame01_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.Dispose();
        }
        private void Frame01_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }
    }
}
