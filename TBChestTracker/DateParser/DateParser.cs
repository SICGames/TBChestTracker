using Emgu.CV.BgSegm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    //--- will expand on this more.

    public struct DateParser
    {
        private DateKeys _keys = new DateKeys();
        private CultureInfo _cultureInfo;
        public void CreateCulture(string cultureName = "")
        {
            if (!String.IsNullOrEmpty(cultureName)) {
                _cultureInfo = CultureInfo.CreateSpecificCulture(cultureName);
            }
            else
            {
                _cultureInfo = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            }
        }
        private CultureInfo Culture => _cultureInfo;

        #region Set/Get DateKeys
        //-- Date Keys 
        /*
         11/12/2024 
         11 - month key - 0 indice
         12 - day key - 1 indice
         2024 - year key - 2 indice
         
         12/11/2024 (en_GB)
         1 - indice (Month)
         0 - Indice (Day)
         2 - Indice (Year)
        */
        public DateKeys GetDateKeys() { return _keys; }
        public void SetDateKeys(DateKeys keys)
        {
            _keys = keys;
        }
        #endregion

        #region Constructor
        public DateParser() 
        {
            CreateCulture();
        }
        public DateParser(string language)
        {
            CreateCulture(language);
        }

        #endregion

        //--- returns -1 for false and 0 for true.
        private int isValidMonth(string month)
        {
            int m = -1;
            Int32.TryParse(month, out m);

            if (m > 12)
            {
                return -1;
            }

            bool bIsValidMonth = false;
            //-- increment until we fail.
            while (true)
            {
                try
                {
                    DateTime test = new DateTime(1970, m, 1);
                    bIsValidMonth = true;
                    m++;
                }
                catch
                {
                    //-- exception occured, we failed.
                    break;
                }
            }

            if (bIsValidMonth)
            {
                return 0;
            }

            return -1;
        }
        //--- returns -1 for false and 0 for true.
        private int isValidDay(string day)
        {
            int d = -1;
            Int32.TryParse(day, out d);

            if (d > 31)
            {
                return -1;
            }

            bool bIsValidDay = false;
            //-- increment until we fail.
            while (true)
            {
                try
                {
                    DateTime test = new DateTime(1970, 1, d);
                    bIsValidDay = true;
                    d++;
                }
                catch
                {
                    //-- exception occured, we failed.
                    break;
                }
            }

            if (bIsValidDay)
            {
                return 0;
            }

            return -1;
        }

        public bool IsDateFormatValid(string datestring)
        {
            if (string.IsNullOrEmpty(datestring))
            {
                throw new ArgumentNullException(nameof(datestring));    
            }

            var date_seperator = Culture.DateTimeFormat.DateSeparator;
            var date_format = Culture.DateTimeFormat.ShortDatePattern;

            /*
              if culture is en-US but the string is in de-DE or something else. We need to return false. 
              Detecting what culture it may be would be tedious. There are times where TryParseExact in a foreach loop returns incorrect culture.

              en-GB date format => dd/mm/yyyy => 11/12/2024
              en-US date format => M/d/yyyy  => 12/11/2024
              de-DE date format => dd.mm.yyyy => 11.12.2024

              Sometimes DateTime.TryExtract doesn't do it's job. en-GB and en-US often get confused. 
              We obtain the date string as it is.  
              Break it apart by it's seperator.
              test each date key by incrementing by 1. 
              Month can not go past 12. Day can not go past 31. 
            */
            var seperator = String.Empty;
            if (datestring.Contains("/"))
            {
                seperator = "/";
            }
            else if (datestring.Contains("-"))
            {
                seperator = "-";
            }
            else if(datestring.Contains("."))
            {
                seperator = ".";
            }
            if(seperator.Equals(date_seperator) == false)
            {
                return false;
            }

            var parts = datestring.Split(seperator[0]);
            var current_part_index = 0;
            var month_index = -1;
            var year_index = -1;
            var day_index = -1;

            foreach (var part in parts)
            {
                if (month_index < 0)
                {
                    if (isValidMonth(part) == 0)
                    {
                        month_index = current_part_index;
                        continue;
                    }
                }
                if(day_index < 0)
                {
                    if(isValidDay(part) == 0)
                    {
                        day_index = current_part_index;
                        continue;
                    }
                }

                current_part_index++;
            }
            return false;
        }
    }
}
