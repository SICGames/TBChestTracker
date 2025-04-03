using com.HellStormGames.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Helpers
{
    public class TessResult : IDisposable
    {
        public List<string> Words { get; set; }
        public TessResult()
        {
            Words ??= new List<string>();
        }
        public void Dispose()
        {
            Words.Clear();
            Words = null;
        }
    }
}
