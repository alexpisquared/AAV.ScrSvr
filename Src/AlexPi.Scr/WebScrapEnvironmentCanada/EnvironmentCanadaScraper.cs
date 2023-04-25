using StandardLib.Extensions;
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using WebScrap;

///rss:
///http://rss.theweathernetwork.com/weather/caon0487
///http://www.rssweather.com/wx/ca/on/buttonville/rss.php
///http://www.rssweather.com/wx/ca//toronto+buttonville/rss.php
///http://www.rssweather.com/wx/ca/on/toronto+pearson+intl./rss.php
///? http://msdn.microsoft.com/en-us/library/system.servicemodel.syndication.syndicationfeed.aspx

/* Mobile:
 * http://m.weather.gc.ca/city/pages/on-85_e.html - Buttonville 
 * http://m.weather.gc.ca/city/pages/on-24_e.html - Pearson 
 * 
 * 
 * P.S : I'm using this code for testing my simple RSS reader:
        var reader = XmlReader.Create("http://feed.2barnamenevis.com/2barnamenevis");
        var feed = SyndicationFeed.Load(reader);
        string s = "";
        foreach (SyndicationItem i in feed.Items)
        {
            s += i.Title.Text + "<br />" + i.Summary.Text + "<br />" + i.PublishDate.ToString() + "<br />";
            foreach (SyndicationElementExtension extension in i.ElementExtensions)
            {
                XElement ele = extension.GetObject<XElement>();
                s += ele.Name + " :: " + ele.Value + "<br />";
            }
            s += "<hr />";
        }
        return 
 * 
 * 
 * Here is an example of the RssParser
 
if (doc.Elements().Count() == 0) return null;
 
XNamespace ns = "http://purl.org/rss/1.0/modules/slash/";

List items = (from item in doc.Descendants("item")
                        select new NewsItem
                {
                    Id = (item.Element("guid") != null ? item.Element("guid").Value : item.Element("link").Value),
                    Title = item.Element("title").Value,
                    Timestamp = Rfc822DateTime.Parse(Rfc822DateTime.Clean(item.Element("pubDate").Value) ?? DateTime.Now.ToString()),
                    Url = item.Element("link").Value,
                    Description = (item.Element("description").Value ?? ""),
                    NewsFeed = feed,
                    FetchTime = DateTime.Now.ToUniversalTime(),
                    Source = feed.Name
                }).ToList();
 return items; */

namespace EnvironmentCanadaScrap
{
  public partial class EnvntCanadaScraper
  {
    const string
      _cacheRoot = @"C:\Temp\web.cache\text.www.weatheroffice.gc.ca\", //todo: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"SkyDrive\") + @"web.cache
      _serializationFile = _cacheRoot + @"envCnd.xml",
      _doneNeData = _cacheRoot + @"Done.NoData",
      _doneNoErrs = _cacheRoot + @"Done.NoErrors",
      _urlCurntCondXml = @"http://www.weatheroffice.gc.ca/rss/city/on-64_e.xml",  // RSS Vaughan
      _urlCurntCondHtm = @"http://weather.gc.ca/city/pages/on-64_metric_e.html",  // Cur + Historical + Sunrize-Set.
#if true // toronto
      _urlPast24hr = @"http://weather.gc.ca/past_conditions/index_e.html?station=yyz",
      _urlFore24hr = @"http://weather.gc.ca/forecast/hourly/on-143_metric_e.html",
#else   // butoonville is bad: no images, observations, etc.
      _urlPast24hr = @"http://weather.gc.ca/past_conditions/index_e.html?station=ykz",  
      _urlFore24hr = @"http://weather.gc.ca/forecast/hourly/on-85_metric_e.html",  
#endif
      _urlExtremes = @"http://www.weatheroffice.gc.ca/almanac/almanac_e.html?yyz",      //yyz has longer history than ykz (which scales things smaller on the screen, though...)
      _urlPast24hSea = @"http://weather.gc.ca/marine/weatherConditions-24hrObsHistory_e.html?mapID=11&siteID=08300&stationID={0}", //http://weather.gc.ca/marine/weatherConditions-24hrObsHistory_e.html?mapID=11&siteID=08300&stationID={0}",
      _urlPast24hrLS = @"http://weather.gc.ca/marine/weatherConditions-24hrObsHistory_e.html?mapID=11&siteID=08300&stationID=45151",
      _urlPast24hrSB = @"http://weather.gc.ca/marine/weatherConditions-24hrObsHistory_e.html?mapID=11&siteID=08300&stationID=45143";


