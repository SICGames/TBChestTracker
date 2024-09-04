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

            var settingsPathFolder = AppContext.Instance.CommonAppFolder;
            if(!System.IO.Directory.Exists(settingsPathFolder))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(settingsPathFolder);
                }
                catch(System.IO.IOException ex) 
                { 
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool Load(string file = "Settings.json")
        {
            var filePath = $"{AppContext.Instance.CommonAppFolder}{file}";
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
            var savePath = $"{AppContext.Instance.CommonAppFolder}{file}";
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


        public void BuildDefaultConfiguration()
        {
            if(Settings == null) 
                Settings = new Settings();

            Settings.OCRSettings.CaptureMethod = "GDI+";
            Settings.OCRSettings.GlobalBrightness = 0.65;
            Settings.OCRSettings.Tags = new ObservableCollection<string>(new List<string> { "Chest", "From", "Source", "Gift" });
            Settings.GeneralSettings.ClanRootFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TotalBattleChestTracker\\";
            Settings.OCRSettings.TessDataFolder = $@"{AppContext.Instance.TesseractData}";
            Settings.OCRSettings.Languages = "eng+tur+ara+spa+chi_sim+chi_tra+kor+fra+jpn+rus+pol+por+pus+ukr+deu";
            Settings.HotKeySettings.StartAutomationKeys = "F9";
            Settings.HotKeySettings.StopAutomationKeys = "F10";
            Settings.OCRSettings.PreviewImage = String.Empty;
            Settings.OCRSettings.Threshold = 85;
            Instance.Settings.OCRSettings.MaxThreshold = 255;

            Save();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Settings.Dispose();
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
