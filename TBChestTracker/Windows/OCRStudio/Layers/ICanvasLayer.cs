using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public interface ICanvasLayer
    {
        public string Name { get; set; }
        public ICanvasShape Shape { get; set; }
    }
}
