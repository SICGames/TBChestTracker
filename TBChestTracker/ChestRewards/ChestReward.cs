using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [Serializable]
    public class ChestReward
    {
        public string ChestType {  get; set; }
        public int ChestLevel { get; set; }
        public string Reward { get; set; }
        public ChestReward(string chestType, int chestLevel, string reward)
        {
            ChestType = chestType;
            ChestLevel = chestLevel;
            Reward = reward;
        } 

    }
}
