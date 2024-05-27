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
using com.KonquestUI.Controls;


namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            var source = (FancyNavigationButton)e.OriginalSource;
            if (source == null) return;
            var children = ((StackPanel)sender).Children;
            if(children.Count == 0) return;
            foreach( var child in children )
            {
                if(child.GetType() == typeof(FancyNavigationButton) )
                {
                    var navButton = (FancyNavigationButton)child;
                    if(navButton.IsActive)
                        navButton.IsActive = false;
                }
            }

            source.IsActive = !source.IsActive;
            SettingsNavigationView.Navigate(source.NavigationSource);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void FancyButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Save();
            this.Close();
        }
    }
}
