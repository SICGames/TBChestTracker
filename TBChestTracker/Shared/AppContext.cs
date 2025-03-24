#define PREVIEW_BUILD
#define RC_BUILD
#define RC1_BUILD
#define RC2_BUILD
#define RC3_BUILD 
#define PRODUCTION_BUILD

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker
{
    public class AppContext : INotifyPropertyChanged
    {

        #region Declarations
        public string ForcedDateFormat =>"yyyy-MM-dd";
        private bool? _showDebugMenu;

        private Visibility _DebugMenuVisibility = Visibility.Hidden;
        public Visibility DebugMenuVisibility
        {
            get => _DebugMenuVisibility;
            set
            {
                _DebugMenuVisibility= value;
                OnPropertyChanged(nameof(DebugMenuVisibility));
            }
        }
        public bool bShowDebugMenu
        {
            get
            {
                return _showDebugMenu.GetValueOrDefault(false);
            }
            private set
            {
                _showDebugMenu = value;
                if (value == false)
                {
                    DebugMenuVisibility = Visibility.Hidden;
                }
                else
                {
                    DebugMenuVisibility = Visibility.Visible;
                }

                OnPropertyChanged(nameof(bShowDebugMenu));
            }
        }

        private bool? _isBuildingChests;
        public bool IsBuildingChests
        {
            get => _isBuildingChests.GetValueOrDefault(false);
            set
            {
                _isBuildingChests = value;
                OnPropertyChanged(nameof(IsBuildingChests));
            }
        }

        private bool? _HasBuiltChests;
        private bool? _HasCollectedChests;

        private bool isAutomationPlayButtonEnabled;
        private bool isAutomationPauseButtonEnabled;
        private bool isAutomationStopButtonEnabled;
        private bool isCurrentClandatabase;
        private bool newClanDatabaseCreated;
        
        private string pAppName = "Total Battle Chest Tracker";
        private string pCurrentProject = "Untitled";
        private string pAppTitle = $"";
        private bool _upgradeAvailable = false;

        private bool? _bDeleteTessData;
        public bool bDeleteTessData
        {
            get
            {
                return _bDeleteTessData.GetValueOrDefault(false);
            }
            set
            {
                _bDeleteTessData = value;
            }
        }
        public bool upgradeAvailable
        {
            get => _upgradeAvailable;
            set
            {
                _upgradeAvailable = value;
                OnPropertyChanged(nameof(upgradeAvailable));
            }
        }

        public string LocalApplicationPath
        {
            get => $@"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\SICGames\TotalBattleChestTracker\";
        }
        public string CommonAppFolder
        {
            get => $@"{System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\SICGames\TotalBattleChestTracker\";
        }
        public string AppFolder
        {
            get => $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        }
        public string RecentOpenedClanDatabases
        {
            get => $@"{LocalApplicationPath}recent.db";
        }
        public string TesseractData => $@"{LocalApplicationPath}TessData";

        public bool isAppClosing = false;
      
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
                extraStr = Manifest.Build;
#endif

                return $"v{AppVersion.Major}.{AppVersion.Minor}.{AppVersion.Build} {extraStr} ";
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
        public bool NewClandatabaseBeenCreated
        {
            get => newClanDatabaseCreated;
            set
            {
                newClanDatabaseCreated = value;
                OnPropertyChanged(nameof(NewClandatabaseBeenCreated));  
            }
        }
        private bool _TessDataExists = false;

        public bool TessDataExists 
        {
            get => _TessDataExists;
            set
            {
                _TessDataExists = value;
                OnPropertyChanged(nameof(TessDataExists));
            }
        }
        private bool _AutomationRunning = false;
        public bool AutomationRunning
        {
            get => _AutomationRunning;
            set
            {
                _AutomationRunning = value;
                OnPropertyChanged(nameof(AutomationRunning));
            }
        }
        private bool? _IsConfiguringHotKeys;
        public bool IsConfiguringHotKeys
        {
            get => _IsConfiguringHotKeys.GetValueOrDefault(false);
            set
            {
                _IsConfiguringHotKeys = value;
                OnPropertyChanged(nameof(IsConfiguringHotKeys));    
            }
        }
        
        private bool _DebugOCRWizardEnabled = false;
        public bool DebugOCRWizardEnabled
        {
            get => _DebugOCRWizardEnabled;
            set
            {
                _DebugOCRWizardEnabled = value;
                OnPropertyChanged(nameof(DebugOCRWizardEnabled));   
            }
        }
        private bool _SaveOCRImages = false;
        public bool SaveOCRImages
        {
            get => _SaveOCRImages;
            set
            {
                _SaveOCRImages = value;
                OnPropertyChanged(nameof(SaveOCRImages));
            }
        }

        private bool _hasHotkeyBeenPressed = false;
        private bool _isAnyGiftsAvailable = false;
        private bool _isBusyProcessingClanChests = false;

        public bool hasHotkeyBeenPressed
        {
            get => _hasHotkeyBeenPressed;
            set
            {
                _hasHotkeyBeenPressed = value;
                OnPropertyChanged(nameof(hasHotkeyBeenPressed));    
            }
        }

        public bool isAnyGiftsAvailable
        {
            get
            {
                return _isAnyGiftsAvailable;
            }
            set
            {
                _isAnyGiftsAvailable = value;
                OnPropertyChanged(nameof(isAnyGiftsAvailable)); 
            }
        }

        public bool isBusyProcessingClanchests
        {
            get => _isBusyProcessingClanChests;
            set
            {
                _isBusyProcessingClanChests = value;
                OnPropertyChanged(nameof(isBusyProcessingClanchests));
            }
        }

        //-- happens incase Settings.json file is repaired or defaulted.
        private bool _requiresOCRWizard = true;
        public bool RequiresOCRWizard
        {
            get => _requiresOCRWizard;
            set
            {
                _requiresOCRWizard = value;
                OnPropertyChanged(nameof(RequiresOCRWizard));
            }
        }

        //-- Should unlock Automation Process.
        private bool _OCRCompleted = false;
        public bool OCRCompleted
        {
            get => _OCRCompleted;
            set
            {
                _OCRCompleted = value;
                OnPropertyChanged(nameof(OCRCompleted));
            }
        }

        private bool _isClanChestDatabaseUpgradeRequired = false;
        public bool IsClanChestDatabaseUpgradeRequired
        {
            get => _isClanChestDatabaseUpgradeRequired;
            set => _isClanChestDatabaseUpgradeRequired = value;
        }

        private bool _isCleaningTessData = false;
        public bool IsCleaningTessData
        {
            get => _isCleaningTessData;
            set => _isCleaningTessData= value;
        }

        public bool HasBuiltChests
        {
            get => _HasBuiltChests.GetValueOrDefault(false);
            set
            {
                _HasBuiltChests = value;
                OnPropertyChanged(nameof(HasBuiltChests));
            }
        }
        public bool HasCollectedChests
        {
            get => _HasCollectedChests.GetValueOrDefault(false);
            set
            {
                _HasCollectedChests = value;
                OnPropertyChanged(nameof(HasCollectedChests));  
            }
        }

        private bool? _UserClosedStartPageDirectly;
        public bool UserClosedStartPageDirectly
        {
            get => _UserClosedStartPageDirectly.GetValueOrDefault(false);
            set
            {
                _UserClosedStartPageDirectly = value;
                OnPropertyChanged(nameof(UserClosedStartPageDirectly));
            }
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

        public void UpdateCurrentProject(string currentProject)
        {
            CurrentProject = currentProject;
        }

        public void UpdateApplicationTitle()
        {
            AppTitle = $"{pAppName} {AppVersionString} - {CurrentProject}";
        }
        public static void ShowDebugMenu(bool state)
        {
            if(Instance == null)
                return;

            Instance.bShowDebugMenu = state;
        }
        public static void RestartApplication(string commandlineargs)
        {
            var selfApplication = System.Reflection.Assembly.GetEntryAssembly().Location;
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.UseShellExecute = false;
            startinfo.RedirectStandardOutput = false;
            startinfo.RedirectStandardError = false;
            startinfo.RedirectStandardInput = false;
            startinfo.CreateNoWindow = true;
            startinfo.FileName = selfApplication;
            startinfo.Arguments = commandlineargs;
            startinfo.WorkingDirectory = AppContext.Instance.AppFolder;
            Process process = new Process();
            process.StartInfo = startinfo;
            process.Start();
            Task.Delay(1000);

            Application.Current.Shutdown();
            return;
        }
        #region AppContext() & Instance
        public static AppContext Instance { get; private set; }

        public AppContext()
        {
            if (Instance == null)
                Instance = this;

        }
        #endregion

    }
}