    static readonly WebScrapEnvironmentCanada.Almanac.NewDataSet dsAlmanac = new();

    static EnvntCanadaScraper()
    {
      var t0 = Stopwatch.StartNew();
      ////dsAlmanac.ReadXml(@"C:\0\0\Web\Scrapping\WebScrapEnvironmentCanada\Almanac\WholeYear.xml");
      //dsAlmanac.ReadXml(new System.IO.StringReader(EnvironmentCanadaScrap.Properties.Resources.WholeYear), XmlReadMode.IgnoreSchema); //tu: read ds from string xml

      Debug.Write(t0.ElapsedMilliseconds, "ReadXml");//120
    }

    public static EnvironmentCanadaData FetchCurConds(bool fromCache)
    {
      var e = new EnvironmentCanadaData();

      GetCurrentConditions(fromCache, e);

      return e;
    }
    public static EnvironmentCanadaData FetchCurrentExtremeConditionsImage(bool fromCache)
    {
      var e = new EnvironmentCanadaData();

      GetCurrentConditions(fromCache, e);
      getCurrentCondiImage(fromCache, e);
      GetExtremeConditions(fromCache, e);

      e.XmlSave(_cacheRoot + DateTime.Now.ToString("yyyyMMdd.HHmmss") + ".xml.txt");
      e.XmlSave(_cacheRoot + DateTime.Now.ToString("MMMdd") + ".xml");

      return e;
    }

