using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Helpers;
using com.HellStormGames.OCR;
using com.HellStormGames.Diagnostics;
using com.HellStormGames.Diagnostics.Logging;

using System.Diagnostics;
using Emgu.CV.Dai;
using System.Windows;
using CsvHelper.Configuration.Attributes;
using System.Runtime.InteropServices;
using MS.WindowsAPICodePack.Internal;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace TBChestTracker.Engine
{

    public class OCREngine
    {
        //public static OCREngine Instance { get; private set; }
        public Tessy OCR { get; private set; }
        public static OCREngine Instance { get; private set; }
        //public TessyWords[] WordCollection { get; private set; }

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
                    OcrEngineMode ocrmode = OcrEngineMode.LstmOnly;

                    if(ocrSettings.TessDataConfig.Prefix.Equals("_best") || ocrSettings.TessDataConfig.Prefix.Equals("_fast"))
                    {
                        ocrmode = OcrEngineMode.LstmOnly;
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
                        OCR.Init(tessdata, languages, OcrEngineMode.LstmOnly);
                        OCR.PageSegmentMode = PageSegMode.PSM_AUTO_ONLY;
                        OCR.SetSourceResolution(600);

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

            Loggio.Info($"Tesseract ({Tessy.Version}) successfully initialized.");
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
                    Loggio.Warn("Screenshot bitmap is null. Shouldn't be null.");
                    return null;
                }

                //-- AccessViolationException -- Correupted Memory sometimes.
                System.Drawing.Imaging.BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                int height = image.Height;
                var width = image.Width;    
                var stride = data.Stride;
                
                Byte[] bytes = new Byte[stride * height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                OCR.SetImage(bytes, (uint)width, (uint)height, 4, ((uint)4 * (uint)width));
                image.UnlockBits(data);

                if (OCR.Recognize() == 0)
                {
                    var resultstr = OCR.GetUTF8Text();
                    if (String.IsNullOrEmpty(resultstr))
                    {
                        Loggio.Warn("OCR Result Text shouldn't be null or empty.");
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
                Loggio.Error(e, "OCR Read Issue", "There's a issue with the OCR Reading. More details are provided.");
                throw new Exception(e.Message, e);
            }
        }

        public TessyWords[] GetWords(System.Drawing.Bitmap bitmap)
        {
            try
            {
                if (bitmap == null)
                {
                    Loggio.Warn("Screenshot bitmap is null. Shouldn't be null.");
                    throw new ArgumentNullException(nameof(bitmap));
                }

                TessyWords[] WordCollection = null;

                //-- AccessViolationException -- Correupted Memory sometimes.
                System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int height = bitmap.Height;
                var width = bitmap.Width;
                var stride = data.Stride;
                int channels = 3;
                Byte[] bytes = new byte[stride * height];

                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    channels = 4;
                }
                else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    channels = 1;
                }
                else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    channels = 3;
                }

                var ppi = channels * (uint)width;
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                OCR.SetImage(bytes, (uint)width, (uint)height, (uint)channels, ((uint)channels * (uint)width));

                bitmap.UnlockBits(data);

                if (OCR.Recognize() == 0)
                {
                    WordCollection = OCR.GetWords();
                    
                    return WordCollection;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Loggio.Error(e, "OCR Read Issue", "There's a issue with the OCR Reading. More details are provided.");
                throw new Exception(e.Message, e);
            }
        }

        public bool RenderOCRResults(TessyWords[] wordcollection, WriteableBitmap writeablebitmap, AOIRect areaofinterest, int penthickness = 3, int offset = 8)
        {
            if (wordcollection == null)
            {
                throw new ArgumentNullException(nameof(wordcollection));
            }
            if (writeablebitmap == null)
            {
                throw new ArgumentNullException(nameof(writeablebitmap));
            }
            if (wordcollection.Length > 0 && wordcollection != null)
            {
                //--- render the letters obtained from Tesseract
                foreach (var word in wordcollection)
                {
                    var boundingbox = word.BoundingBox;
                    var region = new System.Drawing.Rectangle(boundingbox.X,
                        boundingbox.Y,
                        boundingbox.Width, boundingbox.Height);

                    region.Offset(offset, offset);

                    var letterX = region.X + areaofinterest.x;
                    var letterY = region.Y + areaofinterest.y;
                    var letterWidth = (int)letterX + region.Width + 2;
                    var letterHeight = (int)letterY + region.Height + 2;

                    for (var stoke_thickness = 0; stoke_thickness < penthickness; stoke_thickness++)
                    {
                        writeablebitmap.DrawRectangle((int)letterX--, (int)letterY--, letterWidth++, letterHeight++, Colors.Green);
                    }
                }
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            if (OCR != null)
            {
                OCR.Shutdown();
                OCR = null;
            }
        }

    }
}
