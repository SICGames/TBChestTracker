using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ViewModels
{
    public class OCRPreviewView
    {
        public string ChestName { get; set; }
        public string From {  get; set; }
        public string Source { get; set; }
        public string Contains { get; set; }
        public OCRPreviewView(string chestName, string from, string source, string contains)
        {
            ChestName = chestName;
            From = from;
            Source = source;
            Contains = contains;
        }   
    }
}
