using System;

namespace AsLink
{
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
