using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ClanInsightsData
    {
        public string Clan {  get; set; }  
        public IList<string> Members { get; set; }
        public List<String> GameChests { get; set; }
        public Dictionary<string, Dictionary<string, ChestDataItem>> ChestData { get; set; }
        public bool UsePoints { get; set; }
        public string DateFormat { get; set; }
        public string Locale { get; set; }

        public ClanInsightsData(string clan, IList<String> members, List<String> gameChests, Dictionary<string, Dictionary<String, ChestDataItem>> chestdata, bool usepoints, string dateformat, string locale )
        {
            Clan = clan;
            Members = members;
            GameChests = gameChests;
            ChestData = chestdata;
            UsePoints = usepoints;
            DateFormat = dateformat;
            Locale = locale;
        }   
    }
}
