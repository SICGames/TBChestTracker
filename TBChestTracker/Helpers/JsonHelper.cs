using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class JsonHelper
    {
        public static bool isJson(string json)
        {
                try
                {
                    JObject.Parse(json);
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
        }
    }
}
