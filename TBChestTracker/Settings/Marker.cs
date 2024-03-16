using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class Marker : IDisposable
    {
        public double x;
        public double y;
        private double _size = 32;
        public double Size
        {
            get { return _size; }
            set { _size = value; }
        }
        private bool disposedValue;
        public Marker()
        {
            x = 0;
            y = 0;
        }
        public Marker(double x, double y, double size)
        {
            this.x = x;
            this.y = y;
            Size = size;
        }
        public Marker(PointF point, double size)
        {
            x= point.X;
            y= point.Y;
            Size = size;
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
        // ~Marker()
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
