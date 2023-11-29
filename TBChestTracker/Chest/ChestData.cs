using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    [System.Serializable]
    public class ChestData
    {
        public string Clanmate { get; set; }
        public Chest Chest { get; set; }
        public ChestData(string clanmate, Chest chest)
        {
            Clanmate = clanmate;
            Chest = chest;
        }
    }

}
