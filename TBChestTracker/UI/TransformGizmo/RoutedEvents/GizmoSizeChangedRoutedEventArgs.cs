using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker.UI.TransformGizmo.RoutedEvents
{
    public class GizmoSizeChangedRoutedEventArgs : RoutedEventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Int32 Spacing { get; set; }
        public GizmoSizeChangedRoutedEventArgs(RoutedEvent routedEvent, double x, double y, double width, double height, Int32 spacing) : base(routedEvent)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Spacing = spacing;
        }
    }
}
