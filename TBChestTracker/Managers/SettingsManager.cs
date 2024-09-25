using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft;
using Newtonsoft.Json;


namespace TBChestTracker
{
    public class SettingsManager : IDisposable
    {
        private bool disposedValue;

        public Settings Settings { get; private set; }
        public Settings DefaultSettings { get; private set; }
        public static SettingsManager Instance { get; private set; }
        
        public SettingsManager() 
        {
            if (Instance == null)
                Instance = this;


            var settingsPathFolder = AppContext.Instance.LocalApplicationPath;
            var di = new DirectoryInfo(settingsPathFolder);
            if(di.Exists == false)
            {
                di.Create();
            }

            //-- configure default settings.
            DefaultSettings = new Settings();
            DefaultSettings.GeneralSettings.ClanRootFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TotalBattleChestTracker\\";
            DefaultSettings.GeneralSettings.UILanguage = "English";
            
            DefaultSettings.OCRSettings.CaptureMethod = "GDI+";
            DefaultSettings.OCRSettings.GlobalBrightness = 0.65;
            DefaultSettings.OCRSettings.ClanmateSimilarity = 90;
            DefaultSettings.OCRSettings.Tags = new ObservableCollection<string>(new List<string> { "Chest", "From", "Source", "Gift", "Contains" });
            DefaultSettings.OCRSettings.TessDataFolder = $@"{AppContext.Instance.TesseractData}";
            DefaultSettings.OCRSettings.Languages = "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus+pol+por+pus+ukr+deu";
            DefaultSettings.OCRSettings.PreviewImage = String.Empty;
            DefaultSettings.OCRSettings.Threshold = 85;
            DefaultSettings.OCRSettings.MaxThreshold = 255;

            DefaultSettings.HotKeySettings.StartAutomationKeys = "F9";
            DefaultSettings.HotKeySettings.StopAutomationKeys = "F10";

            DefaultSettings.AutomationSettings.AutomationClicks = 4;
            DefaultSettings.AutomationSettings.AutomationScreenshotsAfterClicks = 1250;
            DefaultSettings.AutomationSettings.AutomationDelayBetweenClicks = 100;
            DefaultSettings.AutomationSettings.StopAutomationAfterClicks = 0;


            if (AppContext.Instance.IsFirstRun)
            {
                Settings = new Settings();
                Settings = DefaultSettings;
                Save();
            }
            else
            {
                try
                {
                    bool result = Load();
                    if (result == false)
                    {
                        Settings = new Settings();
                        Settings = DefaultSettings;
                        Save();
                    }
                    else
                    {
                        if (Settings.OCRSettings.TessDataFolder == null)
                        {
                            //--- definately damaged.
                            System.IO.File.Delete($"{settingsPathFolder}Settings.json");
                            if (Settings == null)
                            {
                                Settings = new Settings();

                            }
                            Settings = DefaultSettings;
                            Save();
                        }

                        if(Settings.AutomationSettings.AutomationClicks == 0 && Settings.AutomationSettings.AutomationScreenshotsAfterClicks == 0 && Settings.AutomationSettings.AutomationDelayBetweenClicks == 0)
                        {
                            Settings.AutomationSettings.AutomationClicks = DefaultSettings.AutomationSettings.AutomationClicks;
                            Settings.AutomationSettings.AutomationDelayBetweenClicks = DefaultSettings.AutomationSettings.AutomationDelayBetweenClicks;
                            Settings.AutomationSettings.AutomationScreenshotsAfterClicks = DefaultSettings.AutomationSettings.AutomationScreenshotsAfterClicks;
                            Save();
                        }

                     }
                }
                catch (Exception ex)
                {
                    //-- it's possible it's missing;
                    throw new Exception("Something happened in SettingsManager");
                }
                var aoiHeight = Settings.OCRSettings.SuggestedAreaOfInterest.height;
                var aoiWidth = Settings.OCRSettings.SuggestedAreaOfInterest.width;
                var claimButtonsSize = Settings.OCRSettings.ClaimChestButtons.Count;

                AppContext.Instance.RequiresOCRWizard = (aoiHeight > 0 && aoiWidth > 0) ? false : true;
                AppContext.Instance.OCRCompleted = (aoiHeight > 0 && aoiWidth > 0 && claimButtonsSize > 0) ? true : false;
            }
        }

        private bool Load(string file = "Settings.json")
        {
            var filePath = $"{AppContext.Instance.LocalApplicationPath}{file}";
            if (File.Exists(filePath) == false)
                return false;

            using (StreamReader sr = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                
                if(Settings != null)
                    Settings.Dispose();

                Settings = new Settings();

                Settings = (Settings)serializer.Deserialize(sr, typeof(Settings));
                CommandManager.InvalidateRequerySuggested();
                return true;
            }
        }
        public bool Save(string file = "Settings.json")
        {
            //var saveFilePath = $"Settings.json";
            var savePath = $"{AppContext.Instance.LocalApplicationPath}{file}";
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(savePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(sw, Settings);
                    sw.Close();
                    sw.Dispose();
                }

            }
            catch (IOException ex)
            {
                if (MessageBox.Show($"You'll need adminstration privileges to save settings file. Run Application as Administration.") == MessageBoxResult.OK)
                    return false;
            }

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Settings.Dispose();
                    DefaultSettings.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SettingsManager()
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