    static void GetCurrentConditions(bool fromCache, EnvironmentCanadaData ecd) //todo: replace/sync/unify with teh code from   C:\c\wp8\RLS\RunPlan.Cmn\Services\EnvtCanXmlParser.cs
    {
      //string html = fromCache ? WebScraper.GetHtmlFromCacheOrWeb(_urlCurntCondHtm) : WebScraper.GetHtmlFromWeb(_urlCurntCondHtm);
      var html = WebScraper.GetHtmlFromWeb(_urlCurntCondHtm);
      if (html.Length < 5000) return;

      try
      {
        var curpos = 0;
        ecd.TakenAt = (WebScrapeHelper.GetDateTimeBetween("Date: </dt>\n              <dd class=\"mrgn-bttm-0\">", " EDT ", ref html, ref curpos)); //summer time
        if (curpos <= 0)
        {
          curpos = 0;
          ecd.TakenAt = (WebScrapeHelper.GetDateTimeBetween("Date: </dt>\n              <dd class=\"mrgn-bttm-0\">", " EST ", ref html, ref curpos)); //winter time
        }
        if (curpos <= 0 || ecd.TakenAt == DateTime.MinValue)
        {
          curpos = 0;
          ecd.TakenAt = DateTime.Now;
        }

        ecd.TempAir = (WebScrapeHelper.GetDoubleBetween("<p class=\"text-center mrgn-tp-md mrgn-bttm-sm lead\"><span class=\"wxo-metric-hide\">", "&deg;", ref html, ref curpos));
        //ecd.TempYesterdayMax = (WebScrapeHelper.GetDoubleBetween("Maximum\">Max</span></abbr>:</dt><dd><ul class=\"alignUnit\"><li>", "&deg;C", ref html, ref curpos));
        //ecd.TempYesterdayMin = (WebScrapeHelper.GetDoubleBetween("Minimum\">Min</span></abbr>:</dt><dd><ul class=\"alignUnit\"><li>", "&deg;C", ref html, ref curpos));
        //ecd.TempNormMax = (WebScrapeHelper.GetDoubleBetween("Maximum\">Max</span></abbr>:</dt><dd><ul class=\"alignUnit\"><li>", "&deg;C", ref html, ref curpos));
        //ecd.TempNormMin = (WebScrapeHelper.GetDoubleBetween("Minimum\">Min</span></abbr>:</dt><dd><ul class=\"alignUnit\"><li>", "&deg;C", ref html, ref curpos));

        curpos = html.IndexOf("Humidex");
        if (curpos < 0)
          curpos = html.IndexOf("Wind Chill");
        ecd.Humidex = WebScrapeHelper.GetNlblDoubleBetween("<dd class=\"mrgn-bttm-0 wxo-metric-hide\">", "</dd>", ref html, ref curpos);

        curpos = html.IndexOf("currentimg");
        ecd.ConditiImg = @"http://weather.gc.ca/" + WebScrapeHelper.GetStringBetween("src=\"/", "\" alt", ref html, ref curpos);

        curpos = 0;
        ecd.SunRise = (WebScrapeHelper.GetDateTimeBetween("Sunrise</th>\n            <td>", "</td>", ref html, ref curpos));
        ecd.SunSet = (WebScrapeHelper.GetDateTimeBetween("Sunset</th>\n            <td>", "</td>", ref html, ref curpos));

        if (ecd.SunRise == DateTime.MinValue)
          ecd.SunRise = DateTime.Today.AddHours(6);
        if (ecd.SunSet == DateTime.MinValue)
          ecd.SunSet = DateTime.Today.AddHours(18);
      }
      catch (Exception ex)
      {
        ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
      }
    }
    static void GetExtremeConditions(bool fromCache, EnvironmentCanadaData e3)
    {
      var html = WebScraper.GetHtmlCached(_urlExtremes, new TimeSpan(6, 0, 0), 8000, fromCache);
      if (html.Length > 8000)
      {
        try
        {
          var curpos = 0;
          e3.TempExtrMax = (WebScrapeHelper.GetDoubleBetween("<td headers=\"header1 header6\">", "&deg;C", ref html, ref curpos));
          e3.YearExtrMax = (WebScrapeHelper.GetIntBetween("<td headers=\"header2 header6\">", "</td>", ref html, ref curpos));
          e3.TempExtrMin = (WebScrapeHelper.GetDoubleBetween("<td headers=\"header1 header7\">", "&deg;C", ref html, ref curpos));
          e3.YearExtrMin = (WebScrapeHelper.GetIntBetween("<td headers=\"header2 header7\">", "</td>", ref html, ref curpos));
        }
        catch (Exception ex)
        {
          ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
        }
      }
    }
    static void getCurrentConditiImg_EnvCan(bool fromCache, EnvironmentCanadaData e2)
    {
      var html = WebScraper.GetHtmlCached("http://www.weatheroffice.gc.ca/city/pages/on-59_metric_e.html", new TimeSpan(0, 30, 0), 8000, fromCache);
      if (html.Length > 8000)
      {
        try
        {
          var curpos = 0;
          /// <div id="currentcond-left"><img id="currentimg" src="/weathericons/01.gif" alt="Mainly Sunny" title="Mainly Sunny" /><div>
          e2.ConditiImg = "http://www.weatheroffice.gc.ca/weathericons/" + WebScrapeHelper.GetStringBetween("currentimg\" src=\"/weathericons/", "\" alt=", ref html, ref curpos);
        }
        catch (Exception ex)
        {
          ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
        }
      }
    }
    static void getCurrentCondiImage(bool fromCache, EnvironmentCanadaData e2)
    {
      var html = WebScraper.GetHtmlCached("http://weather.yahoo.com/canada/ontario/markham-935/?unit=c", new TimeSpan(0, 30, 0), 8000, fromCache);
      if (html.Length > 8000)
      {
        try
        {
          var curpos = 0;
          /// <div class="forecast-icon" style="filter: progid:DXImageTransform.Microsoft.AlphaImageLoader(src='http://l.yimg.com/a/i/us/nws/weather/gr/32d.png', sizingMethod='crop'); background-image: url("http://l.yimg.com/a/i/us/nws/weather/gr/32d.png"); background-attachment: scroll; background-repeat: repeat; background-position-x: 0%; background-position-y: 0%; background-size: auto; background-origin: padding-box; background-clip: border-box; background-color: transparent;"/>
          e2.ConditiImg = WebScrapeHelper.GetStringBetween("DXImageTransform.Microsoft.AlphaImageLoader(src='", "', sizingMethod", ref html, ref curpos);
        }
        catch (Exception ex)
        {
          ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
        }
      }
    }


