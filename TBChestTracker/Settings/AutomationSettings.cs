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

        private int? _automationClicks;
        public int AutomationClicks
        {
            get => _automationClicks.GetValueOrDefault(4);
            set
            {
                _automationClicks = value;
                OnPropertyChanged(nameof(AutomationClicks));    
            }
        }
        private bool? _automaticallyCloseChestBuildingDialogAfterFinished;
        public bool AutomaticallyCloseChestBuildingDialogAfterFinished
        {
            get => _automaticallyCloseChestBuildingDialogAfterFinished.GetValueOrDefault(false);
            set
            {
                _automaticallyCloseChestBuildingDialogAfterFinished = value;
                OnPropertyChanged(nameof(AutomaticallyCloseChestBuildingDialogAfterFinished));
            }
        }
        private int? _automationDelayBetweenClicks;
        public int AutomationDelayBetweenClicks
        {
            get => _automationDelayBetweenClicks.GetValueOrDefault(250);
            set
            {
                _automationDelayBetweenClicks = value;
                OnPropertyChanged(nameof(AutomationDelayBetweenClicks));    
            }
        }
        private int? _automationScreenshotsAfterClicks;
        public int AutomationScreenshotsAfterClicks
        {
            get => _automationScreenshotsAfterClicks.GetValueOrDefault(500);
            set
            {
                _automationScreenshotsAfterClicks = value;
                OnPropertyChanged(nameof(AutomationScreenshotsAfterClicks));    
            }
        }
        private int? _stopautomationAfterClicks;
        public int StopAutomationAfterClicks
        {
            get => _stopautomationAfterClicks.GetValueOrDefault(0);
            set
            {
                _stopautomationAfterClicks = value;
                OnPropertyChanged(nameof(StopAutomationAfterClicks));  
            }
        }

        private bool? _autorepairafterstopping;
        public bool AutoRepairAfterStoppingAutomation
        {
            get => _autorepairafterstopping.GetValueOrDefault(true);
            set
            {
               _autorepairafterstopping = value;
                OnPropertyChanged(nameof(AutoRepairAfterStoppingAutomation));
            }
        }
        private bool? _BuildChestsAfterStoppingAutomation;
        public bool BuildChestsAfterStoppingAutomation
        {
            get => _BuildChestsAfterStoppingAutomation.GetValueOrDefault(true);
            set
            {
                _BuildChestsAfterStoppingAutomation = value;
                OnPropertyChanged(nameof(BuildChestsAfterStoppingAutomation));
            }
        }
    }
}
