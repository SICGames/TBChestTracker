using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class GameChest
    {
        [Index(0)]
        public string ChestType { get; set; }
        [Index(1)]
        public string ChestName { get; set; }
        [Index(2)]
        public bool HasLevel { get; set; }
        [Index(3)]
        public int MinLevel { get; set; }
        [Index(4)]
        public int MaxLevel { get; set; }
        [Index(5)]
        public int IncrementPerLevel { get; set; }
    }
}
