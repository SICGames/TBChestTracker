using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using Emgu.CV.OCR;

namespace TBChestTracker.Helpers
{
    public class TesseractHelper
    {
        private static Tesseract _tesseract;
        public static Tesseract GetTesseract() => _tesseract;
        public static Task<StringBuilder> LoadAllLanguagesAsync(string tessdata_path) => Task.Run(()=>LoadAllLanguages(tessdata_path));
        public static StringBuilder LoadAllLanguages(string tessdata_path)
        {
            if(!System.IO.Directory.Exists(tessdata_path))
            {
                throw new Exception($"No folder 'tessdata' exists. Ensure it exists.");
            }

            var tessdatafiles = System.IO.Directory.GetFiles(tessdata_path, "*.traineddata");
            StringBuilder languages = new StringBuilder();
            for (var x = 0; x < tessdatafiles.Length - 1; x++)
            {
                var file = tessdatafiles[x];
                file = file.Substring(file.LastIndexOf(@"\") + 1);
                var language = file.Substring(0, file.LastIndexOf('.'));
                if (x != tessdatafiles.Length - 1)
                    languages.Append($"{language}+");
                else
                    languages.Append($"{language}");
            }
            tessdatafiles = null;
            return languages;
        }
        public static Task<bool> InitAsync(string tessdata_path, string langauges) => Task.Run(()=> Init(tessdata_path, langauges));    
        public static bool Init(string tessdata_path, string languages)
        {
            try
            {
                if(_tesseract == null)
                _tesseract = new Tesseract(tessdata_path, languages, OcrEngineMode.Default);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            return true;
        }
        public static TessResult Read(IInputArray image)
        {
            try
            {
                if (image == null)
                    return null;

                _tesseract.SetImage(image);
                _tesseract.Recognize();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            var resultstr = _tesseract.GetUTF8Text();
            resultstr = resultstr.Replace("\r\n", ",");
            string[] results = resultstr.Split(',');
            List<String> ocrResults = new List<string>();
            foreach (var r in results.ToList())
            {
                if (!String.IsNullOrEmpty(r))
                {
                    ocrResults.Add(r);
                }
            }
            TessResult result = new TessResult();
            result.Words = new List<string>();
            result.Words = ocrResults.ToList();
            results = null;
            ocrResults = null;
            resultstr = string.Empty;

            return result;
        }
        public static void Destroy()
        {
            if( _tesseract != null )
            {
                _tesseract.Dispose();
                _tesseract = null;
            }
           
        }
    }
}
