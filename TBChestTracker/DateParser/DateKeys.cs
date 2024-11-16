using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class DateKeys : IDisposable
    {
        private Dictionary<string, int> _DateIndices = new Dictionary<string, int>();
        public int GetIndice(string datepart) 
        { 
            return _DateIndices[datepart]; 
        }
        public void SetIndice(string datepart, int value)
        {
            _DateIndices[datepart] = value;
        }

        public void Dispose()
        {
            _DateIndices.Clear();
            _DateIndices = null;
        }

        public DateKeys()
        {
            _DateIndices.Clear();
            _DateIndices.Add("month", -1);
            _DateIndices.Add("day", -1);
            _DateIndices.Add("year", -1);
        }
        public DateKeys(int month_index,  int day_index, int year_index)
        {
            _DateIndices.Clear();
            _DateIndices.Add("month", month_index);
            _DateIndices.Add("day", day_index);
            _DateIndices.Add("year", year_index);
        }

    }
}
