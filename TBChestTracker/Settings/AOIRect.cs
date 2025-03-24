using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class AOIRect : IDisposable
    {
        private bool disposedValue;

        public double x {  get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public Point ClickTarget { get; set; }

        public AOIRect() 
        { 
            x = 0;
            y = 0;
            width = 0;
            height = 0;
        }
        public AOIRect(double x, double y, double width, double height, Point ClickLocation)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.ClickTarget = ClickLocation;
        }
        public AOIRect(Point location, SizeF size, Point ClickLocation)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
            this.ClickTarget = ClickLocation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                  
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AOIRect()
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
