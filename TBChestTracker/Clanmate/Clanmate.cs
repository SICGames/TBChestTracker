using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class Clanmate
    {
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public Clanmate() 
        { 
            Name = string.Empty;
            Aliases = new List<string>();
        }
        public Clanmate(string name) {
            Name = name; 
            Aliases = new List<string>();
        }
        public Clanmate(string name, List<string> aliases) : this(name)
        {
            Aliases = aliases;
        }
    }
}
