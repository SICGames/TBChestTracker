using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [Serializable]
    public class ChestCountData
    {
        public string Clanmate { get; set; }
        public int Count { get; set; }
        public ChestCountData() { }
        public ChestCountData(string clanmate, int count)
        {
            Clanmate = clanmate;
            Count = count;
        }
    }
}
