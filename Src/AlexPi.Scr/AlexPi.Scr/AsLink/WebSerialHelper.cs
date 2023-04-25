//using StandardLib.Extensions;
//using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToRunOr.WPF
{
  public partial class WebSerialHelper /// requires C:\c\AsLink\UWP\WebHelper.uwp.cs
	{
    public static async Task<T> UrlToInstnace<T>(string url, TimeSpan? expiryPeriod = null) where T : new()
    {
      try
      {
        var webStr = await WebHelper.GetWebStr(url, expiryPeriod); // Debug.WriteLine($"{url}:\r\n{txt}");      //T t = (T)Serializer.LoadFromStringMin<T>(xml);      return t;                                          //return StringToInstance<T>(xml);
        if (webStr.StartsWith("<")) return (T)new XmlSerializer(typeof(T)).Deserialize(new StringReader(webStr));
        if (webStr.StartsWith("[") || webStr.StartsWith("{")) return JsonStringSerializer.Load<T>(webStr); //needs what? (check BusCatch) ... probably C:\c\AsLink\PlatformNeutral\JsonHelper.cs
        else
        {
          Debug.WriteLine(/*ApplicationData.Current.RoamingSettings.Values["Exn"] = */$"\r\nFAILED deserial-n: \r\n{url}\r\n{webStr}");
        }
      }
      catch (Exception ex) { ex.Log(); throw; }

      return default(T); // null 
    }

    public static T StringToInstance<T>(string xml) => (T)new XmlSerializer(typeof(T)).Deserialize(new StringReader(xml));
  }

  public partial class WebHelper
  {
    public static async Task<string> GetWebStr(string url, TimeSpan? expiryPeriod = null)
    {
      var sw = Stopwatch.StartNew();
      var now = DateTime.Now;

      try
      {
        ////if (expiryPeriod != null)
        ////{
        ////    var cached = await WebCacheHelper.TryGetFromCache(url);
        ////    if (cached != null && cached.Length > 100) //todo: && did not ask for the same twice in 1 minute (Jun2016)
        ////        return cached;
        ////}

#if OFFLINE
      await Task.Delay(33);
      if (url.Contains("agencyList")) return _al;       // url == "http://webservices.nextbus.com/service/publicXMLFeed?command=agencyList") return _al;
      if (url.Contains("routeList")) return _rl;        // url == "http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=ttc" || 
      if (url.Contains("routeConfig")) return _rc;      // url == "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=ttc&r=125" || 
      if (url.Contains("predictions")) return _pr;      // url == "http://webservices.nextbus.com/service/publicXMLFeed?command=predictions&a=actransit&r=5&s=4181&useShortTitles=false" || 
      if (url.Contains("vehicleLocations")) return _lo; // url == "http://webservices.nextbus.com/service/publicXMLFeed?command=vehicleLocations&a=actransit&r=5&t=0" || 
#endif
        var httpClientHandler = new HttpClientHandler { Proxy = WebRequest.DefaultWebProxy, UseProxy = true };
        httpClientHandler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials; //TU: 2/2 //note: is not required for getiing pics and files - html only (3-jun-2010).

        using (var client = new HttpClient(httpClientHandler))
        {
          try
          {
            //ient.DefaultRequestHeaders.Add("Expires", "99999");                           // crashes
            //ient.DefaultRequestHeaders.Add("Cache-Control", "max-age=604800, public");    //no changes   (604800 == 7 days)
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");                  //tu: avoid caching!!!

            var str = await client.GetStringAsync(url);

            ////if (expiryPeriod != null)
            ////    await WebCacheHelper.PutToCache(url, str, now + expiryPeriod.Value);

            return str;
          }
          catch (HttpRequestException ex) { return string.Format("\n HttpRequestException: {0} \n {1}", now, ex); } // ignore disconnected case.
          catch (Exception ex) { return string.Format("\n Exception: {0} \n {1}", now, ex); }
        }
      }
      catch (Exception ex) { return string.Format("\n Exception: {0} \n {1}", now, ex); }
      finally { /*Debug.WriteLine("°");*/ }//==> Caching test: {0,8:N0} ms  for '{1}' (usually 100ms) for url   {2}.", sw.ElapsedMilliseconds, expiryPeriod != null ? "tryCached" : "nonCached", url); }
    }
  }


#if !!TempFix // use instead: C:\c\AsLink\PlatformNeutral\JsonHelper.cs
	public class JsonHelper 	{		public static T FromJson<T>(string txt)		{			return default(T);		}	}
#endif
}
