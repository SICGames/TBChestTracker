using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft;

namespace TBChestTracker
{

    [System.Serializable]
    public enum CaptureEnum
    {
        ENTIRE_SCREEN,
        SPECIFIC_REGION
    }

    [System.Serializable]
    public class OCRSettings : INotifyPropertyChanged, IDisposable
    {
        private string? _TessDataFolder;
        public string? CaptureMethod { get; set; }
       
        public string TessDataFolder
        {
            get
            {
                if(String.IsNullOrEmpty(_TessDataFolder))
                {
                    _TessDataFolder = $@"{AppContext.Instance.TesseractData}";
                }

                return _TessDataFolder;
            }
            set
            {
                _TessDataFolder = value;
                OnPropertyChanged(nameof(TessDataFolder));
            }
        }

        private string _languages;

        public string Languages
        {
            get
            {
                if(String.IsNullOrEmpty(_languages))
                {
                    _languages = "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus+pol+por+pus+ukr+deu";
                }
                return  _languages;
            }
            set
            {
                _languages = value;
            }
        }
        private ObservableCollection<String> _tags;
        public ObservableCollection<String> Tags 
        {
            get {
                return _tags ?? new ObservableCollection<String>() { "Chest", "From", "Source", "Gift" };
            }
            set {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        private CaptureEnum _capture = CaptureEnum.SPECIFIC_REGION;

        public CaptureEnum Capture
        {
            get => _capture;
            set
            {
                _capture = value;
                OnPropertyChanged(nameof(Capture));
            }
        }
        private String _previewImage;
        public String PreviewImage
        {
            get
            {
                return String.IsNullOrEmpty(_previewImage) == true ? String.Empty : _previewImage;
            }
            set
            {
                _previewImage = value;
                OnPropertyChanged(nameof(PreviewImage));
            }
        }

        private Double? _globalbrightness;
        public Double? GlobalBrightness
        {
            get => _globalbrightness.GetValueOrDefault(0.65);
            set
            {
                _globalbrightness = value;
                OnPropertyChanged(nameof(GlobalBrightness));
            }
        }
        private bool? _applyblur;
        public bool ApplyBlur
        {
            get => _applyblur.GetValueOrDefault(true);
            set
            {
                _applyblur = value;
                OnPropertyChanged(nameof(ApplyBlur));
            }
        }
        private bool? _applyInvert;
        public bool ApplyInvert
        {
            get => _applyInvert.GetValueOrDefault(true);
            set
            {
                _applyInvert = value;
                OnPropertyChanged(nameof(ApplyInvert));
            }
        }
        private Int32? _scaleFactor;
        public Int32 ScaleFactor
        {
            get => _scaleFactor.GetValueOrDefault(2);
            set
            {
                _scaleFactor = value;
                OnPropertyChanged(nameof(ScaleFactor));
            }
        }

        private Int32? _threshold;
        public Int32 Threshold
        {
            get => _threshold.GetValueOrDefault(85);
            set
            {
                _threshold = value;
                OnPropertyChanged(nameof(Threshold));
            }
        }

        private Int32? _maxThreshold;
        public Int32 MaxThreshold
        {
            get => _maxThreshold.GetValueOrDefault(255);
            set
            {
                _maxThreshold = value;
                OnPropertyChanged(nameof(MaxThreshold));
            }
        }

        private double _dpi = 600;
        public double Dpi
        {
            get => (double)_dpi;
            set
            {
                _dpi = value;
                OnPropertyChanged(nameof(Dpi));
            }
        }
        /*
        private bool? _enableImageFilter;
        public bool EnableImageFilter
        {
            get => _enableImageFilter.GetValueOrDefault(false);
            set
            {
                _enableImageFilter = value;
                OnPropertyChanged(nameof(EnableImageFilter));
            }
        }
        */

        private bool? _saveScreenCaptures;
        public bool SaveScreenCaptures
        {
            get  => _saveScreenCaptures.GetValueOrDefault(false);   
            set
            {
                _saveScreenCaptures = value;
                OnPropertyChanged(nameof(SaveScreenCaptures));
            }
        }
        private TessDataConfig _TessDataConfig;
        private bool disposedValue;

        public TessDataConfig TessDataConfig
        {
            get
            {
                if (_TessDataConfig == null)
                {
                    
                }
                return _TessDataConfig;
            }
            set
            {
                _TessDataConfig = value;
                OnPropertyChanged(nameof(TessDataConfig));
            }
        }

        public OCRSettings() 
        { 
            // AreaOfInterest = new AOIRect();
            // SuggestedAreaOfInterest = new AOIRect();
            // ClaimChestButtons = new List<Point>();
            // TessDataConfig = new TessDataConfig(TessDataOption.Best);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    TessDataConfig = null;
                    /*
                    if (ClaimChestButtons != null)
                    {
                        ClaimChestButtons.Clear();
                        ClaimChestButtons = null;
                    }
                    SuggestedAreaOfInterest.Dispose();
                    AreaOfInterest.Dispose();
                    */
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OCRSettings()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
