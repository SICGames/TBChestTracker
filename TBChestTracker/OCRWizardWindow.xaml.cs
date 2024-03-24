using System;
using System.Collections.Generic;
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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OCRWizardWindow.xaml
    /// </summary>
    public partial class OCRWizardWindow : Window
    {
        private bool isCompleted = false;

        OCRWizardScreenWindow ocrWizardScreenWindow = null; 
        public OCRWizardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ocrWizardScreenWindow = new OCRWizardScreenWindow();
            ocrWizardScreenWindow.ocrWindow = this;
            OCRWizardGuideViewer.LoadCompleted += OCRWizardGuideViewer_LoadCompleted;
        }

        private void OCRWizardGuideViewer_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var source = e.Uri.OriginalString;
            Debug.WriteLine($"OCRWizard -- {source} loaded");
            var uri = source.Substring(source.LastIndexOf("/") + 1);
            uri = uri.Substring(0,uri.IndexOf("."));
            Debug.WriteLine($"OCRWizard -- {uri} loaded");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ocrWizardScreenWindow?.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void NavigateTo(string uri)
        {
            OCRWizardGuideViewer.Source = new Uri(Uri.UnescapeDataString(uri.ToString()), UriKind.RelativeOrAbsolute);
        }
        public void OCRWizardSuccesful()
        {
            this.NavigateTo("Pages/OCRWizard/OCRWizard_Successful.xaml");
            BeginButton.Visibility = Visibility.Collapsed;
            DoneButton.Visibility = Visibility.Visible;
            ManualEditButton.Visibility = Visibility.Collapsed;
        }
        public void OCRWizardFailed()
        {
            this.NavigateTo("Pages/OCRWizard/OCRWizard_Failed.xaml");

            BeginButton.Visibility = Visibility.Collapsed;
            DoneButton.Visibility = Visibility.Collapsed;
            ManualEditButton.Visibility = Visibility.Visible;
        }
        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            if (ocrWizardScreenWindow.ShowDialog() == true)
            {
                isCompleted = true;
                this.Close();
            }
            else
            {
                this.Show();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo("Pages/OCRWizard/OCRWizard_GiftsTab.xaml");
            
            BeginButton.Visibility = Visibility.Visible;
            NextButton.Visibility = Visibility.Collapsed;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ManualEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
