using AAV.Sys.Ext;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using WebScrap;

namespace RadarPicCollect
{
  public class RadarPicCollector
  {
    public static int GmtOffset => (DateTime.UtcNow - DateTime.Now).Hours;  //4 in summer, 5 in winter
    public static string UrlForModTime(string rsRainOrSnow, DateTime d, string station, bool latest, bool isFallback = false) => EnvCanRadarUrlHelper.GetRadarUrl(d, rsRainOrSnow, station, isFallback);

    static int _stationIndex = 0;
    const int _backLenLive = 25;//usually there is 24 available; (4hr-10min) coverage.
    int _backLenCur = 0; // 300 does not work anymore: there seems to be no access to the historical data - only to immediate last 24 pics/4 hours;//300==48 hours; 240;//40hrs 

    List<PicDetail> _picDtlList = new List<PicDetail>();
    readonly SortedList<string, Bitmap> _urlPicList = new SortedList<string, Bitmap>(StringComparer.CurrentCultureIgnoreCase);

    static readonly string[] _station = { "WKR",								  // king city - RAIN/SNOW
																          "WSO" };                // london    - RAIN/SNOW											
    readonly Point[] _stationOffset = { new Point(0,0),					  // king city - RAIN/SNOW
															          new Point(-292,131) };    // london    - RAIN/SNOW

    public int StationCount => _station.Length;

    public string DownloadRadarPics(int max = 99999)
    {
      _backLenCur = _backLenLive;

      var gmt10 = RoundBy10min(DateTime.UtcNow); // DateTime.Now.AddHours(4));//.AddMinutes(-10 * backLen);//shows 1 hr behind at winter time (Dec2007)  Debug.Assert(DateTime.Now.AddHours(4) == DateTime.UtcNow); //Apr2015

      for (var back = _backLenCur; back >= 0; back--)
      {
        getPic(back, gmt10);
        if (_urlPicList.Count >= max) break;
      }

      return string.Format("   <{0} out of {1} Pics Loaded @{2}>", _picDtlList.Count, _backLenCur, DateTime.Now.ToString("d HH:mm"));
    }
    public string DownloadRadarPics_MostRecent_RAIN_only(int max)
    {
      var gmt10 = RoundBy10min(DateTime.UtcNow); // DateTime.Now.AddHours(4));//.AddMinutes(-10 * backLen);//shows 1 hr behind at winter time (Dec2007)  Debug.Assert(DateTime.Now.AddHours(4) == DateTime.UtcNow); //Apr2015

      for (var times10minBack = 0; times10minBack < _backLenLive && _urlPicList.Count < max; times10minBack++)
        getPic(times10minBack, gmt10, "RAIN");

      return string.Format("   <{0} out of {1} Pics Loaded @{2}>", _picDtlList.Count, _backLenCur, DateTime.Now.ToString("d HH:mm"));
    }

    void getPic(int times10minBack, DateTime gmt10Now, string rainOrSnow = null)
    {
      if (!string.IsNullOrEmpty(rainOrSnow))
        RainOrSnow = rainOrSnow;

#if ___DEBUG //annoying during video watching
      Bpr.BeepFD(11000 + 50 * times10minBack, 40);
#endif 

      try
      {
        _stationIndex = 0;
        var dt = gmt10Now.AddMinutes(-10 * times10minBack);
        var urlLate = UrlForModTime(RainOrSnow, dt, _station[_stationIndex], true);
        var urlHist = UrlForModTime(RainOrSnow, dt, _station[_stationIndex], false);

        if (_urlPicList.ContainsKey(urlLate)) { _urlPicList.Remove(urlLate); Debug.WriteLine(string.Format("+> {0} already there.", urlLate)); }
        // else
        {
          Bitmap pic;
          if (times10minBack < _backLenLive)//
          {
            pic = WebScraperBitmap.DownloadImageCached(urlLate.Split('|')[0], false);
            if (pic != null)
            {
              _urlPicList.Add(urlLate, pic);
              _picDtlList.Add(new PicDetail(pic, dt.AddHours(-4), _station[_stationIndex], _stationOffset[_stationIndex], WebScrap.WebScraper.GetCachedFileNameFromUrl_(urlLate.Split('|')[0], false)));
            }

            report(times10minBack, urlLate, pic, "   WEB");
          }
          else
          {
            pic = WebScraperBitmap.LoadImageFromFile(urlLate.Split('|')[0], false);
            if (pic != null)
            {
              _urlPicList.Add(urlLate, pic);
              _picDtlList.Add(new PicDetail(pic, dt.AddHours(-4), _station[_stationIndex], _stationOffset[_stationIndex], WebScrap.WebScraper.GetCachedFileNameFromUrl_(urlLate.Split('|')[0], false)));
            }
            else
            {
              pic = WebScraperBitmap.LoadImageFromFile(urlHist.Split('|')[0], false);
              if (pic != null)
              {
                _urlPicList.Add(urlLate, pic);
                _picDtlList.Add(new PicDetail(pic, dt.AddHours(-4), _station[_stationIndex], _stationOffset[_stationIndex], WebScrap.WebScraper.GetCachedFileNameFromUrl_(urlHist.Split('|')[0], false)));
              }
              else
              {
                pic = WebScraperBitmap.DownloadImageCached(urlHist.Split('|')[0], false);//GetWebImageFromCache(urlHist.Split('|')[0]);
              }
            }

            report(times10minBack, urlHist, pic, " cache");
          }
        }
      }
      catch (Exception ex) { ex.Log(); }
    }

