using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    [Serializable]
    public class ChestExportData
    {
        public string Clanmate { get; set; }
        public Dictionary<string, int> ExtraHeaders { get; set; }
        public int Total { get; set; }   

        public ChestExportData() 
        { 
            ExtraHeaders = new Dictionary<string, int>();  
        }
    }
}
