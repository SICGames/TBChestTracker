using Emgu.CV.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Helpers.Extensions
{
    public static class ListExtension
    {
        public static void Filter(this Tesseract.Word[] src, string[] values)
        {
            var filtered = src.Where(w => ContainsAny(w.Text, values));
            src = filtered.ToArray();
        }
        private static bool ContainsAny(string str, IEnumerable<string> values)
        {
            if (!string.IsNullOrEmpty(str) || values.Any())
            {
                foreach (string value in values)
                {
                    var bExists = str.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) != -1;
                    if (bExists)
                        return true;
                }
            }
            return false;
        }

    }
}
