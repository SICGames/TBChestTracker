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
using TBChestTracker.Pages.OCRWizard;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OCRWizardWindow.xaml
    /// </summary>
    public partial class OCRWizardWindow : Window
    {
        private bool isCompleted = false;

        OCRWizardManualEditorWindow OCRWizardManualEditorWindow;

        public OCRWizardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
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
            //-- before we close, let's save everthing
            var page = OCRWizardGuideViewer.Content;
            if(page.GetType() == typeof(OCRWizard_Successful))
            {
                SettingsManager.Instance.Save();
                var firstRunFile = $@"{AppContext.Instance.CommonAppFolder}.FIRSTRUN";
                if(System.IO.File.Exists(firstRunFile))
                {
                    System.IO.File.Delete(firstRunFile);    
                }
            }
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
            var page = OCRWizardGuideViewer.Content;
            OCRWizardManualEditorWindow = new OCRWizardManualEditorWindow();
            
            if (page.GetType() == typeof(OCRWizard_SelectRegion))
            {
                OCRWizardManualEditorWindow.OCRManualMode = OCRManualMode.REGION_SELECTION;
            }
            else if(page.GetType() == typeof(OCRWizard_CreateMarker))
            {
                OCRWizardManualEditorWindow.OCRManualMode = OCRManualMode.MARKER_PLACEMENT;
            }
            this.Hide();
            
            if (OCRWizardManualEditorWindow.ShowDialog() == true)
            {
                this.Show();
                //-- we need to move to next step
                var ocrMode = OCRWizardManualEditorWindow.OCRManualMode;
                if (ocrMode == OCRManualMode.REGION_SELECTION)
                {
                    NavigateTo("Pages/OCRWizard/OCRWizard_CreateMarker.xaml");
                }
                else if(ocrMode == OCRManualMode.MARKER_PLACEMENT)
                {
                    NavigateTo("Pages/OCRWizard/OCRWizard_Successful.xaml");
                    BeginButton.Visibility = Visibility.Hidden;
                    NextButton.Visibility = Visibility.Hidden;
                    DoneButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = OCRWizardGuideViewer.Content;

            if (uri.GetType() == typeof(OCRWizard_WelcomePage))
            {
                NavigateTo("Pages/OCRWizard/OCRWizard_GiftsTab.xaml");
            }
            else if(uri.GetType() == typeof(OCRWizard_GiftsTab))
            {
                NavigateTo("Pages/OCRWizard/OCRWizard_SelectRegion.xaml");
                NextButton.Visibility = Visibility.Collapsed;
                BeginButton.Visibility = Visibility.Visible;
            }
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
