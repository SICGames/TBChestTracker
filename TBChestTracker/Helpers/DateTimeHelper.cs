using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TBChestTracker
{
    public class DateTimeHelper
    {

        public static DateTime GetGlobalizedCurrentShortDateTime()
        {
            try
            {
                return DateTime.Parse(DateTime.Now.ToShortDateString(), new CultureInfo(CultureInfo.CurrentCulture.Name));
            }
            catch(Exception ex)
            {
                return DateTime.MinValue;
            }
        }
        
        public static DateTime FormatDateTimeByString(string datetimeformat)
        {
            try
            {
                return DateTime.Parse(datetimeformat, new CultureInfo(CultureInfo.CurrentCulture.Name));
            }
            catch(Exception ex)
            {
                return DateTime.MinValue;
            }
        }
    }
}
