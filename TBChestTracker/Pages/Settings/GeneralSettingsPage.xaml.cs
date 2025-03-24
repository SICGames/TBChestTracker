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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBChestTracker.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using System.IO;
using System.Globalization;

using com.HellStormGames.Imaging;
using com.HellStormGames.Imaging.ScreenCapture;
using com.HellStormGames.Imaging.Extensions;
using TBChestTracker.Extensions;
namespace TBChestTracker.Pages
{
    /// <summary>
    /// Interaction logic for GeneralSettingsPage.xaml
    /// </summary>
    public partial class GeneralSettingsPage : Page
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
        }
        private string oldClanRootFolder = String.Empty;
        private DisplayMonitor DisplayMonitor;
        private int oldMonitorSelection = 0;
        private int newMonitorSelection = 0;
        private bool MonitorSelectionChanged = false;

        private void FancyPicker_Click(object sender, RoutedEventArgs e)
        {
         
        }
      
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayMonitor = new DisplayMonitor();
            var monitors = DisplayMonitor.GetMonitors();
            foreach (var monitor in monitors)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                var monitorname = $"{monitor.MonitorName} ({monitor.ScreenBounds.Width} x {monitor.ScreenBounds.Height})";
                cbi.Content = monitorname;
                cbi.Tag = monitor.MonitorName;
                MonitorSelectionBox.Items.Add(cbi);
            }
            if(SettingsManager.Instance.Settings.GeneralSettings.MonitorIndex > monitors.Count() - 1)
            {
                SettingsManager.Instance.Settings.GeneralSettings.MonitorIndex = 0; //-- revert back to primary monitor.
            }

            monitors.Dispose();
            DisplayMonitor.Dispose();
            
            SettingsWindow settingsWindow = Window.GetWindow(this) as SettingsWindow;
            MonitorPreview.Source = settingsWindow.MonitorPreviewImage;
            oldMonitorSelection = SettingsManager.Instance.Settings.GeneralSettings.MonitorIndex;
            this.DataContext = SettingsManager.Instance.Settings.GeneralSettings;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void UILanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
      
        }
        private void ClanRootFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dialog.FileName;
                var picker = (FancyPicker)sender;
                oldClanRootFolder = picker.Source;
                var newClanRootFolder = dialog.FileName;

                //-- warn user and if they're okay with it, try to move everything to new clan folder.
                var result = MessageBox.Show("Are you sure you want to move all clan databases to new root folder?", "New Clan Root Folder", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    //-- C:\\
                    var newFolderLength = folder.Length;
                    bool bIsHardDriveLetterOnly = false;
                    if (newFolderLength < 4)
                    {
                        bIsHardDriveLetterOnly = true;
                    }
                    //-- fail safe just incase someone wants to get silly.
                    if (bIsHardDriveLetterOnly)
                    {
                        var forcedDirectory = $"{folder}TotalBattleChestTracker";
                        Directory.CreateDirectory(forcedDirectory);
                        newClanRootFolder = forcedDirectory;
                    }

                    MoveClanFolderWindow moveClanFolderWindow = new MoveClanFolderWindow(oldClanRootFolder, newClanRootFolder);
                    if (moveClanFolderWindow.ShowDialog() == true)
                    {
                        SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder = newClanRootFolder;
                        SettingsManager.Instance.Save();
                        //-- reload clan 
                    }
                }
            }
        }
        bool firsttimecall = true;
        private void MonitorSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WarningBoxMonitorChange != null)
            {
                var selectedIndex = ((ComboBox)e.Source).SelectedIndex;
                if (selectedIndex != oldMonitorSelection)
                {
                    WarningBoxMonitorChange.Visibility = Visibility.Visible;
                    MonitorSelectionChanged = true;
                }
                else
                {
                    WarningBoxMonitorChange.Visibility = Visibility.Collapsed;
                }
                if (firsttimecall != true)
                {
                    //-- destroy snapster 
                    Snapster.Release();
                    //-- create new configuration
                    Snapster.Capturer = new SnapsterConfiguration().CapturerContext.WindowsGDI(MonitorConfiguration.ByIndex(selectedIndex)).CreateCapturer();
                    using (var image = Snapster.CaptureDesktop())
                    {
                        MonitorPreview.Source = image.ToBitmap().AsBitmapSource();
                    }
                }
                if(firsttimecall)
                {
                    firsttimecall = false;
                }
            }
        }
    }
}
