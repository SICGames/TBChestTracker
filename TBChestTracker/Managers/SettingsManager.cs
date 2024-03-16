using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            Settings = new Settings();
            if (Instance == null)
                Instance = this;
        }

        public bool Load(string file = "Settings.json")
        {
            if (File.Exists(file) == false)
                return false;

            using (StreamReader sr = File.OpenText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                Settings = (Settings)serializer.Deserialize(sr, typeof(Settings));
                if (Settings != null)
                {
                    CommandManager.InvalidateRequerySuggested();
                    return true;
                }
                else
                    return false;
            }
        }
        public bool Save(string file = "Settings.json")
        {
            //var saveFilePath = $"Settings.json";
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(file))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(sw, Settings);
                    sw.Close();
                    sw.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
