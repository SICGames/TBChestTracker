using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanChestReport
    {
        public string Date { get; set; }
        public string Time { get; set; } 
        public List<ClanChestData> ChestData { get; set; }

        public ClanChestReport() 
        { 
        }
    }
}
