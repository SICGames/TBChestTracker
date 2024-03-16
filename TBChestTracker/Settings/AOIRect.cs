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
        public List<Marker> Markers = new List<Marker>();

        private void BuildMarkers()
        {
            for(int i = 0; i < 4; i++)
                Markers.Add(new Marker());


            var markerHalf = Markers[0].Size / 2;
            
            Markers[0].x = x - markerHalf;
            Markers[0].y = y - markerHalf;
            Markers[1].x = width - markerHalf;
            Markers[1].y = y - markerHalf;
            Markers[2].x = x - markerHalf;
            Markers[2].y = height - markerHalf;
            Markers[3].x = width - markerHalf;
            Markers[3].y = height - markerHalf;

            /*
            var marker_top_left = new Point(x - markerHalf, y - markerHalf);
            var marker_top_right = new Point(width - 16, y - 16);
            var marker_bottom_left = new Point(x - 16, height - 16);
            var marker_bottom_right = new Point(width - 16, height - 16);
            */
        }
        public AOIRect() 
        { 
            x = 0;
            y = 0;
            width = 0;
            height = 0;
        }
        public AOIRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            BuildMarkers();
        }
        public AOIRect(Point location, SizeF size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
            BuildMarkers();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Markers.Clear();
                    Markers = null;
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
