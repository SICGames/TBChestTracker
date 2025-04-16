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
using System.ComponentModel;
using System.Collections.ObjectModel;
using TBChestTracker.ViewModels;
using System.Drawing;
using TBChestTracker.Engine;
using TBChestTracker.Extensions;
using com.HellStormGames.Diagnostics;
using Emgu.CV.Structure;
using Emgu.CV;
using TBChestTracker.Effects;

namespace TBChestTracker.Windows.OCRStudio
{
    /// <summary>
    /// Interaction logic for OCRResultsPreviewWindow.xaml
    /// </summary>
    public partial class OCRResultsPreviewWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected  void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool? _isGeneratingResults;
        public bool IsGeneratingResults
        {
            get => _isGeneratingResults.GetValueOrDefault(true);
            set
            {
                _isGeneratingResults = value;
                OnPropertyChanged(nameof(IsGeneratingResults));
            }
        }

        private string _curtainMessage;
        public string CurtainMessage
        {
            get => _curtainMessage;
            set
            {
                _curtainMessage = value;
                OnPropertyChanged(nameof(CurtainMessage));
            }
        }

        private bool? _enableImageFiltering;
        public bool EnableImageFiltering
        {
            get => _enableImageFiltering.GetValueOrDefault(false);
            set
            {
                _enableImageFiltering = value;
                OnPropertyChanged(nameof(EnableImageFiltering));
            }
        }

        private ObservableCollection<OCRPreviewView> OcrPreviewResults = new ObservableCollection<OCRPreviewView>();
        private BitmapSource _GeneratedBitmap;
        public BitmapSource GeneratedBitmap
        {
            get => _GeneratedBitmap;
            set
            {
                _GeneratedBitmap = value;
                OnPropertyChanged(nameof(GeneratedBitmap));
            }
        }

        private BitmapSource _PreviewImage = null;
        public BitmapSource PreviewImage
        {
            get => _PreviewImage;
            set
            {
                _PreviewImage = value;
                OnPropertyChanged(nameof(PreviewImage));
            }
        }

        private bool? _ShowProgressBar;
        public bool ShowProgressBar
        {
            get => _ShowProgressBar.GetValueOrDefault(false);
            set
            {
                _ShowProgressBar = value;
                OnPropertyChanged(nameof(ShowProgressBar));
            }
        }
        
        public OCRResultsPreviewWindow()
        {
            InitializeComponent();
            this.DataContext = this;    
        }

        private void UpdateCurtainUI(string message, bool showCurtain, bool ProgressBarVisible = false)
        {
            Dispatcher.BeginInvoke(new Action
                (() =>
                {
                    IsGeneratingResults = showCurtain;
                    CurtainMessage = message;
                    ShowProgressBar = ProgressBarVisible;
                }));
        }
        private void ShowTessyResults(Bitmap bitmap)
        {
            if(bitmap == null)
            {
                Loggio.Warn("OCR Studio", "Bitmap is null");
                UpdateCurtainUI("An issue occurred. Close Preview Window and try again.", true);
                return;
            }

            var ocr = OCREngine.Instance;
            this.Dispatcher.BeginInvoke(() =>
            {
                OCRResultsView.ItemsSource = null;
                OcrPreviewResults?.Clear();
            });

            var result = ocr?.Read(bitmap);
            if(result == null)
            {
                Loggio.Warn("OCR Studio", "OCR Results is null");
                UpdateCurtainUI("An issue occurred. Close Preview Window and try again.", true);
                return;
            } 

            var step = 3;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (result != null)
                {
                    for (var index = 0; index < result.Words.Count; index++)
                    {
                        try
                        {
                            var i = result?.Words?.FindIndex(index, s => s.StartsWith("Contains:"));
                            if (i > index)
                            {
                                //-- we have a expired chest.
                                step = 4;
                            }
                            else
                            {
                                step = 3;
                            }
                            var chestname = result?.Words[index + 0];
                            var clanmate = result?.Words[index + 1];
                            var sourceOfChest = result?.Words[index + 2];
                            var contains = step == 4 ? result?.Words[index + 3] : "";
                            OcrPreviewResults?.Add(new OCRPreviewView(chestname, clanmate, sourceOfChest, contains));
                            index += step - 1;
                        }
                        catch (Exception ex)
                        {
                            if (ex is IndexOutOfRangeException)
                            {
                                //-- possibly due to bad filter settings.
                                UpdateCurtainUI("Hit a snag in the road. Possibly bad image filtering settings?", true);
                                ocr = null;
                                return;
                            }
                        }
                    }
                    if (OcrPreviewResults?.Count > 0)
                    {
                        OCRResultsView.ItemsSource = OcrPreviewResults;
                        UpdateCurtainUI("", false);
                    }
                    else
                    {
                        //-- display error.
                        UpdateCurtainUI("There's no results available.", true);
                    }
                    ocr = null;
                }
            }));
        }

        private System.Drawing.Bitmap ApplyImageFiltering(System.Drawing.Bitmap src, double brightness, int threshold, int maxthreshold)
        {
            System.Drawing.Bitmap result = null;
            Image<Gray, byte> image = ((System.Drawing.Bitmap)src).ToImage<Gray, byte>();
            var outputimage = image.Mat;
            outputimage = ImageEffects.Blur(outputimage, 1);
            var threshold_gray = new Gray(threshold);
            var maxThreshold_gray = new Gray(maxthreshold);
            outputimage = ImageEffects.ThresholdBinaryInv(outputimage, threshold_gray, maxThreshold_gray);
            outputimage = ImageEffects.Resize(outputimage, 2, Emgu.CV.CvEnum.Inter.NearestExact);
            outputimage = 255 - outputimage;
            result = outputimage.ToBitmap();
            PreviewImage = result.AsBitmapSource();
            outputimage.Dispose();
            outputimage = null;
            image.Dispose();
            image = null;
            return result;
        }

        private Task ShowTessyResultsTask(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() => ShowTessyResults(bitmap));
        }
        private async Task BeginPreviewingOCRResults()
        {
            var bs = PreviewImage;
            Preview.IsEnabled = false;
            if (bs != null)
            {
                UpdateCurtainUI("Generating OCR Results...", true, true);
                var bmp = ((BitmapSource)bs).AsBitmap();
                System.Drawing.Bitmap FinalBitmap = null;
                if(EnableImageFiltering)
                {
                    var ocr = SettingsManager.Instance.Settings.OCRSettings;
                    FinalBitmap = ApplyImageFiltering(bmp, ocr.GlobalBrightness, ocr.Threshold, ocr.MaxThreshold);
                }
                else
                {
                    FinalBitmap = bmp;
                }

                GeneratedBitmap = FinalBitmap.AsBitmapSource();
                ImageContainer.Source = GeneratedBitmap;

                await ShowTessyResultsTask(FinalBitmap).ContinueWith(t =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        Preview.IsEnabled = true;
                    });
                });
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ocrSettings = SettingsManager.Instance.Settings.OCRSettings;
            UpdateCurtainUI("Press 'Preview' To Generate OCR Results.", true);
            OcrPreviewResults = new ObservableCollection<OCRPreviewView>();
            await BeginPreviewingOCRResults();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OcrPreviewResults?.Clear();
            OcrPreviewResults = null;
        }
     
        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurtainUI("Creating Region of Interest Image...", true, true);
           PreviewImage = ((StudioCanvas)this.Owner).GetRegionOfInterestImage();
           await BeginPreviewingOCRResults();
        }
    }
}
