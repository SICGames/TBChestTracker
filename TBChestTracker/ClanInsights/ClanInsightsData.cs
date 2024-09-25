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
        public int Size { get; set; }
        public List<string> Members { get; set; }
        public List<String> GameChests { get; set; }
        public Dictionary<string, IList<ClanChestData>> ChestData { get; set; }
        public bool UsePoints { get; set; }
        public ClanInsightsData(string clan, int size, List<string> members, List<String> gameChests, Dictionary<string, IList<ClanChestData>> chestData, bool usepoints)
        {
            Clan = clan;
            Size = size;
            Members = members;
            GameChests = gameChests;
            ChestData = chestData;
            UsePoints = usepoints;
        }   
    }
}
