﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Helpers;
using com.HellStormGames.TesseractOCR;
using com.HellStormGames.TesseractOCR.Collections;
using com.HellStormGames.Diagnostics;
using com.HellStormGames.Diagnostics.Logging;

using System.Diagnostics;
using Emgu.CV.Dai;
using System.Windows;

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
                        if (!System.IO.Directory.Exists(tessdata))
                        {
                            Loggio.Warn($"No folder TessData Path ('{tessdata}') exists. Ensure it exists.");
                            return false;
                        }

                        if(languages == null)
                        {
                            MessageBox.Show("There was a problem with initializing Tesseract. View recent log file for more information.");
                            return false;
                        }
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
                        Loggio.Error(ex, "",$"Failed to initialize Tesseract.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //-- OCR failed.
                Loggio.Error(ex, "", "Exception occured while attempting initialize Tesseract.");
                return false;
            }

            Loggio.Info($"Tesseract ({OCR.Version}) successfully initialized.");
            return true;
        }

        public Task<StringBuilder> LoadAllLanguagesAsync(OCRSettings settings) => Task.Run(() => LoadAllLanguages(settings));
        public StringBuilder LoadAllLanguages(OCRSettings settings)
        {
            var tessdata_path = settings.TessDataFolder;

            if (!System.IO.Directory.Exists(tessdata_path))
            {
                Loggio.Warn($"No folder TessData Path ('{tessdata_path}') exists. Ensure it exists.");
                return null;
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
