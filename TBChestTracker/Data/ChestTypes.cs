using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace TBChestTracker
{
    public class ChestTypes
    {
       [Index(0)]
       public string Name { get; set; }
    }
}
