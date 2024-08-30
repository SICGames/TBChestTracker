#define PREVIEW_BUILD
#define RC_BUILD
#define RC1_BUILD
#define RC2_BUILD
#define RC3_BUILD 
#define PRODUCTION_BUILD

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class AppContext : INotifyPropertyChanged
    {

        #region Private fields
        private bool isAutomationPlayButtonEnabled;
        private bool isAutomationPauseButtonEnabled;
        private bool isAutomationStopButtonEnabled;
        private bool isCurrentClandatabase;

        private string pAppName = "Total Battle Chest Tracker";
        private string pCurrentProject = "Untitled";
        private string pAppTitle = $"";

        private int _ChestCountTotal = 0;
        
        #endregion

        #region AppContext() & Instance
        public static AppContext Instance { get; private set; }

        public AppContext() 
        {
            if (Instance == null)
                Instance = this;
        }
        #endregion
        public bool isAppClosing = false;
        
        #region Public Declarations
        public Version AppVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        public string AppVersionString
        {
            get
            {
                var extraStr = String.Empty;
#if PREVIEW_BUILD
                extraStr = "'Preview Build'";
#endif

                return $"v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} [{extraStr}]";
            }
        }
        
        public string CurrentProject
        {
            get
            {
                return pCurrentProject;
            }
            set
            {
                pCurrentProject = value;
            }
        }
        
        public string AppTitle
        {
            get
            {
                return pAppTitle;
            }
            set
            {
                pAppTitle = value;
                OnPropertyChanged(nameof(AppTitle));
            }
        }

        public void UpdateCurrentProject(string currentProject)
        {
            CurrentProject = currentProject;
        }

        public void UpdateApplicationTitle()
        {
            AppTitle = $"{pAppName} {AppVersionString} - {CurrentProject}";

        }
        public bool IsCurrentClandatabase
        {
            get => isCurrentClandatabase;
            set
            {
                isCurrentClandatabase = value;
                OnPropertyChanged(nameof(IsCurrentClandatabase));
            }
        }
        public bool IsAutomationPlayButtonEnabled
        {
            get => isAutomationPlayButtonEnabled;
            set
            {
                isAutomationPlayButtonEnabled = value;
                OnPropertyChanged(nameof(IsAutomationPlayButtonEnabled));
            }
        }
        public bool IsAutomationStopButtonEnabled
        {
            get => isAutomationStopButtonEnabled;
            set
            {
                isAutomationStopButtonEnabled = value;
                OnPropertyChanged(nameof(IsAutomationStopButtonEnabled));
            }
        }

        public static string AppFolder
        {
            get => $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        }

        #endregion

        #region OnPropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
