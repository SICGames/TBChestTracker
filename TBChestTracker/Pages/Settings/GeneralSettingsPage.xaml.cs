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

        private void FancyPicker_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private static List<UIElement> ToList(UIElementCollection collection)
        {
            List<UIElement> elements = new List<UIElement>();
            foreach(UIElement element in collection)
            {
                elements.Add(element);
            }
            return elements;
        }
        private string BuildSelectedLanguagesToString()
        {
            string result = String.Empty;
            var selectedCheckboxes = ToList(CHECKBOXES_PARENT.Children).Where(cb => (bool)((CheckBox)cb).IsChecked);
            for(var x = 0; x < selectedCheckboxes.Count(); x++)
            {
                if(x ==  selectedCheckboxes.Count() - 1)
                    result += $"{((CheckBox)selectedCheckboxes.ToList()[x]).Tag}";
                else 
                    result += $"{((CheckBox)selectedCheckboxes.ToList()[x]).Tag}+";
            }
            return result;
        }
        private void BuildCheckboxes()
        {
            var languages = SettingsManager.Instance.Settings.GeneralSettings.Languages;
            var LanguagesArray = languages.Split('+');
            var checkboxes = CHECKBOXES_PARENT.Children;
            foreach (var checkbox in checkboxes)
            {
                var cb = (CheckBox)checkbox;
                if (cb != null)
                {
                    if (cb.Tag != null)
                    {
                        foreach (var language in LanguagesArray)
                        {
                            if (cb.Tag.ToString().ToLower().Equals(language.ToLower()))
                            {
                                cb.IsChecked = true;
                                break;
                            }
                            else
                            {
                                cb.IsChecked = false;
                            }
                        }
                    }
                }
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = SettingsManager.Instance.Settings.GeneralSettings;
            BuildCheckboxes();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var result = BuildSelectedLanguagesToString();
            SettingsManager.Instance.Settings.GeneralSettings.Languages = result;
            
        }

        private void UILanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TessDataFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var picker = (FancyPicker)sender;
                SettingsManager.Instance.Settings.GeneralSettings.TessDataFolder = dialog.FileName;
            }
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
                    var isHardDriveLetter = folder.LastIndexOf("\\") < 4;
                    //-- fail safe just incase someone wants to get silly.
                    if (isHardDriveLetter)
                    {
                        var forcedDirectory = $"{dialog.FileName}TotalBattleChestTracker";
                        Directory.CreateDirectory(forcedDirectory);
                        newClanRootFolder = forcedDirectory;
                    }

                    MoveClanFolderWindow moveClanFolderWindow = new MoveClanFolderWindow();
                    moveClanFolderWindow.OldClanRootFOlder = oldClanRootFolder;
                    moveClanFolderWindow.NewClanRootFolder = newClanRootFolder;
                    if (moveClanFolderWindow.ShowDialog() == true)
                    {
                        SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder = newClanRootFolder;
                    }
                }
            }
        }
    }
}
