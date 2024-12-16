using Emgu.CV.BgSegm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public String Date { get; private set; }

        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }    

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

        private int isValidYear(string year)
        {
            int y = -1;
            Int32.TryParse(year, out y);

            bool bIsValidYear = false;
            //-- increment until we fail.
            while (true)
            {
                try
                {
                    DateTime test = new DateTime(y,1, 1);
                    bIsValidYear = true;
                    y++;
                }
                catch
                {
                    //-- exception occured, we failed.
                    break;
                }
            }

            if (bIsValidYear)
            {
                return 0;
            }

            return -1;
        }
        //--- returns -1 for false and 0 for true.
        private int isValidMonth(string month)
        {
            int m = -1;
            Int32.TryParse(month, out m);
            
            bool bIsValidMonth = false;
            //-- increment until we fail.
            while (true)
            {
                try
                {
                    DateTime test = new DateTime(1970, m, 1);
                    m++;
                }
                catch
                {
                    //-- exception occured, we failed.
                    if(m <= 12)
                    {
                        bIsValidMonth = true;
                    }
                    else
                    {
                        bIsValidMonth= false;
                    }
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
            bool bIsValidDay = false;
            //-- increment until we fail.
            while (true)
            {
                try
                {
                    DateTime test = new DateTime(1970, 1, d);
                    d++;
                }
                catch
                {
                    //-- exception occured, we failed.
                    if(d <= 31)
                    {
                        bIsValidDay = true;
                    }
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

            var current_part_index = 0;
            var month_index = -1;
            var year_index = -1;
            var day_index = -1;

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

            Date = datestring;
            var parts = datestring.Split(seperator[0]);
            
            //-- is correct date format?
            if (seperator == Culture.DateTimeFormat.DateSeparator)
            {
                DateTime date_result;
                var canBeParsed = DateTime.TryParse(datestring, Culture, DateTimeStyles.None, out date_result);
                if (canBeParsed)
                {
                    //-- if we arrive here, very good stuff.
                    //-- it means the date format is correct. 
                    //-- de-DE => dd.M.yyyy
                    var date_format_parts = date_format.Split(seperator[0]);

                    var day_pos = datestring.IndexOf(date_result.Day.ToString());
                    var month_pos = datestring.IndexOf(date_result.Month.ToString());   
                    var year_pos = datestring.IndexOf(date_result.Year.ToString()); 
                    var date_length = datestring.Length;
                    int day_indice, month_indice, year_indice;
                    day_indice = month_indice = year_indice = -1;

                    //-- really need to not assume and take guesses. year can come first.
                    //-- maybe needs better coding. 
                    //-- Sweden Format - 2024-12-1 - 2/0/1 (Date Indices)
                    //-- German Format - 1-12-2024 - 1/0/2
                    //-- America Format - 12/1/2024 - 0/1/2
                    
                    for(int indice =0; indice < date_format_parts.Length; indice++)
                    {
                        if (date_format_parts[indice].ToLower().Equals("yyyy"))
                        {
                            year_indice = indice;
                            continue;
                        }
                        else if (date_format_parts[indice].ToLower().Equals("m") || date_format_parts[indice].ToLower().Equals("mm"))
                        {
                            month_indice = indice;
                            continue;
                        }
                        else if (date_format_parts[indice].ToLower().Equals("dd") || date_format_parts[indice].ToLower().Equals("d"))
                        {
                            day_indice = indice;
                            continue;
                        }
                    }
                    
                    SetDateKeys(new DateKeys(month_indice, day_indice, year_indice));
                    int y, m, d;
                    Int32.TryParse(parts[year_indice], out y);
                    Int32.TryParse(parts[month_indice], out m);
                    Int32.TryParse(parts[day_indice], out d);
                    Year = y;
                    Month = m;
                    Day = d;

                    return true;
                }
            }

            return false;
            /*
              Will have to work on a better approach in future. 

            foreach (var part in parts)
            {
                if (month_index < 0)
                {
                    if (isValidMonth(part) == 0)
                    {
                        month_index = current_part_index;
                        current_part_index++;
                        continue;
                    }
                }
                if(day_index < 0)
                {
                    if(isValidDay(part) == 0)
                    {
                        day_index = current_part_index;
                        current_part_index++;
                        continue;
                    }
                }
                if (year_index < 0)
                {
                    if (isValidYear(part) == 0)
                    {
                        year_index = current_part_index;
                        current_part_index++;
                        continue;
                    }
                }

            }
            int yearStr, monthStr, dayStr;
            Int32.TryParse(parts[year_index], out yearStr);
            Int32.TryParse(parts[month_index], out monthStr);
            Int32.TryParse(parts[day_index], out dayStr);
            Year = yearStr;
            Month = monthStr;
            Day = dayStr;

            try
            {
                DateTime date = new DateTime(yearStr, monthStr, dayStr);
                SetDateKeys(new DateKeys(month_index, day_index, year_index));
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
            */

        }
    }
}
