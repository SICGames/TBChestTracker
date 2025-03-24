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
using Newtonsoft.Json.Linq;
using com.HellStormGames.Logging;
using com.HellStormGames.Diagnostics;


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
                Loggio.Error(ex, "Settings Manager", "Issue occured while attempting to load Settings.json.");

            }

            if(Settings == null)
            {
                //-- it got corrupted 
                Settings = new Settings();
            }
        }

        //-- Used to load clan. 
        //-- returns null if there's an issue or nothing needs to be done. 
        //-- returns AOIRect if obtained old SuggestedAreaOfInterest from Settings File.
        public AOIRect ObtainObseleteAOIRect(string file = "Settings.json")
        {
            var filePath = $"{AppContext.Instance.LocalApplicationPath}{file}";
            if (File.Exists(filePath) == false)
            {
                Loggio.Warn("Settings Manager", $"{filePath} doesn't exist.");
                return null;
            }

            AOIRect AOIRect = null;
            using (var sr = new StreamReader(filePath))
            {
                var jsonString = sr.ReadToEnd();
                var jObject = JObject.Parse(jsonString);
                foreach (var JProperty in jObject.Properties())
                {
                    var item = (JObject)JProperty.Value;
                    if (item.Path == "OCRSettings")
                    {
                        var ocrSettingsObject = item;
                        foreach (var settingsProperties in ocrSettingsObject.Properties())
                        {
                            if (settingsProperties.Path == "OCRSettings.SuggestedAreaOfInterest")
                            {
                                var AOIObject = settingsProperties;
                                var token = jObject.SelectToken("OCRSettings.SuggestedAreaOfInterest");
                                AOIRect = token.ToObject<AOIRect>();
                            }
                            if(settingsProperties.Path == "OCRSettings.ClaimChestButtons")
                            {
                                var claimbuttonsToken = jObject.SelectToken("OCRSettings.ClaimChestButtons");
                                var claimButtons = claimbuttonsToken.ToObject<List<string>>();
                                var claimButtonsArray = claimButtons[0].Split(','); 
                                int x, y;
                                x = y = 0;
                                Int32.TryParse(claimButtonsArray[0], out x);  
                                Int32.TryParse(claimButtonsArray[1], out y);

                                AOIRect.ClickTarget = new System.Drawing.Point(x,y);
                                break;
                            }
                        }
                        if (AOIRect != null)
                        {
                            break;
                        }
                    }
                }
                sr.Close();
            }
            return AOIRect;
        }

        private bool Load(string file = "Settings.json")
        {
            var filePath = $"{AppContext.Instance.LocalApplicationPath}{file}";
            if (File.Exists(filePath) == false)
                return false;

            using (StreamReader sr = new StreamReader(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                
                Settings = (Settings)serializer.Deserialize(sr, typeof(Settings));
                CommandManager.InvalidateRequerySuggested();
                sr.Close();
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
