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
        public static SettingsManager Instance { get; private set; }

        public SettingsManager()
        {
            if (Instance == null)
                Instance = this;


            var settingsPathFolder = AppContext.Instance.LocalApplicationPath;
            var di = new DirectoryInfo(settingsPathFolder);
            if (di.Exists == false)
            {
                di.Create();
            }
            Settings = new Settings();

            try
            {
                bool loadResult = Load();
            }
            catch (Exception ex)
            {

            }

            if(Settings == null)
            {
                //-- it got corrupted 
                Settings = new Settings();
            }

            var aoiHeight = Settings.OCRSettings.SuggestedAreaOfInterest.height;
            var aoiWidth = Settings.OCRSettings.SuggestedAreaOfInterest.width;
            var claimButtonsSize = Settings.OCRSettings.ClaimChestButtons.Count;

            AppContext.Instance.RequiresOCRWizard = (aoiHeight > 0 && aoiWidth > 0) ? false : true;
            AppContext.Instance.OCRCompleted = (aoiHeight > 0 && aoiWidth > 0 && claimButtonsSize > 0) ? true : false;

            Save();
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
