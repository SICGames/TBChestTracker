using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ChestDailyData
    {
        public string Date { get; set; }
        public List<ClanChestData> ChestData { get; set; }
        public ChestDailyData()
        {

        }
        public ChestDailyData(string date, List<ClanChestData> chestData)
        {
            Date = date;
            ChestData = chestData;
        }
    }
}
