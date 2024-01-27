using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    [Serializable]
    public class ChestTypeData
    {
        public string Chest { get; set; }
        public int Total { get; set; }
        public ChestTypeData() { }
        public ChestTypeData(string Chest, int total)
        {
            this.Chest = Chest;
            this.Total = total;
        }
    }

    [Serializable]
    public class ChestCountData
    {
        public string Clanmate { get; set; }
        public List<ChestTypeData> ChestTypes { get; set; }

        public int Count { get; set; }
        public ChestCountData() 
        { 
            ChestTypes = new List<ChestTypeData>();
        }
        public ChestCountData(string clanmate, int count)
        {
            Clanmate = clanmate;
            ChestTypes = new List<ChestTypeData>();

            Count = count;
        }
    }
}
