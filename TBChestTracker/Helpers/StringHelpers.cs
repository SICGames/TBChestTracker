using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class StringHelpers
    {
        public static string truncate_file_name(string filename, int maxlength)
        {
            var result = filename.Length <= maxlength ? filename : $"...{filename.Substring(maxlength)}";
            return result;
        }
        public static List<int> findOccurances(string text, char chr)
        {
            int occured = 0;
            List<int> position = new List<int>();
            for(var i = 0; i < text.Length; i++)
            {
                if (text[i] == chr)
                { 
                    occured++; 
                    position.Add(i);
                }
            }
            return position;
        }
        public static int findNthOccurance(string text, char chr, int nth)
        {
            int occured = 0;
            for(var i = 0; i < text.Length; i++)
            {
                if (text[i] == chr) 
                { 
                    occured++; 
                }
                if(occured == nth)
                    return i;
            }
            return -1;
        }
        public static string ConvertToUTF8(string text)
        {
            return Convert(text, 65001);
        }
        public static string Convert(string text, int encodingId)
        {
            return Encoding.UTF8.GetString(Encoding.GetEncoding(encodingId).GetBytes(text));

        }
    }
}
