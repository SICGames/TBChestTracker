using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class OCRSettings : INotifyPropertyChanged
    {

        private double _GlobalBrightness;
        public double GlobalBrightness
        {
            get
            {
                return _GlobalBrightness;
            }
            set
            {
                _GlobalBrightness = value;
                OnPropertyChanged(nameof(GlobalBrightness));
            }
        }

        private ObservableCollection<string> _tags = new ObservableCollection<string>();
        public ObservableCollection<String> Tags 
        {
            get {
         
                return _tags;
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

        public string CaptureMethod { get; set; }
        public AOIRect AreaOfInterest { get; set; }
        public AOIRect SuggestedAreaOfInterest { get; set; }
        public List<Point> ClaimChestButtons {  get; set; }    
        public OCRSettings() 
        { 
            AreaOfInterest = new AOIRect();
            SuggestedAreaOfInterest = new AOIRect();
            ClaimChestButtons = new List<Point>();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
