﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace TBChestTracker
{
    [System.Serializable]
    public class Settings : IDisposable
    {
        private bool disposedValue;

        public GeneralSettings GeneralSettings { get; private set; }
        public OCRSettings OCRSettings { get; set; }
        public HotKeySettings HotKeySettings { get; set; }
        public AutomationSettings AutomationSettings { get; set; }
        public Settings() 
        {
            OCRSettings = new OCRSettings();
            GeneralSettings = new GeneralSettings();
            HotKeySettings = new HotKeySettings();
            AutomationSettings = new AutomationSettings();  
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    HotKeySettings = null;
                    OCRSettings = null;
                    GeneralSettings = null;

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Settings()
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
