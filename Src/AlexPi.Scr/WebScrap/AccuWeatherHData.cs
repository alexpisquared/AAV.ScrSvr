using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebScrap
{
  internal enum Site
  {
    NorthYork = 0,
    SaubleBch = 1
  };

  public class AccuWeatherHData
  {
    const string _urf = @"http://vortex.accuweather.com/adc2010/images/icons-numbered/{0}-h.png";

    //                   http://vortex.accuweather.com/adc2010/images/icons-{0}/{1}-h.png";
    //                   http://vortex.accuweather.com/adc2010/images/slate/icons/04.svg                   2016-Sep
    //                  https://www.accuweather.com/en/ca/north-york/m2n/hourly-weather-forecast/49569?hour=8

    static readonly string[] urls = new string[] {
                @"https://www.accuweather.com/en/ca/north-york/m2n/hourly-weather-forecast/49569?hour={0}",					// 0 - ALL ???            
				@"https://www.accuweather.com/en/ca/north-york/m2n/hourly-weather-forecast/49569?hour={0}",					// 1 - NY            
				@"https://www.accuweather.com/en/ca/sutton/l0e/hourly-weather-forecast/55246?hour={0}",								// 2 - SU            
				@"https://www.accuweather.com/en/ca/sauble-beach/n0h/hourly-weather-forecast/2288924?hour={0}",				// 3 - SB            
				@"https://www.accuweather.com/en/ca/sauble-beach/n0h/hourly-weather-forecast/2288924?hour={0}",
                @"https://www.accuweather.com/en/ca/sauble-beach/n0h/hourly-weather-forecast/2288924?hour={0}",
                @"https://www.accuweather.com/en/ca/sauble-beach/n0h/hourly-weather-forecast/2288924?hour={0}",
                @"https://www.accuweather.com/en/ca/sauble-beach/n0h/hourly-weather-forecast/2288924?hour={0}"
        };

    public DateTime Time;
    public float
        TempAsIs,
        TempFeel,
        Precipit,
        WindKmHr,
        Humidity,
        UV_Index,
        CloudCvr,
        DewPoint;
    public string
        WindDirn,
        Conditns,
        IuhContainer,
        ImgUrlHolder;

    public string WeaImageUrl => $"http://vortex.accuweather.com/adc2010/images/icons-numbered/{ImgUrlHolder}-h.png";
    public string WeaImageUr_ => $"http://vortex.accuweather.com/adc2010/images/slate/icons/{ImgUrlHolder}.svg"; // public string WeaImageUrl { get { return IuhContainer == null ? "" : string.Format(urf, (ImgUrlHolder.Split('-')[1].Length < 2 ? "0" : "") + ImgUrlHolder.Split('-')[1]); } }

    public static List<HourWeatherData> GetHourForecast(int max96, bool fromCache, int site = 0)
    {
      //max96 = 144; // <== Mar2016. 96; //jun2013: ignore less than max - take max/everything should not be too much longer than just a part.
      var l = new List<HourWeatherData>();
      for (var startHour = DateTime.Now.Hour > 8 ? 8 : DateTime.Now.Hour; startHour < max96; startHour += 8)
      {
        var awd8hr = AccuWeatherHData.GetParse8hours(startHour, fromCache, site, out var daysOld);
        for (var hr = 0; hr < awd8hr.Length; hr++)
          l.Add(new HourWeatherData { Time = awd8hr[hr].Time.AddDays(-daysOld), ConditionsDescription = awd8hr[hr].Conditns, ConditionsImageUrl = awd8hr[hr].WeaImageUrl, Temp = awd8hr[hr].TempAsIs, TempFeel = awd8hr[hr].TempFeel });
      }

      return l;
    }
    public static AccuWeatherHData[] GetParse8hours_OLD(int startHour, bool fromCache, int site, out int daysOld)
    {
      var url = string.Format(urls[site], startHour);
      var age = TimeSpan.FromTicks(0);
      var htm = fromCache ? WebScraper.GetHtmlFromCacheOrWeb(url, out age) : WebScraper.GetHtmlFromWeb(url); //= Dbg.Htm;
      var pos = 0;
      var stHr = DateTime.Today.AddHours(startHour);
      var awd = new AccuWeatherHData[8];

      daysOld = (int)(DateTime.Today - (DateTime.Now - age).Date).TotalDays;
      
      _ = WebScrapeHelper.GetStringBetween(@"<th class=""first-col""><span>", "</span>", ref htm, ref pos);

      for (var hr = 0; hr < 8; hr++)
      {
        awd[hr] = new AccuWeatherHData();
        _ = WebScrapeHelper.GetStringBetween(hr == 0 ? "<br />" : ">", @"</th>", ref htm, ref pos); //string[] hh = new string[] { "12am", "1am", "2am", "3am", "4am", "5am", "6am", "7am", "8am", "9am", "10am", "11am", "12pm", "1pm", "2pm", "3pm", "4pm", "5pm", "6pm", "7pm", "8pm", "9pm", "10pm", "11pm", "12" };
        awd[hr].Time = stHr.AddHours(hr);// for (int j = 0; j < hh.Length; j++) if (hh[j] == s) awd[i].Time = stHr.AddHours(j);
      }

      pos = htm.IndexOf(@"<th scope=""row"">Forecast</th>", StringComparison.Ordinal);
      if (pos < 0)
        pos = htm.IndexOf(@"<th>Forecast</th>", StringComparison.Ordinal);
      if (pos < 0)
        return awd;

      for (var i = 0; i < 8; i++)
      {
        awd[i].IuhContainer = WebScrapeHelper.GetStringBetween(@"<td class=""", @""">", ref htm, ref pos);
        awd[i].ImgUrlHolder = WebScrapeHelper.GetStringBetween(@"<span", @"""", ref htm, ref pos);
        awd[i].Conditns = WebScrapeHelper.GetStringBetween(@"<span>", @"</span>", ref htm, ref pos);
      }
      for (var i = 0; i < 8; i++) awd[i].TempAsIs = getInt(ref htm, ref pos, awd, i, "&#176;");
      for (var i = 0; i < 8; i++) awd[i].TempFeel = getInt(ref htm, ref pos, awd, i, "&#176;");
      for (var i = 0; i < 8; i++) awd[i].Precipit = getInt(ref htm, ref pos, awd, i, "%");
      for (var i = 0; i < 8; i++)
      {
        skipTd(ref htm, ref pos);
        var s = WebScrapeHelper.GetStringBetween(@">", "<", ref htm, ref pos);

        getWind(awd, i, s);
      }
      for (var i = 0; i < 8; i++) awd[i].Humidity = getInt(ref htm, ref pos, awd, i, "%");
      for (var i = 0; i < 8; i++) awd[i].UV_Index = awd[i].Humidity = getInt(ref htm, ref pos, awd, i, "<");
      for (var i = 0; i < 8; i++) awd[i].CloudCvr = getInt(ref htm, ref pos, awd, i, "%");
      for (var i = 0; i < 8; i++) awd[i].DewPoint = getInt(ref htm, ref pos, awd, i, "&#176;");

      //for (int i = 0; i < 8; i++) Console.WriteLine(awd[i]);			//Debug.Write(htm);

      return awd;
    }

    static void getWind(AccuWeatherHData[] awd, int i, string s)
    {
      if (s.Split(' ').Length > 1)
      {
        awd[i].WindDirn = s.Split(' ')[1];

        awd[i].WindKmHr = (float.TryParse(s.Split(' ')[0], out var f)) ? f : -1;
      }
      else
      {
        awd[i].WindDirn = s;
        awd[i].WindKmHr = -2;
      }
    }

    public static AccuWeatherHData[] GetParse8hours(int startHour, bool fromCache, int site, out int daysOld)
    {
      #region 2016 Sep:
      /*<div class="hourly-table overview-hourly">
    <table>
        <thead>
            <tr>
                <th>
                    Thursday

                </th>
                    <td class="day first-col">
                        <div>11am</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
                    <td class="day ">
                        <div>12pm</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
                    <td class="day ">
                        <div>1pm</div>
                        <div class="icon-weather icon i-15-s"></div>
                    </td>
                    <td class="day ">
                        <div>2pm</div>
                        <div class="icon-weather icon i-15-s"></div>
                    </td>
                    <td class="day ">
                        <div>3pm</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
                    <td class="day ">
                        <div>4pm</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
                    <td class="day ">
                        <div>5pm</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
                    <td class="day last-col">
                        <div>6pm</div>
                        <div class="icon-weather icon i-7-s"></div>
                    </td>
            </tr>
        </thead>
        <tbody>
            <tr>
                <th>Forecast</th>
                    <td class="day  first-col">
                        <span>Cloudy</span>
                    </td>
                    <td class="day ">
                        <span>Cloudy</span>
                    </td>
                    <td class="day ">
                        <span>T-storms</span>
                    </td>
                    <td class="day ">
                        <span>T-storms</span>
                    </td>
                    <td class="day ">
                        <span>Cloudy</span>
                    </td>
                    <td class="day ">
                        <span>Cloudy</span>
                    </td>
                    <td class="day ">
                        <span>Cloudy</span>
                    </td>
                    <td class="day  last-col">
                        <span>Cloudy</span>
                    </td>
            </tr>
            <tr>
                <th>Temp(°F)</th>
                    <td class="day first-col">
                        <span>80°</span>
                    </td>
                    <td class="day ">
                        <span>80°</span>
                    </td>
                    <td class="day ">
                        <span>80°</span>
                    </td>
                    <td class="day ">
                        <span>81°</span>
                    </td>
                    <td class="day ">
                        <span>83°</span>
                    </td>
                    <td class="day ">
                        <span>83°</span>
                    </td>
                    <td class="day ">
                        <span>83°</span>
                    </td>
                    <td class="day last-col">
                        <span>82°</span>
                    </td>
            </tr>
            <tr>
                <th>RealFeel®</th>
                    <td class="day first-col">
                        <span>86°</span>
                    </td>
                    <td class="day ">
                        <span>85°</span>
                    </td>
                    <td class="day ">
                        <span>82°</span>
                    </td>
                    <td class="day ">
                        <span>82°</span>
                    </td>
                    <td class="day ">
                        <span>86°</span>
                    </td>
                    <td class="day ">
                        <span>85°</span>
                    </td>
                    <td class="day ">
                        <span>84°</span>
                    </td>
                    <td class="day last-col">
                        <span>82°</span>
                    </td>
            </tr>
            <tr>
                <th>Wind(mph)</th>
                    <td class="day first-col">
                        <span>9 SW</span>
                    </td>
                    <td class="day ">
                        <span>9 SW</span>
                    </td>
                    <td class="day ">
                        <span>9 WSW</span>
                    </td>
                    <td class="day ">
                        <span>10 W</span>
                    </td>
                    <td class="day ">
                        <span>11 W</span>
                    </td>
                    <td class="day ">
                        <span>12 W</span>
                    </td>
                    <td class="day ">
                        <span>12 WSW</span>
                    </td>
                    <td class="day last-col">
                        <span>11 W</span>
                    </td>
            </tr>

        </tbody>
    </table>
</div>
      */
      #endregion
      var url = string.Format(urls[site], startHour);
      var age = TimeSpan.FromTicks(0);
      var htm = fromCache ? WebScraper.GetHtmlFromCacheOrWeb(url, out age) : WebScraper.GetHtmlFromWeb(url); //= Dbg.Htm;
      var stHr = DateTime.Today.AddHours(startHour);
      var awd = new AccuWeatherHData[8];

      daysOld = (int)(DateTime.Today - (DateTime.Now - age).Date).TotalDays;

      for (var hr = 0; hr < 8; hr++)
      {
        awd[hr] = new AccuWeatherHData
        {
          Time = stHr.AddHours(hr)
        };
      }

      try
      {
        agilityExplorerOne(awd, htm, "//table/thead/tr/td", doIconActn, 3);
        agilityExplorerAry(awd, htm, "//table/tbody", doRest); // the same as "//tbody"
                                                               //agilityExplorer(htm, "//table/thead/tr");
                                                               //agilityExplorer(htm, "//table/thead/div");
                                                               //agilityExplorer(htm, "//table/thead/tr/div");
                                                               //agilityExplorer(htm, "//table/thead");
                                                               //agilityExplorer(htm, "//thead");
                                                               //agilityExplorer(htm, "//table/tbody/tr", doExploreActn);
                                                               //agilityExplorer(htm, "//table", doExploreActn);
      }
      catch (Exception ex) { Debug.WriteLine(ex.Message, ">>> " + System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodInfo.GetCurrentMethod().Name); }

      return awd;
    }

    static void agilityExplorerOne(AccuWeatherHData[] awd, string htm, string path, Action<AccuWeatherHData, HtmlNode, int> actn, int max = 0) //tu: Action/Func
    {
      try
      {
        //77 Debug.Write($"\n\n\n============ htm.len: {htm.Length} '{path}' \r\n ");

        var doc = new HtmlDocument();
        doc.LoadHtml(htm); //note: var doc2 = new HtmlWeb().Load(htm); allows  "//table/tbody/tr"
                           //77 Debug.Write($"{doc.DocumentNode.SelectNodes(path)?.Count,3}:");

        var sn = 0;
        foreach (var tr in doc.DocumentNode.SelectNodes(path).Where(tr => tr.ChildNodes.Count > max))
        {
          actn(awd[sn], tr, max);
          //77 Debug.Write($"\n{sn,4}.{tr.ChildNodes.Count(),2}: tr.ChildNodes.Count: ");
          ++sn;
        }
      }
      catch (Exception ex) { Debug.WriteLine(ex.Message, ">>> " + System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodInfo.GetCurrentMethod().Name); }
    }

    static void agilityExplorerAry(AccuWeatherHData[] awd, string htm, string path, Action<AccuWeatherHData[], HtmlNode, int> actn, int max = 0) //tu: Action/Func
    {
      try
      {
        //77 Debug.Write($"\n\n\n============ htm.len: {htm.Length} '{path}' \r\n ");

        var doc = new HtmlDocument();
        doc.LoadHtml(htm); //note: var doc2 = new HtmlWeb().Load(htm); allows  "//table/tbody/tr"
                           //77 Debug.Write($"{doc.DocumentNode.SelectNodes(path)?.Count,3}:");

        actn(awd, doc.DocumentNode.SelectNodes(path).Where(tr => tr.ChildNodes.Count > max).First(), max);

        var sn = 0;
        foreach (var tr in doc.DocumentNode.SelectNodes(path).Where(tr => tr.ChildNodes.Count > max))
        {
          //77 Debug.Write($"\n{sn,4}.{tr.ChildNodes.Count(),2}: tr.ChildNodes.Count: ");
          ++sn;
        }
      }
      catch (Exception ex) { Debug.WriteLine(ex.Message, ">>> " + System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodInfo.GetCurrentMethod().Name); }
    }

    static void doIconActn(AccuWeatherHData awd, HtmlNode tr, int max)
    {
      //77 Debug.Write(tr.ChildNodes.Count() > max ? $"   x[1]:{(tr.ChildNodes[1]).InnerHtml} : x[{max}]:{(tr.ChildNodes[max]).OuterHtml} " : $"  --------------- {tr.ChildNodes.Count()} < {max} --------------- ");

      var ss = tr.ChildNodes[max].OuterHtml[30..].Split(new[] { '-', '"' });
      Debug.Assert(ss.Length > 1, "usually 'i-36-s' ...");
      awd.ImgUrlHolder = ss[1].Length == 1 ? ("0" + ss[1]) : ss[1];

      //todo: //Debug.Assert(awd.Time.Hour.ToString() == tr.ChildNodes[1].InnerHtml);
    }

    static void doRest(AccuWeatherHData[] awd, HtmlNode tr, int max)
    {
      var cn = 0;
      var nodes = tr.ChildNodes.Where(r => r.ChildNodes.Count > 17).ToArray();
      foreach (var r in nodes) // .Where(n => n.ChildNodes.Any()))
      {
        //77 Debug.Write($"\n    {cn,3}) ttl:{r.ChildNodes.Count()}:  ");				int hn = 0;				foreach (HtmlNode t in r.ChildNodes) Debug.Write($" {hn++}:'{t.InnerText.Replace("\n", "").Replace("\t", "").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim()}'\t");
        ++cn;
      }

      for (var i = 0; i < 8; i++) awd[i].Conditns = nodes[0].ChildNodes[3 + i * 2].InnerText.Trim(new[] { ' ', '\r', '\n' });
      for (var i = 0; i < 8; i++) awd[i].TempAsIs = float.Parse(nodes[1].ChildNodes[3 + i * 2].InnerText.Split(new[] { '&', '#' })[0]);
      for (var i = 0; i < 8; i++) awd[i].TempFeel = float.Parse(nodes[2].ChildNodes[3 + i * 2].InnerText.Split(new[] { '&', '#' })[0]);
      for (var i = 0; i < 8; i++) getWind(awd, i, nodes[3].ChildNodes[3 + i * 2].InnerText);
    }

    static void doExploreAct_(AccuWeatherHData awd, HtmlNode tr, int max)
    {
      var cn = 0;
      foreach (var r in tr.ChildNodes) // .Where(n => n.ChildNodes.Any()))
      {
        var c = r.ChildNodes;
        //77 Debug.Write($"\n    {cn,3}) ttl:{c.Count()}:  ");				int hn = 0;				foreach (HtmlNode t in c) Debug.Write($" {hn++}:'{t.InnerText.Replace("\n", "").Replace("\t", "").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim()}'\t");
        ++cn;
      }
    }

    static int getInt(ref string htm, ref int pos, AccuWeatherHData[] awd, int i, string c2)
    {
      skipTd(ref htm, ref pos);
      return WebScrapeHelper.GetIntBetween(@">", c2, ref htm, ref pos);
    }

    static void skipTd(ref string htm, ref int pos) => WebScrapeHelper.GetStringBetween(@"<td class=""", @"""", ref htm, ref pos);

    public override string ToString() => string.Format(" {0:dd ddd HH:mm}   {1,2} {2,2} {3,3} {4,3} {5,-4} {6,2} {7,2} {8,2}    {9,2}  {10,-25}  {11,-22}  {12,-22}  {13}",
        Time,        //  0
        TempAsIs,    //  1
        TempFeel,    //  2
        Precipit,    //  3
        WindKmHr,    //  4
        WindDirn,    //  5
        Humidity,    //  6
        UV_Index,    //  7
        CloudCvr,    //  8
        DewPoint,    //  9
        IuhContainer,          // 10
        ImgUrlHolder,          // 11
        Conditns,
        WeaImageUrl);
  }

  public class Dbg
  {
    public static string Htm => _htm;

    #region debug HTML
    const string _htm = @"
<!DOCTYPE html>
<html>
<head> 
    <title>North York Hourly Weather - AccuWeather Forecast for Ontario Canada</title>    
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
        <script type=""text/javascript"">
            var apgUserInfoObj = { country: 'CA', city: 'North York', state: 'ON', metro: '', zip: 'M2R', partner: 'accuweather', sessionPartner: 'accuweather', referer: '', lang: 'en-us', langid: '1', lat: '43.778', lon: '-79.447', dma: '' , ip: '66.36.128.116' };
            var apgWxInfoObj = {ut:'1',cu:{wx:'4',hi:'26',wd:'',hd:'',uv:''},fc:[{wx:'',hi:'',lo:''},{wx:'',hi:'',lo:''},{wx:'',hi:'',lo:''}],ix:{arthritis:'',asthma:'',bbq:'',cold:'',dogwalk:'',flu:'',indoor:'',lawnmowing:'',migraine:'',outdoor:'',sports:'',schoolclosing:'',sinus:'',soil:'',field:'',beach:'',biking:'',concert:'',construction:'',composting:'',dust:'',fishing:'',fueleconomy:'',golf:'',heart:'',hiking:'',hvac:'',jogging:'',kite:'',mosq:'',sailing:'',running:'',schoolbus:'',skate:'',skiing:'',star:'',tennis:'',frizz:'',pollen:'',uvindex:''}};   
        </script>

    

</noscript>
    <noscript>
    <a href=""http://www.omniture.com"" title=""Web Analytics""> <img src=""http://accuweather.122.2o7.net/b/ss/accuweatherdev/1/H.16--NS/0"" style=""display:none;"" height=""1"" width=""1"" border=""0"" alt="""" /></a>
    </noscript>
</body>
</html>
";

    #endregion
  }
}
