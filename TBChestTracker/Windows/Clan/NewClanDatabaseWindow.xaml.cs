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
                    try
                    {
                        ClanManager.Instance.AddOcrProfile("Default", null);
                        ClanManager.Instance.SetCurrentOcrProfile("Default");
                    }
                    catch(Exception ex)
                    {
                        if(ex is ArgumentException)
                        {
                            if (MessageBox.Show("There's already an existing OCR Profile for the created clan. This is catching the exception that is being tossed. You may need to re-run Ocr Studio to ensure everything is correct.") == MessageBoxResult.OK)
                            {
                                ClanManager.Instance.SetCurrentOcrProfile("Default");
                            }
                        }
                    }
                    ClanManager.Instance.ClanDatabaseManager.Save();
                    AppContext.Instance.IsCurrentClandatabase = true;
                    AppContext.Instance.NewClandatabaseBeenCreated = true;
                    AppContext.Instance.OCRCompleted = false;
                    AppContext.Instance.RequiresOCRWizard = true;
                    AppContext.Instance.IsAutomationPlayButtonEnabled = false;
                    this.Close();
                }
                
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
