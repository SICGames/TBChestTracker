using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

namespace TBChestTracker
{
    [System.Serializable]
    public class OCRSettings : IDisposable
    {
        private bool disposedValue;

        public AOIRect AreaOfInterest { get; set; }
        public AOIRect SuggestedAreaOfInterest { get; set; }
        public List<Point> ClaimChestButtons {  get; set; }    
        public OCRSettings() 
        { 
            AreaOfInterest = new AOIRect();
            SuggestedAreaOfInterest = new AOIRect();
            ClaimChestButtons = new List<Point>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    ClaimChestButtons.Clear();
                    ClaimChestButtons = null;
                    SuggestedAreaOfInterest.Dispose();
                    AreaOfInterest.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OCRSettings()
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
