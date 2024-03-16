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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OCRWizardWindow.xaml
    /// </summary>
    public partial class OCRWizardWindow : Window
    {
        private bool isCompleted = false;

        public OCRWizardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            OCRWizardScreenWindow oCRWizardScreenWindow = new OCRWizardScreenWindow();

            if (oCRWizardScreenWindow.ShowDialog() == true)
            {
                isCompleted = true;
                this.Close();
            }
            else
            {
                this.Show();
            }
        }
    }
}
