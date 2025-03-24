using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace TBChestTracker
{
    public class CanvasRectangle : ICanvasShape, ISelectableShape, IHoverable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public System.Windows.Media.Brush Color { get; set; }
        public bool IsSelected { get; set; }
        public Brush SelectedColor { get; set; }
        public int Thickness { get; set; }
        
        private bool _hoverable = true;
        public bool CanBeHoverable => _hoverable;
        
        private Brush _hoverOverBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 160, 255));
        public System.Windows.Media.Brush HoverOverColor => _hoverOverBrush;

        public System.Windows.Rect BoundingRect { get; private set; }

        public System.Windows.Shapes.Rectangle RectangleShape { get; set; }
        public void Create(int x, int y, int width, int height, int thickness, Brush color, System.Windows.Shapes.Rectangle shape)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
            IsSelected = false;
            SelectedColor = new SolidColorBrush(Colors.Yellow);
            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, 0, 160, 255);
            Thickness = thickness;
            BoundingRect = new System.Windows.Rect(x, y, width, height);
            RectangleShape = shape;
        }
    }
}
