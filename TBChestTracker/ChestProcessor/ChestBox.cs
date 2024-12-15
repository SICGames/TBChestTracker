using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestBox : IDisposable
    {
        public List<string> Content {  get; set; }
        public ChestBox() 
        { 
            Content = new List<string>();
        }
        public void Add(string content)
        {
            if (Content != null)
            {
                Content.Add(content);
            }
        }
        public void Clear()
        {
            Content?.Clear();   
        }

        public void Dispose()
        {
            Clear();
            Content = null;
        }
        public override String ToString()
        {
            string result = string.Empty;
            foreach (var item in Content)
            {
                result += $"{item}\n";
            }
            return result;
        }
    }
}
