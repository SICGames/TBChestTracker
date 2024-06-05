using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class DateTimeHelper
    {

        public static DateTime GetCurrentDate()
        {
            var timezone = TimeZone.CurrentTimeZone;
            DateTime dateTime = DateTime.Now;

            return dateTime;
        }
    }
}
