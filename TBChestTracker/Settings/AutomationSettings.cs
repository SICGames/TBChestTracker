using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    public class AutomationSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _automationClicks;
        public int AutomationClicks
        {
            get => _automationClicks;
            set
            {
                _automationClicks = value;
                OnPropertyChanged(nameof(AutomationClicks));    
            }
        }
        private int _automationDelayBetweenClicks;
        public int AutomationDelayBetweenClicks
        {
            get => _automationDelayBetweenClicks;
            set
            {
                _automationDelayBetweenClicks = value;
                OnPropertyChanged(nameof(AutomationDelayBetweenClicks));    
            }
        }
        private int _automationScreenshotsAfterClicks;
        public int AutomationScreenshotsAfterClicks
        {
            get => _automationScreenshotsAfterClicks;
            set
            {
                _automationScreenshotsAfterClicks = value;
                OnPropertyChanged(nameof(AutomationScreenshotsAfterClicks));    
            }
        }
    }
}
