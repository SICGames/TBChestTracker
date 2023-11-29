using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    [Serializable]
    public class Chest
    {
        public string Name { get; set; }
        public ChestType Type { get; set; }
        public int Level { get; set; }
        public Chest(string name, ChestType type, int level)
        {
            Name = name;
            Type = type;
            Level = level;
        }
    }
}
