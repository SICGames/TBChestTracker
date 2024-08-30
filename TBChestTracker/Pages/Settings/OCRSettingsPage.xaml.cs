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
            this.DataContext = SettingsManager.Instance.Settings;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //-- let's ensure we have the updated image preview
            var ocr = SettingsManager.Instance.Settings.OCRSettings;
            
            UpdatePreviewImage(ocr.GlobalBrightness, (int)ocr.Threshold, (int)ocr.MaxThreshold);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Settings.OCRSettings.CaptureMethod = CaptureMethodBox.Text;
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
                var imageScaled = imageBrightened.Resize(5, Emgu.CV.CvEnum.Inter.Cubic);

                var threshold_gray = new Gray(threshold);
                var maxThreshold_gray = new Gray(maxthreshold);

                var imageThreshold = imageScaled.ThresholdBinaryInv(threshold_gray, maxThreshold_gray);

                ImagePreview.Source = imageThreshold.ToBitmap().ToBitmapSource();
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
