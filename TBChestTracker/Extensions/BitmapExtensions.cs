using com.HellStormGames.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TBChestTracker.Extensions
{
    public static class BitmapExtensions
    {
        public static System.Drawing.Bitmap AsBitmap(this BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                throw new ArgumentNullException(nameof(bitmapSource));

            try
            {
                System.Drawing.Bitmap result = null;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(ms);
                    result = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(ms);
                    encoder = null;
                    ms.Close();
                    ms.Dispose();
                }
                return result;
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Bitmap Extensions", "Issue converting BitmapSource to Bitmap");
                return null;
            }
        }
        public static BitmapSource AsBitmapSource(this System.Drawing.Bitmap bitmap)
        {
            try
            {
                if(bitmap == null)
                {
                    throw new ArgumentNullException(nameof(bitmap));
                }

                BitmapImage result = null;
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    result = new BitmapImage();
                    result.BeginInit();
                    ms.Position = 0;
                    result.StreamSource = ms;
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.EndInit();
                    ms.Close();
                    ms.Dispose();
                }
                return (BitmapSource)result;
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Bitmap Extensions", "Issue converting Bitmap to BitmapSource");
                return null;
            }
        }
    }
}
