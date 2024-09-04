using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.OCR;

namespace TBChestTracker.Engine
{
    public class OCREngine
    {
        public static OCREngine Instance { get; private set; }
        public Tesseract OCR { get; private set; }
        public OCREngine()
        {
            if (Instance == null)
                Instance = this;
        }
        public bool Init(OCRSettings ocrSettings)
        {
            if (OCR == null)
            {
                OCR = new Tesseract();
            }
            return true;
        }
    }
}
