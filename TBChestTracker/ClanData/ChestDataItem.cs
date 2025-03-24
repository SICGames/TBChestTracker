using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ChestDataItem
    {
        //public string Clanmate { get; set; }
        public IList<Chest> chests { get; set; }
        public int Points { get; set; }
        public ChestDataItem()
        {
            chests = new List<Chest>();
        }
        public ChestDataItem(string name, IList<Chest> chests)
        {
          //  Clanmate = name;
            this.chests = chests;
        }
        public ChestDataItem(string name, IList<Chest> chests, int points)
        {
            //this.Clanmate = name;
            this.chests = chests;
            this.Points = points;
        }
    }
}
