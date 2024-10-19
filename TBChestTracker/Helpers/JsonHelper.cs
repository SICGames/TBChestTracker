using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

        //-- attempts to load json file, if fails returns false. If loaded, returns true.
        public static bool TryLoad<T>(string filepath, out T data)
        {
            try
            {
                using (StreamReader sr = File.OpenText(filepath))
                {
                    var d = sr.ReadToEnd();
                    data = JsonConvert.DeserializeObject<T>(d);
                    sr.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                data = default(T);
                return false;
            }
        }
    }
}
