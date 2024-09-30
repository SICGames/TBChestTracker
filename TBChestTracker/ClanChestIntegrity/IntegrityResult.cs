using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class IntegrityResult : IDisposable
    {
        public Dictionary<string, int> Errors { get; set; }
        public IntegrityResult() 
        { 
            Errors = new Dictionary<string, int>();
        }
        
        public void Add(string name, int value)
        {
            bool exists = Errors.ContainsKey(name);
            if (exists)
            {
                Errors[name] = value;
            }
            else
            {
                Errors.Add(name, value);
            }
        }

        public void Dispose()
        {
            if (Errors != null)
            {
                Errors.Clear();
            }
            Errors = null;
        }
    }
}
