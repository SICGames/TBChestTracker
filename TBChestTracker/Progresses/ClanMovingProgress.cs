using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanMovingProgress
    {
        public string? ClanName {  get; set; }
        public string? File {  get; set; }
        public double? Percent {  get; set; }
        public bool? isCompleted { get; set; }
        public ClanMovingProgress(string clanName, string file, double? percent, bool? isCompleted)
        {
            ClanName = clanName;
            File = file;
            Percent = percent;
            this.isCompleted = isCompleted;
        }

    }
}
