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
    /// Interaction logic for AbsentClanmatesCleanerWindow.xaml
    /// </summary>
    public partial class AbsentClanmatesCleanerWindow : Window
    {
        public AbsentClanmatesCleanerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {

            AbsentClanmatesWindow absentClanmatesWindow = new AbsentClanmatesWindow();
            absentClanmatesWindow.SetAbsentDuration(AbsentDurationComboBox.Text);

            Debug.WriteLine(AbsentDurationComboBox.Text);

            if (absentClanmatesWindow.ShowDialog() == true)
            {
                this.Close();
            }
        }
    }
}
