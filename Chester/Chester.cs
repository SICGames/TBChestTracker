using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Web.Http.Headers;

namespace HellSpace
{
    public class Chester
    {
        private Language pLanguage;
        public OcrEngine ocrEngine { get; private set; }
        public Language Language
        {
            get { return pLanguage; }
        }
        public void SetLanguage(string language = "en")
        {
            if (pLanguage == null)
                pLanguage = new Language(language);
        }
        public Chester(string languageCode)
        {
            SetLanguage(languageCode);
            ocrEngine = OcrEngine.TryCreateFromLanguage(Language);
        }
        public Task<OcrResult> GrabTextFromBitmap(SoftwareBitmap bitmap)
        {
            return ocrEngine.RecognizeAsync(bitmap).AsTask();
        }

        public void Destroy()
        {
            ocrEngine = null;
            pLanguage = null;
        }
    }
}