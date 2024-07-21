using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TBChestTracker.Localization
{
    public static class LocalizationManager 
    {
        public static void Set(string Language = "en-US")
        {
            CultureInfo.CurrentCulture = new CultureInfo(Language, false);
            CultureInfo.CurrentUICulture = new CultureInfo(Language, false);    
            Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
        }
    }
}