    static void report(int back, string url_time_, Bitmap pic, string src)
    {
      // Debug.WriteLine(string.Format("{0,3}/{1} {2}: from   {3}: {4}", back, _backLenLive, url_time_.Substring(78), src, pic == null ? "-Unable to get this pic" : "+SUCCESS"));
    }

    public static string RainOrSnow
    {
      get
      {
        if (_rainOrSnow == null)
        {
          var url = string.Format("http://weather.gc.ca/radar/index_e.html?id={0}", _station[_stationIndex]);
          var htm = WebScrap.WebScraper.GetHtmlFromCacheOrWeb(url, TimeSpan.FromHours(6));
          var idxRain = htm.IndexOf("RAIN");
          var idxSnow = htm.IndexOf("SNOW");
          //Debug.WriteLine("html.len:{0}, index: rain:{1}, snow:{2}. \r\n ##/##         UTC Time|LocalTime: from   ", htm.Length, idxRain, idxSnow);
          _rainOrSnow = idxSnow > 0 ? "SNOW" : "RAIN";
        }
        return _rainOrSnow;
      }
      set
      {
        if (_rainOrSnow != value)
        {
          _rainOrSnow = value;
        }
      }
    }

    static string _rainOrSnow = null;//(DateTime.Now.DayOfYear < 72 || DateTime.Now.Month == 12) ? "SNOW" : "RAIN";

    public string DownloadRadarPicsNextBatch(int stationIndex = 0) { _stationIndex = stationIndex; return DownloadRadarPics(); }

    public List<PicDetail> Pics
    {
      get => _picDtlList;
      set => _picDtlList = value;
    }
    public int IdxTime(DateTime time)
    {
      for (var i = 0; i < _picDtlList.Count; i++)
      {
        if (_picDtlList[i].ImageTime == time)
          return i;
      }
      return -1;
    }
    public PicDetail Time(DateTime time)
    {
      foreach (var pd in _picDtlList)
      {
        if (pd.ImageTime == time)
          return pd;
      }
      return null;
    }
    public static DateTime RoundBy10min(DateTime dt)
    {
      dt = dt.AddTicks(-dt.Ticks % (10 * TimeSpan.TicksPerMinute));
      return dt;
    }
    public static TimeSpan RoundBy10min(TimeSpan dt)
    {
      dt = dt.Add(TimeSpan.FromTicks(-dt.Ticks % (10 * TimeSpan.TicksPerMinute)));
      return dt;
    }

    public static Bitmap GetBmpLclTime(DateTime lcltime)
    {
      var gmt = RoundBy10min(lcltime.AddHours(GmtOffset));
      return GetBmp(gmt);
    }
    public static Bitmap GetLatestBmp()
    {
      var gmtNow = RoundBy10min(DateTime.UtcNow);
      Debug.Assert((int)((DateTime.UtcNow - DateTime.Now.AddHours(RadarPicCollector.GmtOffset)).TotalHours) == 0);

      try
      {
        for (var i = 0; i < 18; i++) // in the last 3 hours.
        {
          var dt = gmtNow.AddMinutes(-10 * i);

          var bmp = RadarPicCollector.GetBmp(dt);
          if (bmp != null)
            return bmp;
        }
      }
      catch (Exception ex) { ex.Log(); }

      return null;
    }
    public static Bitmap GetBmp(DateTime gmtTime, string station = "WSO")
    {
      if (_cache.ContainsKey(gmtTime)) return _cache[gmtTime];

      //string rainOrSnow = (DateTime.Now.DayOfYear < 70 || DateTime.Now.Month == 12) ? "SNOW" : "RAIN";//todo: maybe temperature based.
      Bitmap bmp = null;

      try
      {
        if (bmp == null) bmp = getFromCacheOrWeb(UrlForModTime(RainOrSnow, gmtTime, "WKR", true, false).Split('|')[0]);
        if (bmp == null) bmp = getFromCacheOrWeb(UrlForModTime(RainOrSnow, gmtTime, "WKR", true, true).Split('|')[0]);
        if (bmp == null) bmp = getFromCacheOrWeb(UrlForModTime(RainOrSnow, gmtTime, "WSO", true, false).Split('|')[0]);
        if (bmp == null) bmp = getFromCacheOrWeb(UrlForModTime(RainOrSnow, gmtTime, "WSO", true, true).Split('|')[0]);
      }
      catch (Exception ex) { ex.Log(); }
      finally
      {
        if (bmp != null && !_cache.ContainsKey(gmtTime)) _cache.Add(gmtTime, bmp);
      }

      return bmp;
    }

