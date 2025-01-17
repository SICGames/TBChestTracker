using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TBChestTracker.Pages;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for ClanWizardWindow.xaml
    /// </summary>
    public partial class ClanWizardWindow : Window
    {
        public ObservableCollection<String> Steps { get; set; }
        public ClanWizardContext Context { get; set; }

        public static Frame FrameViewer { get; set; }
        public ClanWizardWindow()
        {
            InitializeComponent();
            Steps = new ObservableCollection<string>() { "Clan Name", "Clanmates", "Finish" };
            MilestoneBar.Steps = Steps;
            Context = new ClanWizardContext();
            Context.InitNavigationUris("/Pages/ClanPage.xaml", "/Pages/GeneralSettingsPage.xaml");
            this.DataContext = Context;

            FrameViewer = FRAME_VIEW;
        }

        private void Frame_ContentRendered(object sender, EventArgs e)
        {
            
        }
    }
}
