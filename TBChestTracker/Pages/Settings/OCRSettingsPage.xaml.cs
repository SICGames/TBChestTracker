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
using Microsoft.WindowsAPICodePack.Dialogs;
using TBChestTracker.UI;
using TBChestTracker.Extensions;
using System.Drawing;
using com.HellStormGames.OCR;
using TBChestTracker.Engine;
using System.Collections.ObjectModel;
using TBChestTracker.ViewModels;
using TBChestTracker.Effects;

namespace TBChestTracker.Pages.Settings
{
    /// <summary>
    /// Interaction logic for OCRSettingsPage.xaml
    /// </summary>
    /// 

    ///- need to make BitmapExtensions for ToBitmap() and ToBitmapSource()
    ///
    public partial class OCRSettingsPage : Page, INotifyPropertyChanged
    {
        private BitmapSource _PreviewImage = null;
        private ObservableCollection<OCRPreviewView> OcrPreviewResults = new ObservableCollection<OCRPreviewView>();
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

        private string _CurtainMessage = "Click Preview Button To Show OCR Results from Preview Image.";
        public string CurtainMessage
        {
            get => _CurtainMessage; 
            set
            {
                _CurtainMessage = value;
                onPropertyChanged(nameof(CurtainMessage));
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
      

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //-- let's ensure we have the updated image preview
            ocrResultsListView.DataContext = this;
            var ocr = SettingsManager.Instance.Settings.OCRSettings;
            UpdatePreviewImage((int)ocr.Threshold, (int)ocr.MaxThreshold);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            OcrPreviewResults.Clear();
            OcrPreviewResults = null;
            SettingsManager.Instance.Settings.OCRSettings.Tags = TagBox.Tags;
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
            UpdatePreviewImage(ocr.Threshold, ocr.MaxThreshold);
        }

        private void ShowTessyResults(Bitmap bitmap)
        {
            var ocr = OCREngine.Instance;
            var result = ocr.Read(bitmap);
            var step = 3;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                for (var index = 0; index < result.Words.Count; index++)
                {
                    try
                    {
                        var i = result.Words.FindIndex(index, s => s.StartsWith("Contains:"));
                        if (i > index)
                        {
                            //-- we have a expired chest.
                            step = 4;
                        }
                        else
                        {
                            step = 3;
                        }
                        var chestname = result.Words[index + 0];
                        var clanmate = result.Words[index + 1];
                        var sourceOfChest = result.Words[index + 2];
                        var contains = step == 4 ? result.Words[index + 3] : "";
                        OcrPreviewResults.Add(new OCRPreviewView(chestname, clanmate, sourceOfChest, contains));
                        index += step - 1;
                    }
                    catch (Exception ex)
                    {
                        if(ex is IndexOutOfRangeException)
                        {
                            //-- possibly due to bad filter settings.
                            CurtainMessage = "Hit a rock in the road. Bad filtering settings, maybe. Adjust filtering settings and try again.";
                            ocr = null;
                            return;
                        }
                    }
                }
                if (OcrPreviewResults.Count > 0)
                {
                    ocrResultsListView.ItemsSource = OcrPreviewResults;
                }
                else
                {
                    //-- display error.
                    CurtainMessage = "There's No Results Available.";
                }

                ocr = null;
            }));
        }
        private Task ShowTessyResultsTask(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() => ShowTessyResults(bitmap));
        }

        private void UpdatePreviewImage(int threshold, int maxthreshold)
        {
            if (String.IsNullOrEmpty(SettingsManager.Instance.Settings.OCRSettings.PreviewImage) == false)
            {
                var bmp = System.Drawing.Bitmap.FromFile(SettingsManager.Instance.Settings.OCRSettings.PreviewImage);
                Image<Gray, byte> image = ((System.Drawing.Bitmap)bmp).ToImage<Gray, byte>();
                var threshold_gray = new Gray(threshold);
                var maxThreshold_gray = new Gray(maxthreshold);
                var blurStrength = 1;
                var outputimage = ImageEffects.Blur(image.Mat, blurStrength);
                outputimage = ImageEffects.ThresholdBinaryInv(outputimage, threshold_gray, maxThreshold_gray);
                outputimage = ImageEffects.Resize(outputimage, 2, Emgu.CV.CvEnum.Inter.NearestExact);
                outputimage = 255 - outputimage;
                
                ImagePreview.Source = outputimage.ToBitmap().AsBitmapSource();

                SettingsManager.Instance.Settings.OCRSettings.Threshold = threshold;
                SettingsManager.Instance.Settings.OCRSettings.MaxThreshold = maxthreshold;
                
                outputimage.Dispose();
                outputimage = null;
                image.Dispose();
                image = null;
                bmp.Dispose();
                bmp = null;
            }
        }

        private void ThresholdNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var fancyNumeric = (FancyNumericValue)sender;
            UpdatePreviewImage((int)fancyNumeric.Value, SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);
        }

        private void MaxThresholdNumericValue_ValueChanged(object sender, RoutedEventArgs e)
        {
            var fancyNumeric = (FancyNumericValue)sender;
            UpdatePreviewImage(SettingsManager.Instance.Settings.OCRSettings.Threshold, (int)fancyNumeric.Value);
        }

        private void EnableImageFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreviewImage(SettingsManager.Instance.Settings.OCRSettings.Threshold, SettingsManager.Instance.Settings.OCRSettings.MaxThreshold);
        }

        private async void PreviewOCRButton_Click(object sender, RoutedEventArgs e)
        {
            var bs = ImagePreview.Source;
            if (bs != null)
            {
                CurtainMessage = "Generating OCR Result...";
                Curtain.Visibility = Visibility.Visible;
                OcrPreviewResults.Clear();

                PreviewOCRButton.IsEnabled = false;
                var bmp = ((BitmapImage)bs).AsBitmap();
                await ShowTessyResultsTask(bmp).ContinueWith(t =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        if (OcrPreviewResults.Count > 0)
                        {
                            Curtain.Visibility = Visibility.Hidden;
                        }
                        PreviewOCRButton.IsEnabled = true;
                    });
                });

            }
        }

        private void OcrLanguagesToolLink_Click(object sender, RoutedEventArgs e)
        {
            OcrLanguageSelectionWindow langselect = new OcrLanguageSelectionWindow(SettingsManager.Instance);
            langselect.Show();
        }
    }
}
