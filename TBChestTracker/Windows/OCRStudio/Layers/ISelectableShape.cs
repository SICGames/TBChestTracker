using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace TBChestTracker
{
    public interface ISelectableShape
    {
        public bool IsSelected { get; set; }
        public Brush SelectedColor { get; set; }
    }
}
