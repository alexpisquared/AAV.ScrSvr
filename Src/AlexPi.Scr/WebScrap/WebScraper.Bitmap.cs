using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;

namespace WebScrap
{
  public partial class WebScraperBitmap : WebScraper
  {
    public static Bitmap SaveWebImageToFile(string url, string fn) => SaveWebImageToFile(url, fn, false);
    public static Bitmap SaveWebImageToFile(string url, string fn, bool logErrors)
    {
      try
      {
        var bitmap = DownloadBitmap(url);
        if (bitmap == null)
          return null;

        bitmap.Save(fn);//, System.Drawing.Imaging.ImageFormat.Jpeg);
        return bitmap;
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex, MethodInfo.GetCurrentMethod()?.DeclaringType.Name + "." + MethodInfo.GetCurrentMethod()?.Name);
      }

      return null;
    }
    public static Bitmap LoadImageFromFile(string url, bool useOneDrive)
    {
      string fn = GetCachedFileNameFromUrl_NEW(url, useOneDrive);

      if (File.Exists(fn))// && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
      {
        var bitmap = new Bitmap(fn);
        //bitmap.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
        return bitmap;
      }
      else
      {
        return null;
      }
    }
    public static Bitmap DownloadImageCached(string url, bool useOneDrive) => DownloadImageCachedWithExpiry(url, TimeSpan.FromDays(1000), useOneDrive); // image in cache is good forever.
    public static Bitmap DownloadImageCachedWithExpiry(string url, TimeSpan rotTime, bool useOneDrive)
    {
      string fn = WebScraper.GetCachedFileNameFromUrl_NEW(url, useOneDrive);

      if (File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
      {
        try
        {
          Bitmap bitmap = new Bitmap(fn);
          return bitmap;
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message + " - " + url, MethodInfo.GetCurrentMethod()?.DeclaringType.Name + "." + MethodInfo.GetCurrentMethod()?.Name);
          File.Delete(fn); // delete corrupt images
        }
        return new Bitmap(5, 5);
      }
      else
      {
        return SaveWebImageToFile(url, fn);
      }
    }
    public static Bitmap DownloadBitmap(string url)
    {
      try
      {
        Bitmap bitmap = new Bitmap(WebScraper.getProxyAndCreds(new Uri(url)).OpenRead(url));
        Debug.WriteLine($" -- {url} -- {(bitmap == null ? "Failed" : "Success")}");
        return bitmap;
      }
      catch (WebException ex)
      {
        if (!url.Contains("radar"))//ingore radar imgs: may be snow on a hot day....
        {
          Debug.WriteLine(ex.Message + " - " + url, MethodInfo.GetCurrentMethod()?.DeclaringType.Name + "." + MethodInfo.GetCurrentMethod()?.Name);
        }       //ignore: images
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message + " - " + url, MethodInfo.GetCurrentMethod()?.DeclaringType.Name + "." + MethodInfo.GetCurrentMethod()?.Name);
      }

      return null;//success;
    }
  }
}
