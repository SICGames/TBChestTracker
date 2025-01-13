using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Threading;
using com.HellStormGames;


namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {

        public DispatcherTimer ScrollTimer { get; set; }

        public ConsoleWindow()
        {
            InitializeComponent();
            //this.DataContext = com.HellStormGames.Logging.Console.Instance;
            
        }

        private void ConsoleWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(ConsoleView) > 0)
            {
                FrameworkElement border = (FrameworkElement)VisualTreeHelper.GetChild(ConsoleView, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((INotifyCollectionChanged)ConsoleView.ItemsSource).CollectionChanged += ConsoleWindow_CollectionChanged;
        }

        
    }
}
