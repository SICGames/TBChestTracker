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

        private void FancyPicker_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var picker = (FancyPicker)sender;
                picker.Source = dialog.FileName;
            }
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
        }
    }
}
