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
                return;

            var languages = SettingsManager.Settings.OCRSettings.Languages.Split('+');
            foreach (var language in languages)
            {
                var availableLanguage = OcrLanguages.Languages.Select(l => l).Where(c => c.Code.Equals(language)).FirstOrDefault();
                SelectedLanguageList.Add(availableLanguage);
            }
        }
        private void BuildAvailableLanguageList()
        {
            OcrLanguagesList.Clear();
            foreach(var lang in OcrLanguages.Languages)
            {
                OcrLanguagesList.Add(lang);
            }
            AvailableLanguagesBox.ItemsSource = OcrLanguagesList;
            CollectionViewSource.GetDefaultView(AvailableLanguagesBox.Items).SortDescriptions.Add(new System.ComponentModel.SortDescription("Language", System.ComponentModel.ListSortDirection.Ascending));
            SelectedLanguagesBox.ItemsSource = SelectedLanguageList;
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
            OcrLanguagesList?.Clear();
            OcrLanguagesList = null;
            SelectedLanguageList?.Clear();
            SelectedLanguageList = null;
            OcrLanguages.Dispose();
        }

        private void Applybtn_Click(object sender, RoutedEventArgs e)
        {
            var langStr = String.Empty;
            foreach(var langage in SelectedLanguageList.ToList())
            {
                langStr += $"{langage.Code}+";
            }
            langStr = langStr.Substring(0, langStr.Length - 1);
            SettingsManager.Settings.OCRSettings.Languages = langStr;
            SettingsManager.Settings.GeneralSettings.ShowOcrLanguageSelection = false;
            SettingsManager.Save();
            if(MessageBox.Show("New Ocr Language Configuration have been applied. Requires to restart Tesseract OCR", "New Ocr Languages Applied", MessageBoxButton.OK) == MessageBoxResult.OK)
            {
                ChestAutomation.Instance.Reinitialize(SettingsManager.Settings.OCRSettings);
            }
            this.Close();            
        }


        private void SelectedListControl_Click(object sender, RoutedEventArgs e)
        {
            var fb = (FancyButton)e?.OriginalSource;
            var tag = fb?.Tag?.ToString().ToLower();
            if (tag.Equals("moveup"))
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
            else if (tag.Equals("movedown"))
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
            else if (tag.Equals("movetostart"))
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
            else if (tag.Equals("movetoend"))
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

        }

        private void AvailableListControl_Click(object sender, RoutedEventArgs e)
        {
            var fb = (FancyButton)e?.OriginalSource;
            var tag = fb?.Tag?.ToString().ToLower();
            if (tag.Equals("left"))
            {
                var items = SelectedLanguagesBox.SelectedItems.Cast<OcrLanguage>();
                foreach(var item in items.ToList()) {
                    OcrLanguagesList.Add(item);
                    SelectedLanguageList.Remove(item);
                }
                Resort();
            }
            else if(tag.Equals("right"))
            {
                var item = AvailableLanguagesBox.SelectedItems.Cast<OcrLanguage>();
                foreach (var i in item.ToList())
                {
                    SelectedLanguageList.Add(i);
                    OcrLanguagesList.Remove(i);
                }
            }
            else if(tag.Equals("moveallleft"))
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
            else if(tag.Equals("moveallright"))
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
        }
    }
}
