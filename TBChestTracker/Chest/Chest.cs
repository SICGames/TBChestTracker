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
        public string Type { get; set; }
        public string Source { get; set; }
        public int Level { get; set; }
        public Chest(string name, string type, string source, int level)
        {
            Name = name;
            Type = type;
            Source = source;
            Level = level;
        }
    }
}
