using com.HellStormGames.Diagnostics;
using CsvHelper;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Data
{
    public class OcrLanguages : IDisposable
    {
        private string languageFile => $"{AppContext.Instance.AppFolder}Data\\OcrLanguages\\Languages.csv";
        public List<OcrLanguage> Languages { get; set; }
        public OcrLanguages() 
        { 
            Languages = new List<OcrLanguage>();
        }
        public bool Load()
        {
            Loggio.Info("Ocr Language Loader", $"Attempting to load '{languageFile}'");
            if(System.IO.File.Exists(languageFile) == false) 
                return false;

            try
            {
                using (StreamReader sr = new StreamReader(languageFile))
                {
                    using (CsvReader csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        Languages.Clear();
                        Languages = csv.GetRecords<OcrLanguage>().ToList();
                        sr.Close();
                    }
                }
                Loggio.Info("Ocr Language Loader", $"Successfully loaded '{languageFile}'");
                return true;
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Loading Ocr Languages failed", "There was an exception caught. If there is any provided information, share with developer.");
                return false;
            }
        }

        public void Dispose()
        {
            Languages?.Clear();
            Languages  = null;
        }
    }
}
