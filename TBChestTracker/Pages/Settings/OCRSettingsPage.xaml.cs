using Emgu.CV.Structure;
using Emgu.CV;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBChestTracker.Managers;
using com.KonquestUI.Controls;
using System.ComponentModel;
using com.HellstormGames.Imaging.Extensions;
using Microsoft.WindowsAPICodePack.Dialogs;
using TBChestTracker.UI;

namespace TBChestTracker.Pages.Settings
{
    /// <summary>
    /// Interaction logic for OCRSettingsPage.xaml
    /// </summary>
    public partial class OCRSettingsPage : Page, INotifyPropertyChanged
    {
        private BitmapSource _PreviewImage = null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void onPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapSource PreviewImage
        {
            get => _PreviewImage;
            set
            {
                _PreviewImage = value;
                onPropertyChanged(nameof(PreviewImage));
            }
        }
        public OCRSettingsPage()
        {
            InitializeComponent();

            //-- TessDataFolder value not showing in uiFancyPicker. Only in Page_Loaded event.

            this.DataContext = SettingsManager.Instance.Settings.OCRSettings;
        }

        private static List<UIElement> ToList(UIElementCollection collection)
        {
            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement element in collection)
            {
                elements.Add(element);
            }
            return elements;
        }
        private string BuildSelectedLanguagesToString()
        {
            string result = String.Empty;
            var selectedCheckboxes = ToList(CHECKBOXES_PARENT.Children).Where(cb => (bool)((CheckBox)cb).IsChecked);
            for (var x = 0; x < selectedCheckboxes.Count(); x++)
            {
                if (x == selectedCheckboxes.Count() - 1)
                    result += $"{((CheckBox)selectedCheckboxes.ToList()[x]).Tag}";
                else
                    result += $"{((CheckBox)selectedCheckboxes.ToList()[x]).Tag}+";
            }
            return result;
        }
        private void BuildCheckboxes()
        {
            var languages = SettingsManager.Instance.Settings.OCRSettings.Languages;
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
            //-- let's ensure we have the updated image preview
            
            var ocr = SettingsManager.Instance.Settings.OCRSettings;
            UpdatePreviewImage(ocr.GlobalBrightness, (int)ocr.Threshold, (int)ocr.MaxThreshold);
            BuildCheckboxes();
            
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var result = BuildSelectedLanguagesToString();
            SettingsManager.Instance.Settings.OCRSettings.Languages = result;
            SettingsManager.Instance.Settings.OCRSettings.CaptureMethod = CaptureMethodBox.Text;
        }

        private void TessDataFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var picker = (FancyPicker)sender;
                SettingsManager.Instance.Settings.OCRSettings.TessDataFolder = dialog.FileName;
            }
        }

        private void FancyNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var value = ((FancyNumericValue)sender).Value;
            var ocr = SettingsManager.Instance.Settings.OCRSettings;
            SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness = value;
            UpdatePreviewImage(value, ocr.Threshold, ocr.MaxThreshold);

        }

        private void UpdatePreviewImage(double brightness, int threshold, int maxthreshold)
        {
            if (String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.PreviewImage) == false)
            {
                var bmp = System.Drawing.Bitmap.FromFile(SettingsManager.Instance.Settings.OCRSettings.PreviewImage);

                Image<Gray, byte> image = ((System.Drawing.Bitmap)bmp).ToImage<Gray, byte>();
                
                var imageBrightened = image.Mul(brightness) + brightness;
                var imageScaled = imageBrightened.Resize(2, Emgu.CV.CvEnum.Inter.Cubic);

                var threshold_gray = new Gray(threshold);
                var maxThreshold_gray = new Gray(maxthreshold);

                var imageThreshold = imageScaled.ThresholdBinaryInv(threshold_gray, maxThreshold_gray);
                
                Image<Gray,byte> erodedImage = imageThreshold.Erode(1);

                ImagePreview.Source =  erodedImage.ToBitmap().ToBitmapSource();
                SettingsManager.Instance.Settings.OCRSettings.Threshold = threshold;
                SettingsManager.Instance.Settings.OCRSettings.MaxThreshold = maxthreshold;
                
            }
        }

        private void ThresholdNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var fancyNumeric = (FancyNumericValue)sender;
            var brightness = SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness;
            UpdatePreviewImage((double)brightness, (int)fancyNumeric.Value, SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);
        }

        private void MaxThresholdNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var fancyNumeric = (FancyNumericValue)sender;
            var brightness = SettingsManager.Instance.Settings.OCRSettings.GlobalBrightness;
            UpdatePreviewImage(brightness, SettingsManager.Instance.Settings.OCRSettings.Threshold, (int)fancyNumeric.Value);
        }
    }
}
