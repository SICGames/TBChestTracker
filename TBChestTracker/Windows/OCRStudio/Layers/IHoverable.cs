using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace TBChestTracker
{
    public interface IHoverable
    {
        public bool CanBeHoverable { get; }
        public Brush HoverOverColor {  get; }
    }
}
