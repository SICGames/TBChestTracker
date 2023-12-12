using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace TBChestTracker
{
    [System.Serializable]
    public class TranslationResult
    {
        public string TranslationText { get; set; }
    }

    public class Translator
    {
        public static TranslationResult Translate(string text, string source_language, string target_language)
        {
            TranslationResult result = new TranslationResult(); 
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={source_language}&tl={target_language}&dt=t&q={text}";
            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            if(response.StatusCode == HttpStatusCode.OK)
            { 
                using(StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    var json_data = sr.ReadToEnd();

                    var json = JsonConvert.SerializeObject(json_data);

                    //Debug.WriteLine(json);

                    sr.Dispose();
                }
             
            }
            response.Close();
            response.Dispose();
            httpWebRequest = null;
            return result;
        }
    }
}
