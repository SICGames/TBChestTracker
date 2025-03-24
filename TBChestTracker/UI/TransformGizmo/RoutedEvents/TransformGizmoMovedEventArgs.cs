using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker.UI
{
    public class TransformGizmoMovedEventArgs : RoutedEventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Spacing { get; set; }
        public TransformGizmoMovedEventArgs(RoutedEvent routed_event, double x, double y, double spacing) : base(routed_event)
        {
            X = x;
            Y = y;
            Spacing = spacing;
        }
    }
}
