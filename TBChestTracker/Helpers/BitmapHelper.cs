using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TBChestTracker
{
    public class BitmapHelper
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static BitmapSource ConvertFromBitmap(System.Drawing.Bitmap bitmap, int dpiX = 96, int dpiY = 96)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;    
                var BitmapImage = new BitmapImage();
                BitmapImage.BeginInit();
                BitmapImage.StreamSource = memory;
                BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                BitmapImage.EndInit();
                BitmapImage.Freeze();
                
                return BitmapImage;
            }
        }
        public static System.Drawing.Bitmap ConvertFromBitmapSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap = null;
            using (var memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                encoder.Save(memory);
                bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memory);
            }
            return bitmap;
        }
    }
}
