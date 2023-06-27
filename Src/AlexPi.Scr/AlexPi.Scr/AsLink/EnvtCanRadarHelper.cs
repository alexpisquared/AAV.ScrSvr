using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace AsLink.Standard.Helpers
{
  public class EnvtCanRadarHelper
  {
    ///https://weather.gc.ca/data/radar/         temp_image//WKR/WKR_PRECIP_RAIN_2018_07_28_16_30.GIF  -- dark
    ///https://weather.gc.ca/data/radar/detailed/temp_image//WKR/WKR_PRECIP_RAIN_2018_07_28_16_30.GIF  -- white
    public static DateTime RoundBy10min(DateTime t, int intervalMin) => t.AddTicks(-t.Ticks % (intervalMin * TimeSpan.TicksPerMinute));
    public static string GetRadarUrl(DateTime utc, bool isRain = true, bool isDark = false, string station = "WKR", bool isFallback = false) =>
        $"https://weather.gc.ca/data/radar/" +
        $"{(isDark ? "" : "detailed/")}" +
        $"temp_image//{station}/{station}_" +
        $"{(isFallback ? "CAPPI" : "PRECIP")}_" +
        $"{(isRain ? "RAIN" : "SNOW")}_" +
        $"{utc.Year}_{utc.Month:0#}_{utc.Day:0#}_{utc.Hour:0#}_{utc.Minute:0#}.GIF";

    public static async Task<(bool success, string report)> DoesImageExistRemotely(string url)     //, string mimeType = "image/gif")
    {
      var request = (HttpWebRequest)WebRequest.Create(url);
      request.Method = "HEAD";
      var report = "";

      try
      {
        using (var x = await request.GetResponseAsync())
        {
          var response = (HttpWebResponse)(x);
          if (response.StatusCode == HttpStatusCode.OK)         // && response.ContentType == mimeType)
            return (true, report);
        }
      }
      catch (WebException ex) { report = ex.Message; }
      catch (Exception ex) { report = ex.Message; if (Debugger.IsAttached) Debugger.Break(); }

      return (false, report);
    }
  }
}