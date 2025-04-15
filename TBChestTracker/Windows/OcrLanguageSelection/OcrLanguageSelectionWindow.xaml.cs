using com.KonquestUI.Controls;
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
using TBChestTracker.Automation;
using TBChestTracker.Data;
using TBChestTracker.UI;
using com.HellStormGames.Diagnostics.Logging;
using com.HellStormGames.Diagnostics;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for OcrLanguageSelectionWindow.xaml
    /// </summary>
    public partial class OcrLanguageSelectionWindow : Window
    {
        private OcrLanguages OcrLanguages = new OcrLanguages();
        private ObservableCollection<OcrLanguage> OcrLanguagesList;
        private ObservableCollection<OcrLanguage> SelectedLanguageList;
        private SettingsManager SettingsManager;

        public OcrLanguageSelectionWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            OcrLanguagesList = new ObservableCollection<OcrLanguage>();
            SelectedLanguageList = new ObservableCollection<OcrLanguage>();
            this.DataContext = this;
            SettingsManager = settingsManager;
        }

        private void BuildLanguagesFromSettings()
        {
            if (SettingsManager == null)
            {
                Loggio.Warn("Ocr Language Selection", "Settings Manager is null");
                return;
            }
            if (String.IsNullOrEmpty(SettingsManager.Settings.OCRSettings.Languages))
            {
                Loggio.Warn("Ocr Languages Selection", "Ocr Languages from settings.json file is empty. It could be because the user is new.");
                return;
            }
            var languages = SettingsManager.Settings.OCRSettings.Languages;
            if (languages.Contains("all"))
            {
                languages = languages.Replace("+all", "");
                SettingsManager.Settings.OCRSettings.Languages = languages;
                SettingsManager.Save();
            }

            var languagesArray = languages.Split('+');
            foreach (var language in languagesArray)
            {
                var availableLanguage = OcrLanguages.Languages.Select(l => l).Where(c => c.Code.Equals(language)).FirstOrDefault();
                var index = AvailableLanguagesBox.Items.IndexOf(availableLanguage);
                AvailableLanguagesBox.SelectedIndex = index;
                MoveRight();
            }
        }
        private void BuildAvailableLanguageList()
        {
            OcrLanguagesList?.Clear();
            if (OcrLanguages.Languages.Any() == false)
            {
                Loggio.Warn("Ocr Languages Selection", "Languages are empty or didn't load properly. Technically, shouldn't have happened.");
            }

            foreach(var lang in OcrLanguages?.Languages)
            {
                OcrLanguagesList?.Add(lang);
            }
            if (OcrLanguagesList?.Count > 0)
            {
                AvailableLanguagesBox.ItemsSource = OcrLanguagesList;
                CollectionViewSource.GetDefaultView(AvailableLanguagesBox.Items).SortDescriptions.Add(new System.ComponentModel.SortDescription("Language", System.ComponentModel.ListSortDirection.Ascending));
                SelectedLanguagesBox.ItemsSource = SelectedLanguageList;
            }
        }
        private void Resort()
        {
            CollectionViewSource.GetDefaultView(AvailableLanguagesBox.Items).SortDescriptions.Add(new System.ComponentModel.SortDescription("Language", System.ComponentModel.ListSortDirection.Ascending));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OcrLanguages.Load())
            {
                BuildAvailableLanguageList();
                BuildLanguagesFromSettings();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Loggio.Info("Ocr Languages Selection", "Closing window.");
            OcrLanguagesList?.Clear();
            OcrLanguagesList = null;
            SelectedLanguageList?.Clear();
            SelectedLanguageList = null;
            OcrLanguages?.Dispose();
        }

        private void Applybtn_Click(object sender, RoutedEventArgs e)
        {
            var langStr = String.Empty;
            Loggio.Info("Ocr Languages Selection", "Ocr Languages being generated and applied to settings.");
            foreach (var language in SelectedLanguageList?.ToList())
            {
                langStr += $"{language.Code}+";
            }
            Loggio.Info("Fixing language string.");
            langStr = langStr.Substring(0, langStr.Length - 1);
            Loggio.Info("Adding languages to settings.");
            SettingsManager.Settings.OCRSettings.Languages = langStr;
            Loggio.Info("Disabling showing Ocr language Selection.");
            SettingsManager.Settings.GeneralSettings.ShowOcrLanguageSelection = false;
            Loggio.Info("Saving settings.");
            SettingsManager?.Save();
            Loggio.Info("Ocr Languages Selection", "Ocr Languages Selected and saved.");

            if(MessageBox.Show("New Ocr Language Configuration have been applied. Requires to restart Tesseract OCR", "New Ocr Languages Applied", MessageBoxButton.OK) == MessageBoxResult.OK)
            {
                if (ChestAutomation.Instance != null)
                {
                    ChestAutomation.Instance.Reinitialize(SettingsManager.Settings.OCRSettings);
                }
            }
            this.Close();            
        }

        private void MoveUp()
        {
            var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            foreach (var item in items.ToList())
            {
                var pos = SelectedLanguageList.IndexOf(item);
                if (pos > 0)
                {
                    SelectedLanguageList.Move(pos, pos - 1);
                }
            }
        }
        private void MoveDown()
        {
            var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            foreach (var item in items.ToList())
            {
                var pos = SelectedLanguageList.IndexOf(item);
                if (pos < SelectedLanguageList.Count)
                {
                    SelectedLanguageList.Move(pos, pos + 1);
                }
            }
        }
        private void MoveToStart()
        {
            var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            var start = 0;
            foreach (var item in items.ToList())
            {
                var pos = SelectedLanguageList.IndexOf(item);
                if (pos > 0)
                {
                    SelectedLanguageList.Move(pos, start);
                }
                start++;
            }
        }
        private void MoveToEnd()
        {
            var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            var end = SelectedLanguageList.Count - 1;
            foreach (var item in items.ToList())
            {
                var pos = SelectedLanguageList.IndexOf(item);
                if (pos < SelectedLanguageList.Count)
                {
                    SelectedLanguageList.Move(pos, end);
                    end--;
                }
            }
        }
        private void MoveLeft()
        {
            var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            foreach (var item in items.ToList())
            {
                OcrLanguagesList.Add(item);
                SelectedLanguageList.Remove(item);
            }
            Resort();
        }

        private void MoveRight()
        {
            var item = AvailableLanguagesBox.SelectedItems.Cast<OcrLanguage>();
            foreach (var i in item.ToList())
            {
                SelectedLanguageList.Add(i);
                OcrLanguagesList.Remove(i);
            }
        }

        private void MoveAllLeft()
        {
            if (SelectedLanguageList.Count > 0)
            {
                foreach (var item in SelectedLanguageList.ToList())
                {
                    OcrLanguagesList.Add(item);
                }
                SelectedLanguageList.Clear();
            }
        }

        private void MoveAllRight()
        {
            if (OcrLanguagesList.Count > 0)
            {
                foreach (var item in OcrLanguagesList.ToList())
                {
                    SelectedLanguageList.Add(item);
                }
                OcrLanguagesList.Clear();
            }
        }
        private void SelectedListControl_Click(object sender, RoutedEventArgs e)
        {
            var fb = (FancyButton)e?.OriginalSource;
            var tag = fb?.Tag?.ToString().ToLower();
            if (tag.Equals("moveup"))
            {
                MoveUp();
            }
            else if (tag.Equals("movedown"))
            {
                MoveDown();   
            }
            else if (tag.Equals("movetostart"))
            {
                MoveToStart();
            }
            else if (tag.Equals("movetoend"))
            {
                MoveToEnd();
            }

        }

        private void AvailableListControl_Click(object sender, RoutedEventArgs e)
        {
            var fb = (FancyButton)e?.OriginalSource;
            var tag = fb?.Tag?.ToString().ToLower();
            if (tag.Equals("left"))
            {
                MoveLeft();
            }
            else if(tag.Equals("right"))
            {
                MoveRight();
            }
            else if(tag.Equals("moveallleft"))
            {
                MoveAllLeft();
            }
            else if(tag.Equals("moveallright"))
            {
                MoveAllRight();
            }
        }
    }
}
