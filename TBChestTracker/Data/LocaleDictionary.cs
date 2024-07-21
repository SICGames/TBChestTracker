using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    //--- only used when loading locale Strings then forgotten about after dictionary is created.
    public class LocaleDictionary
    {
        [Index(0)]
        public string Key { get; set; }
        [Index(1)]
        public string Value { get; set; }
    }
}
