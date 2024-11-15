using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public static class DateExtensions
    {

        private static string[] locales =
        {
            "en-US",
            "en-GB",
            "en-AU",
            "fr-FR",
            "it-IT",
            "jp-JP",
            "de-DE",
            "es-SP",
            "es-MX",
            "ru-RU"
        };

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        public static DateTime ConvertToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static double ConvertToUnixTimeStamp(this DateTime dateTime)
        {
            TimeSpan elapsedTime = dateTime - Epoch;
            return elapsedTime.TotalSeconds;
        }

        public static DateTime ParseCulture(this string date)
        {
            DateTime result = new DateTime();
            var successful = false;
            foreach (var culture in locales)
            {
                var cultureInfo = CultureInfo.CreateSpecificCulture(culture);
                var dateformats = cultureInfo.DateTimeFormat.GetAllDateTimePatterns('d');
                foreach (var dateformat in dateformats)
                {
                    if (DateTime.TryParseExact(date, dateformat, new CultureInfo(culture), DateTimeStyles.None, out result))
                    {
                        successful = true; 
                        break;
                    }
                }

                if (successful)
                {
                    break;
                }
            }

            return result;
        }

        public static CultureInfo DetectDateTimeLocale(this DateTime dateTime)
        {
            CultureInfo result = null;

            foreach (var culture in locales)
            {
                var cultureinfo = CultureInfo.CreateSpecificCulture(culture);
                var shortDate = dateTime.ToShortDateString();
                var date = new DateTime();

                var canParse = DateTime.TryParse(shortDate, cultureinfo, DateTimeStyles.AssumeLocal, out date);
                if(canParse)
                {
                    result = cultureinfo;
                    break;
                }
            }

            return result;
        }
    }
}
