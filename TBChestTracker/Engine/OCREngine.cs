using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Helpers;
using com.HellStormGames.TesseractOCR;
using com.HellStormGames.TesseractOCR.Collections;

using System.Diagnostics;
using Emgu.CV.Dai;

namespace TBChestTracker.Engine
{

    public class OCREngine
    {
        //public static OCREngine Instance { get; private set; }
        public Tessy OCR { get; private set; }
        public static OCREngine Instance { get; private set; }
        public OCREngine()
        {
          Instance = this;
        }
        public Task<bool> InitAsync(OCRSettings settings) => Task.Run(() => Init(settings));
        public bool Init(OCRSettings ocrSettings)
        {
            try
            {
                if (OCR == null)
                {
                    OcrEngineMode ocrmode = OcrEngineMode.Lstm;

                    if(ocrSettings.TessDataConfig.Prefix.Equals("_best") || ocrSettings.TessDataConfig.Prefix.Equals("_fast"))
                    {
                        ocrmode = OcrEngineMode.Lstm;
                    }

                    var languages = String.Empty;
                    if(ocrSettings.Languages.ToLower().Contains("all"))
                    {
                        languages = LoadAllLanguages(ocrSettings).ToString();
                    }
                    else
                    {
                        languages = ocrSettings.Languages;
                    }
                    try
                    {
                        var tessdata = $@"{ocrSettings.TessDataFolder}\";
                        
                        OCR = new Tessy();

                        //-- accessviolationexception being tossed because of OcrEngineMode.LstmOnly
                        OCR.Init(tessdata, languages, OcrEngineMode.Lstm);
                        OCR.SetPageSegmentation(PageSegmentationMode.PSM_AUTO);
                        //OCR.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789:-' ");
                        //OCR.SetVariable("load_system_dawg", "F");
                        //OCR.SetVariable("load_freq_dawg", "F");
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Failed to initialize Tesseract. Reason given: => {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                //-- OCR failed.
                // com.HellStormGames.Logging.Console.Write($"{ex.Message}", "OCR Init", com.HellStormGames.Logging.LogType.ERROR);
                return false;
            }

            // com.HellStormGames.Logging.Console.Write($"Tesseract ({OCR.Version}) Initialized Successfully", "OCR Init", com.HellStormGames.Logging.LogType.INFO);
            return true;
        }

        public Task<StringBuilder> LoadAllLanguagesAsync(OCRSettings settings) => Task.Run(() => LoadAllLanguages(settings));
        public StringBuilder LoadAllLanguages(OCRSettings settings)
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

        public Task<TessResult> ReadAsync(System.Drawing.Bitmap image) => Task.Run(() => Read(image));

        public TessResult Read(System.Drawing.Bitmap image)
        {
            try
            {
                if (image == null)
                {
                    return null;
                }

                //-- AccessViolationException -- Correupted Memory sometimes.
                System.Drawing.Imaging.BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
                image.PixelFormat);
                OCR.SetImage(image, 600);
                image.UnlockBits(data); 

                if (OCR.Recongize() == 0)
                {
                    var resultstr = OCR.GetUTF8Text();
                    if (String.IsNullOrEmpty(resultstr))
                    {
                        throw new Exception("Tesseract OCR UTF8 Text isn't Suppose To be empty.");
                    }
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
                else 
                    return null;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public void Destroy()
        {
            if (OCR != null)
            {
                OCR.Destroy();
                OCR = null;
            }
        }

    }
}
