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
using Newtonsoft;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for NewClanDatabaseWindow.xaml
    /// </summary>
    public partial class NewClanDatabaseWindow : Window
    {

        public NewClanDatabaseWindow()
        {
            InitializeComponent();
            this.DataContext = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
        }

        private void CreateClanDatabaseBtn_Click(object sender, RoutedEventArgs e)
        {
            ClanManager.Instance.ClanDatabaseManager.Create(result =>
            {
                if (result)
                {
                    this.DialogResult = true;
                    ClanManager.Instance.ClanDatabaseManager.Save();
                    this.Close();
                }
                
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