    public static List<EnvironmentCanadaData> Past24hourMarine(bool fromCache, int site)
    {
      var _urlPast24hr = string.Format(_urlPast24hSea, site);

      string s = "", sDate = "", html = WebScraper.GetHtml_(_urlPast24hr, fromCache);//, new TimeSpan(0, 30, 0), 25000, fromCache);
      var ecdList = new List<EnvironmentCanadaData>();

      if (html.Length < 25000)
        return ecdList;

      for (int curpos = 0, i = 0; i < 24; i++)
      {
        var e = process1hour_LakeSimcoeLikeEntry(ref s, ref sDate, ref html, ref curpos);
        if (e != null)
          ecdList.Add(e);
      }

      //for (int i = 0; i < ecdList.Count; i++)				Console.WriteLine("{0,2}) {1}", i, ecdList[i].ToString());

      return ecdList;
    }
    public static void LoadToSql()
    {
      var lst = EnvironmentCanadaDataList.XmlLoad(_serializationFile);
      if (lst == null)
        lst = new EnvironmentCanadaDataList();

      if (!Directory.Exists(_doneNeData)) Directory.CreateDirectory(_doneNeData);
      if (!Directory.Exists(_doneNoErrs)) Directory.CreateDirectory(_doneNoErrs);

      var files = Directory.GetFiles(_cacheRoot, "*.html");
      for (var i = 0; i < files.Length; i++)
      {
        Console.WriteLine(":> {0}/{1} {2}%", 1 + i, files.Length, 100 * (1 + i) / files.Length);

        var html = File.ReadAllText(files[i]);
        if (html.Length < 25000)
          continue;

        var badEntryCount = addDistinctEtriesToList(html, lst, files[i].Split(new char[] { '=', '&', '-' })[7]);
        //if (badEntryCount == 24)
        //  File.Move(files[i], Path.Combine(_doneNeData, Path.GetFileName(files[i])));
        //else if (badEntryCount > 0)
        //  Console.WriteLine(">{0} bad entries in files[i] {1}", badEntryCount, files[i]);
        //else
        //  File.Move(files[i], Path.Combine(_doneNoErrs, Path.GetFileName(files[i])));

        //for (int i = 0; i < ecdList.Count; i++)				Console.WriteLine("{0,2}) {1}", i, ecdList[i].ToString());
      }

      //todo: uncomment if SQL datasets in 2020 is still a thing lst.SaveToSql();
    }

    static int addDistinctEtriesToList(string html, EnvironmentCanadaDataList lst, string siteID)
    {
      string s = "", sDate = "";
      var siteId = Convert.ToInt32(siteID);
      var badEntryCount = 0;
      for (int curpos = 0, i = 0; i < 24; i++)
      {
        var e = process1hour_LakeSimcoeLikeEntry(ref s, ref sDate, ref html, ref curpos);
        if (e != null && e.TempSea > 0)
        {
          //ecd.XmlSave(OneDrive.Root, @"\web.cache\ecd.xml.txt");
          e.SiteId = siteId;
          lst.Add(e);
        }
        else
        {
          ++badEntryCount;
        }
      }

      return badEntryCount;
    }