    static Bitmap getFromCacheOrWeb(string url)
    {
      Bitmap bmp = null;
      bmp = WebScraperBitmap.LoadImageFromFile(url, useOneDrive: false); if (bmp != null) return bmp;
      bmp = WebScraperBitmap.DownloadImageCached(url, useOneDrive: false); if (bmp != null) return bmp;
      return bmp;
    }

    static readonly SortedList<DateTime, Bitmap> _cache = new SortedList<DateTime, Bitmap>();

    public static string GetRainForecastReport()
    {
      var report = "";
      var gmtNow = RadarPicCollector.RoundBy10min(DateTime.UtcNow);

      //Debug.Assert((int)((DateTime.UtcNow - DateTime.Now.AddHours(RadarPicCollector._GmtOffset)).TotalHours) == 0);

      try
      {
        const int max = 2;
        var tm = new DateTime[max];
        var mh = new double[max];
        for (int j = max - 1, i = 0; i < 72 && j >= 0; i++)
        {
          var dt = gmtNow.AddMinutes(-10 * i);

          var bmp = RadarPicCollector.GetBmp(dt);
          if (bmp != null)
          {
            tm[j] = dt.AddHours(-RadarPicCollector.GmtOffset);
            mh[j] = PicMea.CalcMphInTheArea(bmp, dt);
            j--;
          }
        }

        var dMh = mh[0] - mh[max - 1];
        var dTm = tm[0] - tm[max - 1];
        var kh = dMh / dTm.TotalHours;
        var tmNow = DateTime.Now;
        var mhNow = mh[0] + kh * (tmNow - tm[0]).TotalHours;
        var tm30m = DateTime.Now.AddMinutes(30);
        var mh30m = mh[0] + kh * (tm30m - tm[0]).TotalHours;

        var f = "  {0:HH:mm}:  {1:N3} \n";
        report += string.Format("{0:MMM d:}\n", tmNow);
        report += string.Format(f, tm[0], mh[0]);
        report += string.Format(f, tm[1], mh[1]);
        report += string.Format(f, tmNow, mhNow);
        report += string.Format(f, tm30m, mh30m);
        if (dMh <= 0)
          report += string.Format("More is coming.");
        else
          report += string.Format("End at {0:HH:mm} or in {1:N1} hours.", tmNow.AddHours(-mhNow / kh), (-mhNow / kh));
      }
      catch (Exception ex)
      {
        report = ex.Message;
      }

      return report;
    }
  }

  public class EnvCanRadarUrlHelper
  {
    public static DateTime RoundBy10min(DateTime dt) => dt.AddTicks(-dt.Ticks % (10 * TimeSpan.TicksPerMinute));
    public static string GetRadarUrl(DateTime d, string rsRainOrSnow = "RAIN", string station = "WKR", bool isFallbackCAPPI = false, bool? isDark = null, bool isFallbackCOMP = false)
    {
      isDark = isDark ?? DateTime.Now.Hour < 8 || 20 <= DateTime.Now.Hour;

      ///todo: on error try this _COMP_
      var cmp = isFallbackCOMP ? "_COMP" : "";
      var cap = isFallbackCAPPI ? "CAPPI" : "PRECIP";
      var drk = isDark == true ? "" : "detailed/";

      var url = $"https://weather.gc.ca/data/radar/{drk}temp_image//{station}/{station}{cmp}_{cap}_{rsRainOrSnow}_{d.Year}_{d.Month:0#}_{d.Day:0#}_{d.Hour:0#}_{d.Minute:0#}.GIF";

      //var mdl = $"https://weather.gc.ca/data/radar/detailed/temp_image//WKR/WKR_COMP_PRECIP_RAIN_2019_07_24_15_00.GIF";      Debug.Assert(url.Substring(0, 84) == mdl.Substring(0, 84), $">>:: actual {url}\r\n>>::  model {mdl}\r\n");

      return url;
    }
  }

}