using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using CefSharp.DevTools.CSS;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TBChestTracker.Effects
{
    public class ImageEffects
    {
        public ImageEffects() 
        { 

        }
        public static Mat ConvertToGrayscale(Mat source, bool save=false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().Mat;
            if(save)
            {
                result.Save($@"{outputFolder}\OCR_Gray.png");
            }
            return result;
        }
        public static Mat Brighten(Mat source, double value, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().Mul(value) + value;
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_Brightened.png");
            }
            return result.Mat;
        }

        public static Mat Erode(Mat source, int iterations, bool save = false, string outputFolder= "")
        {
            var result = source.ToImage<Gray, byte>().Erode(iterations);
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_Eroded.png");
            }
            return result.Mat;
        }

        public static Mat MedianBlur(Mat source, int value, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().SmoothMedian(value);
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_MedianBlurred.png");
            }
            return result.Mat;
        }

        public static Mat BlurGaussian(Mat source, int value, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().SmoothGaussian(value);
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_GaussianBlur.png");
            }
            return result.Mat;
        }

        public static Mat Blur(Mat source, int value, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().SmoothBlur(value, value);
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_Blur.png");
            }
            return result.Mat;
        }

        public static Mat Resize(Mat source, int size, Emgu.CV.CvEnum.Inter interlopation, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().Resize(size, interlopation).Mat;
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_Reized.png");
            }
            return result;
        }

        public static Mat ThresholdBinaryInv(Mat source, Gray threshold, Gray maxThreshold, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().ThresholdBinaryInv(threshold, maxThreshold).Mat;
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_ThresholdBinaryInv.png");
            }
            return result;
        }

        public static Mat Dilate(Mat source, int iterations, bool save = false, string outputFolder = "")
        {
            var result = source.ToImage<Gray, byte>().Dilate(iterations).Mat;
            if (save)
            {
                result.Save($@"{outputFolder}\OCR_Dilate.png");
            }
            return result;
        }

        public static Image<Gray, byte> ToGrayscaleImage(Mat source)
        {
            return source.ToImage<Gray,byte>(); 
        }
    }
}
