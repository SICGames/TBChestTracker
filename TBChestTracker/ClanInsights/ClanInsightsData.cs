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
        public List<GameChest> GameChests { get; set; }
        public Dictionary<string, List<ClanChestData>> ChestData { get; set; }

        public ClanInsightsData(string clan, int size, List<GameChest> gameChests, Dictionary<string, List<ClanChestData>> chestData)
        {
            Clan = clan;
            Size = size;
            GameChests = gameChests;
            ChestData = chestData;
        }   
    }
}