    static EnvironmentCanadaData process1hourButtonvilleLikeEntry(ref string s, ref string sDate, ref string html, ref int curpos)
    {
      var curpos0 = curpos;
      try
      {
        var e4 = new EnvironmentCanadaData();
        var pff = "<th colspan=\"8\" class=\"wxo-th-bkg\">";
        var pdate = html.IndexOf(pff, curpos);
        if (pdate < 0)
        {
          pff = "<th colspan=\"8\" class=\"align-left\">";
          pdate = html.IndexOf(pff, curpos);
        }
        var ptime = html.IndexOf("<td headers=\"header1\">", curpos);

        if (pdate > 0 && pdate < ptime)
          sDate = WebScrapeHelper.GetStringBetween(pff, "</th>", ref html, ref curpos);

        s = WebScrapeHelper.GetStringBetween("<td headers=\"header1\">", "</td>", ref html, ref curpos);
        e4.TakenAt = Convert.ToDateTime(sDate + ' ' + s);

        e4.Conditions = WebScrapeHelper.GetStringBetween("<td headers=\"header2\">", "</td>", ref html, ref curpos);

        var cc = "<td headers=\"header3\" class=";
        curpos = html.IndexOf(cc, curpos);

        if (html.Substring(curpos + cc.Length, 1) != "a") curpos += (cc.Length + 12);

        e4.TempAir = (WebScrapeHelper.GetDoubleBetween(">", "<", ref html, ref curpos));
        e4.Humidity = (WebScrapeHelper.GetDoubleBetween("<td headers=\"header4\">", "</td>", ref html, ref curpos));
        e4.DewPoint = (WebScrapeHelper.GetDoubleBetween("<td headers=\"header5\">", "</td>", ref html, ref curpos));

        s = WebScrapeHelper.GetStringBetween("<td headers=\"header6\">", "</td>", ref html, ref curpos);
        var w = s.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        switch (w.Length)
        {
          case 1: e4.WindDir = w[0]; e4.WindKmH = 0; break;
          case 2: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); break;
          case 3: break;
          case 4: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); e4.WindGust = Convert.ToDouble(w[3]); break;
          default: break;
        }

        e4.Pressure = WebScrapeHelper.GetDoubleBetween("<td headers=\"header7\">", "</td>", ref html, ref curpos);
        e4.Visibility = WebScrapeHelper.GetDoubleBetween("<td headers=\"header8\">", "</td>", ref html, ref curpos);
        e4.Humidex = WebScrapeHelper.GetNlblDoubleBetween("<td headers=\"header9\">", "</td>", ref html, ref curpos);

