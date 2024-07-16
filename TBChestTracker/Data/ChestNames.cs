using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestNames
    {
        [Index(0)]
        public string Name { get; set; }    
    }
}
