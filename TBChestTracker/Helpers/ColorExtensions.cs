using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TBChestTracker
{
    public static class ColorExtensions
    {
        public static SolidColorBrush FromHex(this String hex)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
        }
    }
}
