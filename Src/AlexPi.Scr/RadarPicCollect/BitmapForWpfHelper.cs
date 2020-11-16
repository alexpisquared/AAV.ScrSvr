using AAV.Sys.Ext;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Radar
{
    public class BitmapForWpfHelper
    {
        public static async Task<BitmapSource> BitmapToBitmapSource(Bitmap bmp)
        {
            //testCheckForRainyPixels(bmp);

            var intptr = bmp.GetHbitmap();
            // bmp.Dispose(); // not tested (Jul 2018)
            try { return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); }
            catch (OutOfMemoryException ex) { ex.Log("10 sec pause..."); await Task.Delay(9999); } // a feeble attempt to let the GC do its bidding.
            catch (Exception ex) { ex.Log(); }
            finally { DeleteObject(intptr); }
            return null;
        }

        static void testCheckForRainyPixels(Bitmap bmp)
        {
            var noRainColor = Color.FromArgb(255, 153, 153, 102);//"#ff999966");//, ARGB=(255, 153, 153, 102)}"	System.Drawing.Color

            var hml = new List<Color>();
            int ttlArea = 0;
            for (int x0 = 250, x = -3; x <= 3; x++)
            {
                for (int y0 = 260, y = -3; y <= 3; y++)
                {
                    ttlArea++;
                    var pixelColor = bmp.GetPixel(x0 + x, y0 + y);
                    if (!noRainColor.Equals(pixelColor))
                        hml.Add(pixelColor);
                }
            }
            Debug.WriteLine(string.Format("hml.Count:{0} out of ttlArea:{1}", hml.Count, ttlArea));
            //Color c7 = source.GetPixel(255, 250); //Global Ofc
        }

        ///    Monday, March 10, 2008 4:26 AM by Tolgahan Albayrak 
        ///Sometimes (when you need to re-create some bitmaps or change them) it will crash the memory.. 
        ///This is a small fix for your sollution..
        ///getHBitmap will allocate memory by total image size.. And each call will allocate again.. So if you delete old hBitmap after creating BitmapSource, I think, the RAM will feel better :)
        [DllImport("gdi32")] static extern int DeleteObject(IntPtr o);
    }
}
