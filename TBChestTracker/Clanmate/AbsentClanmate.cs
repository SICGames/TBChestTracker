using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class AbsentClanmate
    {
        public string Clanmate { get; set; }
        public string LastActivityDate
        {
            get
            {
                if (ActivityDates.Count == 0 || ActivityDates == null) return String.Empty;
                else
                {
                    return ActivityDates.First();
                }
            }
        }

        public List<string> ActivityDates { get; set; }
        public AbsentClanmate(string clanmate, List<string> activityDates)
        {
            Clanmate = clanmate;
            ActivityDates = activityDates;
        }
    }
}
