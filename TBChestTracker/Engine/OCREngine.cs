using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.OCR;

using TBChestTracker.Helpers;
using System.Diagnostics;
using Emgu.CV.Structure;

namespace TBChestTracker.Engine
{
    public enum TessDataOption
    {
        None,
        Best,
        Fast
    };

    public class OCREngine
    {
        public static OCREngine Instance { get; private set; }
        public static Tesseract OCR { get; private set; }
        public OCREngine()
        {
            if (Instance == null)
                Instance = this;
        }
        public static Task<bool> InitAsync(OCRSettings settings,TessDataOption option) => Task.Run(() => Init(settings,option));
        public static bool Init(OCRSettings ocrSettings, TessDataOption option)
        {
            try
            {
                if (OCR == null)
                {
                    var enginemode = option == TessDataOption.None ? 
                        OcrEngineMode.Default : 
                        (option == TessDataOption.Best ? 
                        OcrEngineMode.LstmOnly : OcrEngineMode.TesseractOnly);

                    OCR = new Tesseract();
                    OCR.PageSegMode = PageSegMode.SparseTextOsd;
                    
                    OCR.Init($@"{ocrSettings.TessDataFolder}\", ocrSettings.Languages, enginemode);
                    var psm = OCR.PageSegMode;

                }
            }
            catch (Exception ex)
            {
                //-- OCR failed.
                com.HellStormGames.Logging.Console.Write($"{ex.Message}", "OCR Init", com.HellStormGames.Logging.LogType.ERROR);
                return false;
            }

            com.HellStormGames.Logging.Console.Write($"Tesseract ({Tesseract.VersionString}) Initialized Successfully", "OCR Init", com.HellStormGames.Logging.LogType.INFO);
            return true;
        }

        public static Task<StringBuilder> LoadAllLanguagesAsync(OCRSettings settings) => Task.Run(() => LoadAllLanguages(settings));
        public static StringBuilder LoadAllLanguages(OCRSettings settings)
        {
            var tessdata_path = settings.TessDataFolder;

            if (!System.IO.Directory.Exists(tessdata_path))
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

        public static TessResult Read(IInputArray image)
        {
            try
            {
                if (image == null)
                {
                    return null;
                }

                //OCR.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                OCR.SetVariable("load_system_dawg", "false");
                OCR.SetVariable("load_freq_dawg", "false");

                //-- AccessViolationException -- Correupted Memory sometimes.
                OCR.SetImage(image);
                OCR.Recognize();

                var resultstr = OCR.GetUTF8Text();
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
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public static void Destroy()
        {
            if (OCR != null)
            {
                OCR.Dispose();
                OCR = null;
            }
        }

    }
}
