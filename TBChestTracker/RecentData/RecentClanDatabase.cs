using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class RecentClanDatabase
    {
        
        public string ClanAbbreviations { get; set; }
        public string ClanName { get; set; }
        public string ShortClanRootFolder { get; set; }
        public string FullClanRootFolder { get; set; }
        public string LastOpened { get; set; }
    }
}
