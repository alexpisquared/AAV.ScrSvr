using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AsLink.Standard.Helpers
{
  public class EnvtCanRadarHelper
  {
    ///https://weather.gc.ca/data/radar/         temp_image//WKR/WKR_PRECIP_RAIN_2018_07_28_16_30.GIF  -- dark
    ///https://weather.gc.ca/data/radar/detailed/temp_image//WKR/WKR_PRECIP_RAIN_2018_07_28_16_30.GIF  -- white
    public static DateTime RoundBy10min(DateTime t) => t.AddTicks(-t.Ticks % (10 * TimeSpan.TicksPerMinute));
    public static string GetRadarUrl(DateTime utc, bool isRain = true, bool isDark = false, string station = "WKR", bool isFallback = false) =>
        $"https://weather.gc.ca/data/radar/" +
        $"{(isDark ? "" : "detailed/")}" +
        $"temp_image//{station}/{station}_" +
        $"{(isFallback ? "CAPPI" : "PRECIP")}_" +
        $"{(isRain ? "RAIN" : "SNOW")}_" +
        $"{utc.Year}_{utc.Month:0#}_{utc.Day:0#}_{utc.Hour:0#}_{utc.Minute:0#}.GIF";

    public static async Task<bool> DoesImageExistRemotely(string url)     //, string mimeType = "image/gif")
    {
      var request = (HttpWebRequest)WebRequest.Create(url);
      request.Method = "HEAD";

      try
      {
        using (var x = await request.GetResponseAsync())
        {
          var response = (HttpWebResponse)(x);
          if (response.StatusCode == HttpStatusCode.OK)         // && response.ContentType == mimeType)
            return true;
        }
      }
      catch (WebException ex)
      {
        Debug.Write($"{url} - {ex.Message} ");
        using (WebResponse response = ex.Response)
        {
          var httpResponse = (HttpWebResponse)response;
          Debug.Write($" StatusCode: {httpResponse.StatusCode} ");
          using (var stream = response.GetResponseStream())
          using (var reader = new StreamReader(stream))
          {
            string text = reader.ReadToEnd();
            Debug.WriteLine(text);
          }
        }
      }
      catch (Exception ex) { Debug.WriteLine($"{url} - {ex}"); if (Debugger.IsAttached) Debugger.Break(); }

      return false;
    }
  }
}