        return e4;
      }
      catch (Exception ex)
      {
        ex.Log();
        curpos = curpos0;
        return null;
      }
    }
    static EnvironmentCanadaData process1hour_LakeSimcoeLikeEntry(ref string s, ref string sDate, ref string html, ref int curpos)
    {
      var curpos0 = curpos;
      const string td = "<td class=\"t-center\">";
      try
      {
        var e5 = new EnvironmentCanadaData();
        var pdate = html.IndexOf("<th class=\"date\" colspan=\"7\">", curpos);
        var ptime = html.IndexOf("<td class=\"t-center\">", curpos);

        if (pdate == -1 && ptime == -1)
          return null;

        if (pdate > 0 && pdate < ptime)
        {
          sDate = WebScrapeHelper.GetStringBetween("<th class=\"date\" colspan=\"7\">", "</th>", ref html, ref curpos);
        }

        s = WebScrapeHelper.GetStringBetween("<td class=\"t-center\">", "</td>", ref html, ref curpos);
        e5.TakenAt = Convert.ToDateTime(sDate + ' ' + s);

        s = WebScrapeHelper.GetStringBetween(td, "</td>", ref html, ref curpos);
        var w = s.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        switch (w.Length)
        {
          case 1: e5.WindDir = w[0]; e5.WindKmH = 0; break;
          case 2: e5.WindDir = w[0]; e5.WindKmH = Convert.ToDouble(w[1]); break;
          case 3: break;
          case 4: e5.WindDir = w[0]; e5.WindKmH = Convert.ToDouble(w[1]); e5.WindGust = Convert.ToDouble(w[3]); break;
          default: break;
        }

        e5.WaveHeight = WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos);
        e5.WavePeriod = WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos);
        e5.Pressure = (WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos));
        e5.TempAir = (WebScrapeHelper.GetDoubleBetween(">", "</td>", ref html, ref curpos));
        e5.TempSea = (WebScrapeHelper.GetDoubleBetween(">", "</td>", ref html, ref curpos));

        //ecd.Conditions = WebScrapeHelper.GetStringBetween(td, "</td>", ref html, ref curpos);

        //curpos = html.IndexOf(td, curpos); // <td headers="header3" class="highTemp">30</td>
        //if (curpos < 0)
        //  Debug.WriteLine("");

        //ecd.Humidity = (WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos));
        //ecd.DewPoint = (WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos));

        //ecd.Visibility = (WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos));
        ////?ecd.Humidex  = WebScrapeHelper.GetDoubleBetween(td, "</td>", ref html, ref curpos);

        return e5;
      }
      catch (Exception ex)
      {
        ex.Log();
        curpos = curpos0;
        return null;
      }
    }

    public static void ScrapAndLog24HourMarineObservations()
    {
      foreach (var url in 
        //Properties.Settings.Default.SiteList
        "45151,smc,Lake Simcoe,1|45137,ngb,North Georgian Bay,0|45143,sgb,South Georgian Bay,1|45139,wlo,West Lake Ontario,1|45135,elo,East Lake Ontario,0|45149,slh,South Lake Huron,0|45008,msh,Mid South Huron,1|45003,nlh,North Lake Huron,1|45132,wle,West Lake Erie,0|45142,ele,East Lake Erie,0"
        .Split('|')

        )
      {
        EnvntCanadaScraper.ScrapAndLog24HourMarineObservations(url.Split(','), @"http://weather.gc.ca/marine/weatherConditions-24hrObsHistory_e.html?mapID=11&siteID=08300&stationID={0}"); //  Properties.Settings.Default.ECUrl24hourTemplate);
      }
    }
    public static void ScrapAndLog24HourMarineObservations(string[] url, string urlTemplate)
    {
      try
      {
        var html = WebScraper.GetHtmlCached(string.Format(urlTemplate, url[0]), TimeSpan.FromHours(1));

        var lst = new EnvironmentCanadaDataList();

        var badEntryCount = addDistinctEtriesToList(html, lst, url[0]);

        //todo: uncomment if SQL datasets in 2020 is still a thing lst.SaveToSql();
      }
      catch (Exception ex)
      {
        ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());

        var s = WebScraper.CacheHtmlToUniqueFile(string.Format(urlTemplate, url[0]));
      }
    }

    public static EnvironmentCanadaData GetHistoricalTemp(DateTime date)
    {
      var e6 = new EnvironmentCanadaData();

      //regenSchema();

      var mr = dsAlmanac.month[date.Month - 1]; // Oct 2010: foreach (Almanac.NewDataSet.monthRow mr in dsAlmanac.month.Select("index=" + date.Month.ToString()))

      //`Console.WriteLine("m:{0}", mr.index);
      foreach (var dr in mr.GetdayRows())
      {
        if (dr.index != date.Day.ToString())
          continue;

        //`Console.Write("    d:{0}:  ", dr.index);
        foreach (var pr in dr.GettemperatureRows())
        {
          //`Console.Write("\n {0,44} {1}", pr.ecClass, pr.temperature_Text);
          switch (pr.ecClass)
          {
            case "extremeMax": e6.TempExtrMax = Convert.ToDouble(pr.temperature_Text); break;
            case "extremeMin": e6.TempExtrMin = Convert.ToDouble(pr.temperature_Text); break;
            case "normalMax": e6.TempNormMax = Convert.ToDouble(pr.temperature_Text); break;
            case "normalMin": e6.TempNormMin = Convert.ToDouble(pr.temperature_Text); break;
          }
        }
        //`Console.WriteLine("    ");
      }


      return e6;
    }

    static DataSet regenSchema()
    {
      var ds = new DataSet();
      ds.ReadXml(@"C:\0\0\Web\Scrapping\WebScrapEnvironmentCanada\Almanac\WholeYear.xml");
      //ds.WriteXml(@"C:\0\0\Web\Scrapping\WebScrapEnvironmentCanada\Almanac\WholeYear_WriteXml.xml");
      ds.WriteXmlSchema(@"C:\0\0\Web\Scrapping\WebScrapEnvironmentCanada\Almanac\WholeYear_WriteXmlSchema.xsd");

      //DataClassLibrarry.Util.ExploreDataSet_And_Relations(ds);

      return ds;
    }

    public static SortedList<DateTime, double> GasPriceTomrw(bool fromCache)
    {
      var d = new SortedList<DateTime, double>();
      try
      {
        var url = @"http://www.stockr.net/Toronto/GasPrice.aspx";
        var html = GetHtml(fromCache, url);

        var curpos = html.IndexOf(@">Today</h3>");
        var priceToday = WebScrapeHelper.GetDoubleBetween(@"lPrice2"" class=""gasPrice"">", @"</span>", ref html, ref curpos);
        var date_Today = WebScrapeHelper.GetDateTimeBetween(@"lDate2"">", @"</span>", ref html, ref curpos);
        var priceTomrw = WebScrapeHelper.GetDoubleBetween(@"lPrice1"" class=""gasPrice"">", @"</span>", ref html, ref curpos);
        var date_Tomrw = WebScrapeHelper.GetDateTimeBetween(@"lDate1"">", @"</span>", ref html, ref curpos);

        if (priceToday > 33 && date_Today > DateTime.Today.AddDays(-2)) d.Add(date_Today.AddDays(.500), priceToday - 100);
        if (priceTomrw > 33 && date_Tomrw > DateTime.Today.AddDays(-0)) d.Add(date_Tomrw.AddDays(.511), priceTomrw - 100);
      }
      catch (Exception ex) { ex.Log(); }

      return d;
    }

    static string GetHtml(bool fromCache, string url)
    {
      string html;
      if (fromCache)
        html = WebScraper.GetHtmlFromCacheOrWeb(url);
      else
        html = WebScraper.GetHtmlFromWeb(url);
      return html;
    }
  }

  public enum MarineSite
  {
    Simcoe1 = 45151,
    Nipisng = 45152,
    OntzriW = 45139,
    OntAjax = 45159,
    GeoBayS = 45143,
    SaubleN = 45003,
    SaubleM = 45008,
    SaubleS = 45149,
    Superio = 45004,
    Lk_Erie = 45142
  }

  public class EnvironmentCanadaHistDataSet
  {
    const int range = 5;
    readonly double min = 555, max = -555, _padding = .5;
    public EnvironmentCanadaData[] _ecds = new EnvironmentCanadaData[range];

    public EnvironmentCanadaHistDataSet()
    {
      //140905 Trace .WriteLine(string.Format("{0} - {1}.{2}", DateTime.Now.ToString("\nHH:mm:ss"), System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name, System.Reflection.MethodInfo.GetCurrentMethod().Name));

      try
      {
        for (var i = 0; i < range; i++)
        {
          //140905 Trace .WriteLine(string.Format("{0} - {1}.{2}", DateTime.Now.ToString("\nHH:mm:ss"), System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name, System.Reflection.MethodInfo.GetCurrentMethod().Name));
          _ecds[i] = EnvntCanadaScraper.GetHistoricalTemp(DateTime.Today.AddDays(i - 1));
          if (min > _ecds[i].TempExtrMin) min = _ecds[i].TempExtrMin;
          if (max < _ecds[i].TempExtrMax) max = _ecds[i].TempExtrMax;
        }
      }
      catch (Exception ex)
      {
        //140905 Trace .WriteLine(string.Format("{0} - {1}.{2}", DateTime.Now.ToString("\nHH:mm:ss"), System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name, System.Reflection.MethodInfo.GetCurrentMethod().Name));
        ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
      }
      //140905 Trace .WriteLine(string.Format("{0} - {1}.{2}", DateTime.Now.ToString("\nHH:mm:ss"), System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name, System.Reflection.MethodInfo.GetCurrentMethod().Name));
    }

    public double Min => min - _padding;
    public double Max => max + _padding;
  }
}
/* Oct 2016: 
 * Historical data (no sunrize, though)
http://climate.weather.gc.ca/climate_data/bulk_data_e.html?format=csv&stationID=53678&Year=2016&Month=9&Day=14&timeframe=1&submit= Download+Data
http://climate.weather.gc.ca/climate_data/bulk_data_e.html?format=csv&stationID=53678&Year=2016&Month=9&Day=14&timeframe=2
http://climate.weather.gc.ca/climate_data/bulk_data_e.html?format=csv&stationID=53678&Year=2016&Month=9&Day=14&timeframe=2

XML:  !!!
http://climate.weather.gc.ca/climate_data/bulk_data_e.html?format=xml&stationID=53678&Year=2016&Month=9&Day=14&timeframe=2*/
