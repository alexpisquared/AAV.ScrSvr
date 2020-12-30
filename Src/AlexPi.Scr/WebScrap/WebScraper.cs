//todo: move to shared somewhere (2020-12)
using AAV.Sys.Ext;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace WebScrap
{
  public partial class WebScraperBase
  {
    protected static WebClient getProxyAndCreds(Uri uri)
    {
      var wc = new WebClient
      {
        Proxy = WebRequest.DefaultWebProxy //TU: 1/2
      };
      wc.Proxy.Credentials = getICredentials(uri);

      //wc.UseDefaultCredentials = true; //Sep2015
      wc.Credentials = CredentialCache.DefaultNetworkCredentials;

      return wc;
    }
    protected static WebRequest createWebRequest(Uri uri)
    {
      var wr = HttpWebRequest.Create(uri); //TU: 0/2
      wr.Proxy = WebRequest.DefaultWebProxy; //TU: 1/2

      //does not owrk on hte new Lenovo T510; need more smarts and exlicit credentials: //todo: why Default... does not work?
      wr.Proxy.Credentials = getICredentials(uri);

      return wr;
    }
    protected static ICredentials getICredentials(Uri uri)
    { //WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("connect.garmin.com/proxy/download-service") ? new NetworkCredential("pigida", sABC + sABC + "6NG") : // is it still available (Nov 2020)
      //WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("aavproltd") ? new NetworkCredential("apigida", sABC + " " + s123.ToString(), "AAVproLtd") :
      //WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("10.0.10.") ? new NetworkCredential("alexp", sABC.Substring(1) + sABC.Substring(1), "gemc") :
      //WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("10.1.0.1") ? new NetworkCredential("alexp", sABC.Substring(1) + sABC.Substring(1), "gemc") :
      return CredentialCache.DefaultNetworkCredentials; //TU: 2/2 //note: is not required for getiing pics and files - html only (3-jun-2010).
      //const int s123 = 345; const string sABC = "are you crazy!"; // from old <-- 4+4 <--
    }
    protected static void safeFile_WriteAllText(string file, string text)
    {
      try { File.WriteAllText(file, text); }
      catch (Exception ex) { ex.Log(file); }
    }
  }

  public partial class WebScraper : WebScraperBase
  {
    public static string Check(string url) //check if onternet is available
    {
      try
      {
        var request = createWebRequest(new Uri(url));
        request.Timeout = 5000; //ms

        using (var response = request.GetResponse())
        {
          using (var stream = new StreamReader(response.GetResponseStream()))
          {
            var nootUsed = stream.ReadToEnd();
          }
        }

        return "Connected";
      }
      catch (WebException ex)
      {
        return "Disconnected (WebException: " + ex.Message + ")";
      }
      catch (Exception ex)
      {
        return "Disconnected (Exception: " + ex.Message + ")";
      }
    }
    public static XDocument GetXml(string url)
    {
      var fc = Console.ForegroundColor;
      XDocument feedXML = null;
      for (var i = 0; i < 3 && feedXML == null; i++)
      {
        try
        {
          //feedXML = XDocument.LoadUsing_XDocument(fe.Url);  ==> for behind the proxy domains:
          var wc = getProxyAndCreds(new Uri(url));
          var ms = new MemoryStream(wc.DownloadData(url));
          var rdr = new XmlTextReader(ms);
          feedXML = XDocument.Load(rdr);
        }
        catch (Exception ex) { ex.Log(); }
        finally { Console.ForegroundColor = fc; }
      }

      return feedXML;
    }

    public static string ScrapAndRemoveHtml(string url)
    {
      var strOutput = GetWebStr(url);

      strOutput = RemoveHtml(strOutput);

      return strOutput;
    }
    public static string RemoveHtml(string strHTML)
    {
      string strOutput = null;

      try
      {
        strOutput = new Regex(@"<[^>]*>").Replace(strHTML, " "); //tu: remove html
                                                                 //				strOutput = new Regex(@"(\n)").Replace(strOutput, " "); 
        strOutput = new Regex(@"(\t)").Replace(strOutput, " ");
        strOutput = new Regex(@"      ").Replace(strOutput, " ");
        strOutput = new Regex(@"    ").Replace(strOutput, " ");
        strOutput = new Regex(@"  ").Replace(strOutput, " ");
      }
      catch (Exception ex)
      {
        ex.Log();
      }

      //Debug.WriteLine(strOutput);

      return strOutput;
    }

    public static bool SaveWebFileToLocalFile(string srcUrl, string trgFile, bool isImage = true)
    {
      try
      {
        using (var wc = getProxyAndCreds(new Uri(srcUrl)))
        {
          if (!isImage)
            wc.DownloadFile(srcUrl, trgFile); //  <= no control over missing imaes (May 2015)
          else
          {
            var fileBytes = wc.DownloadData(srcUrl);
            var fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];
            if (fileType != null && fileType.Contains("image"))
              System.IO.File.WriteAllBytes(trgFile, fileBytes);
            else
            {
              Trace.WriteLine($"Unable to dnld file: {trgFile}");
              return false; // will be taken care of later by GetBitmapFromFile().
            }
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        ex.Log();
        return false;
      }
    }
    public static bool SaveWebFileToLocalFileAsync(string srcUrl, string trgFile)
    {
      //77 Debug.WriteLine(srcUrl, "DnLd Started: ");
      try
      {
        var wc = getProxyAndCreds(new Uri(srcUrl));

        wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
        wc.DownloadFileCompleted += new AsyncCompletedEventHandler(AsyncCompletedCallBack);
        wc.DownloadFileAsync(new Uri(srcUrl), trgFile);
        return true;
      }
      catch (Exception ex)
      {
        ex.Log();
        return false;
      }
    }
    static void DownloadProgressCallback(object s, DownloadProgressChangedEventArgs e)
    {
      //Debug.WriteLine(e.ProgressPercentage.ToString(), "DnLd % colpete: ");			//MessageBox.Show("Download finished...");
      //this.prgDownload.Value = e.ProgressPercentage;
      //this.lblSize.Text = "Total Size: " + (e.TotalBytesToReceive / 1048576).ToString() + "MB";
      //this.lblReceived.Text = "Bytes Received: " + (e.BytesReceived / 1048576).ToString() + "MB";
      //this.lblPercent.Text = "Progress Percentage: " + e.ProgressPercentage.ToString();
    }
    static void AsyncCompletedCallBack(object s, AsyncCompletedEventArgs c)
    {
      //77 Debug.WriteLine(c.ToString(), "DnLd Finished: ");     //MessageBox.Show("Download finished...");
    }

    [Obsolete]
    public static string GetHtmlCached(string url, TimeSpan rotTime)
    {
      var fn = GetCachedFileNameFromUrl(url);
      if (File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
        return File.ReadAllText(fn);
      else
      {
        var html = GetWebStr(url);
        if (!string.IsNullOrEmpty(html))
          safeFile_WriteAllText(fn, html);
        else
            if (File.Exists(fn))
          return File.ReadAllText(fn);

        return html;
      }
    }
    [Obsolete("AAV-> ")]
    public static string GetHtmlCached(string url, TimeSpan rotTime, long minAcceptableLength, bool discardOldCache)
    {
      //if (Debugger.IsAttached) Debugger.Break();
      var fn = GetCachedFileNameFromUrl(url);
      if (!discardOldCache)
      {
        if (File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
        {
          var fi = new FileInfo(fn);
          if (fi.Length > minAcceptableLength)
            return File.ReadAllText(fn);
        }
      }

      var html = GetWebStr(url);
      if (html != null)
      {
        if (html.Length > minAcceptableLength)
          safeFile_WriteAllText(fn, html);

        if (html.Contains("The remote name could not be resolved") && File.Exists(fn))
          return File.ReadAllText(fn);
      }
      else if (File.Exists(fn))
      {
        return File.ReadAllText(fn);
      }

      return html;
    }

    public static string GetWebStr(string url, bool showErrorReport = true)
    {
      string strOutput = null, er = "";

      try
      {
#if DEBUG_WINDGURU
				Uri ourUri = new Uri(url);            

				// Create a 'WebRequest' object with the specified url. 
				WebRequest myWebRequest = WebRequest.Create(ourUri); 
				Console.WriteLine("\nThe Uri that responded for the Request is   \n{0}\n",myWebRequest.RequestUri);


				// Send the 'WebRequest' and wait for response.
				WebResponse myWebResponse = myWebRequest.GetResponse(); 

				// Use "ResponseUri" property to get the actual Uri from where the response was attained.
				if (ourUri.Equals(myWebResponse.ResponseUri))
					Console.WriteLine("\nRequest Url : {0} was not redirected",url);   
				else
					Console.WriteLine("\nRequest Url : {0} was redirected to {1}",url,myWebResponse.ResponseUri);   
				
				using(StreamReader stream = new StreamReader(myWebResponse.GetResponseStream()))
				{
					string s = stream.ReadToEnd();
					//Console.WriteLine(s);
				}

				// Release resources of response object.
				myWebResponse.Close ( );
  Console.WriteLine("\nThe HttpHeaders are \n{0}",myWebRequest.Headers);

#endif

        var request = WebRequest.Create(url); // WebRequest request = createWebRequest(new Uri(url));
        request.Timeout = 5000; //ms                //if (Debugger.IsAttached) Debugger.Break();

        using (var response = request.GetResponse()) //todo: Nov 2017 : hangs here
        {
          using (var stream = new StreamReader(response.GetResponseStream()))
          {
            return stream.ReadToEnd();
          }
        }
      }
      catch (Exception ex)
      {
        er = ex.Message;
        if (showErrorReport) ex.Log();
        strOutput = $@"<!DOCTYPE html><html><head/> <body>\t{ex.Message}\t</body></html>";
      }
      finally
      {
        //Trace.WriteLine(url + "     <<= " + er, er.Length == 0 ? "~*** + " : "~*** - ");
      }

      return strOutput; //null on error.
    }

    public static string GetHtml_(string url, bool fromCache)
    {
      var html = fromCache ? GetHtmlFromCacheOrWeb(url) : GetHtmlFromWeb(url);
      return html;
    }
    public static string GetHtmlFromCacheOnly(string url)
    {
      var fn = GetCachedFileNameFromUrl(url);
      return File.Exists(fn) ? File.ReadAllText(fn) : null;
    }
    public static string GetHtmlFromCacheOrWeb(string url, TimeSpan acptblAge)
    {
      var fn = "Unknwn";
      try
      {
        fn = GetCachedFileNameFromUrl(url);
        if (File.Exists(fn))
          if (acptblAge == TimeSpan.MaxValue || DateTime.Now - File.GetLastWriteTime(fn) < acptblAge)
            return File.ReadAllText(fn);
      }
      catch (Exception ex) { ex.Log(fn); }

      return GetHtmlFromWeb(url);
    }
    public static string GetHtmlFromCacheOrWeb(string url) => GetHtmlFromCacheOrWeb(url, TimeSpan.MaxValue);
    public static string GetHtmlFromCacheOrWeb(string url, out TimeSpan age)
    {
      var fn = GetCachedFileNameFromUrl(url);
      if (File.Exists(fn)) //  && FileAccess.Read)
      {
        age = DateTime.Now - new FileInfo(fn).LastWriteTime;

        try
        {
          return File.ReadAllText(fn); //below code renders the same exception:
                                       //string rv = ""; using (FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { using (StreamReader sr = new StreamReader(fs)) { while (sr.Peek() >= 0) { rv += sr.ReadToEnd(); } } } return rv;
        }
        catch (Exception ex) { ex.Log(); }
      }

      age = TimeSpan.FromTicks(0);
      return GetHtmlFromWeb(url);
    }

    public static string GetHtmlFromWeb(string url)
    {
      var html = GetWebStr(url);

      if (!string.IsNullOrEmpty(html))
        safeFile_WriteAllText(GetCachedFileNameFromUrl(url), html);

      return html;
    }

    public static string CacheHtmlToUniqueFile(string url)
    {
      var fn = GetCachedFileNameFromUrl(url) + DateTime.Now.ToString("-[yyyy.MM.dd-HH]-") + ".HTML";
      if (File.Exists(fn))
        return File.ReadAllText(fn);
      else
      {
        var html = WebScrap.WebScraper.GetWebStr(url);
        safeFile_WriteAllText(fn, html);
        return html;
      }
    }


    public static string GetCachedFileNameFromUrl_NEW(string url, bool useOneDrive)
    {
      var fn = url.Replace("/", "-").Replace(":", "-").Replace("http", "").Replace("?", "!").Replace("|", "!").Replace("---", "");
      var folder = Path.Combine(
              //useOneDrive ? OneDrive.Folder_Alex(@"web.cache\") : 
              @"C:\temp\web.cache", fn.Split('-')[0]); //TODO: separate perm vs deletable...
      fn = fn.Substring(fn.IndexOf("-") + 1);

      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

      return Path.Combine(folder, fn);
    }

    public static string GetCachedFileNameFromUrl(string url)
    {
      var fn = url.Replace("/", "-").Replace(":", "-").Replace("http", "").Replace("?", "!").Replace("|", "!").Replace("---", "");
      var folder = Path.Combine(//useOneDrive ? OneDrive.Folder_Alex(@"web.cache\") : 
              @"C:\temp\web.cache", fn.Split('-')[0]); //TODO: separate perm vs deletable...
      fn = fn.Substring(fn.IndexOf("-") + 1);

      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

      return Path.Combine(folder, fn);
    }

    [Obsolete("AAV-> Fix & Clarify the logic/intent")]
    public static string GetCachedFileNameFromUrl_(string url, bool downloadIfFileNotExists)
    {
      //return url; //todo: 2016: use direct html urls ... maybe not.
      var fn = GetCachedFileNameFromUrl(url);

      if (downloadIfFileNotExists && !File.Exists(fn))// && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
      {
        SaveWebFileToLocalFile(url, fn); // throw new Exception("Nust simplify to remove SaveWebImageToFile()"); // 
      }

      return fn;
    }
    [Obsolete("AAV-> Fix & Clarify the logic/intent")]
    public static string GetCachedFileNameFromUrl_Old_1(string url, TimeSpan rotTime)
    {
      var fn = GetCachedFileNameFromUrl(url);

      if (!File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
      {
        throw new Exception("Nust simplify to remove SaveWebImageToFile()"); // SaveWebImageToFile(url, fn);
      }

      return fn;
    }
  }
}
/*
http://stackoverflow.com/questions/14758917/c-sharp-download-file-from-the-web-with-login


  C# download file from the web with login

up vote
3
down vote
favorite
I can already login to the web page with redirect (i am saving cookies) with this code

   CookieCollection cookies = new CookieCollection();
        HttpWebRequest cookieRequest = (HttpWebRequest)WebRequest.Db("https://www.loginpage.com/"); 
        cookieRequest.CookieContainer = new CookieContainer();
        cookieRequest.CookieContainer.Add(cookies);
        HttpWebResponse cookieResponse = (HttpWebResponse)cookieRequest.GetResponse();
        cookies = cookieResponse.Cookies;

        string postData = "name=********&password=*********&submit=submit";
        HttpWebRequest loginRequest = (HttpWebRequest)WebRequest.Db("https://www.loginpage.com/");
        loginRequest.CookieContainer = new CookieContainer();
        loginRequest.CookieContainer.Add(cookies);
        loginRequest.Method = WebRequestMethods.Http.Post;
        loginRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
        loginRequest.AllowWriteStreamBuffering = true;
        loginRequest.ProtocolVersion = HttpVersion.Version11;
        loginRequest.AllowAutoRedirect = true;
        loginRequest.ContentType = "application/x-www-form-urlencoded";

        byte[] byteArray = Encoding.ASCII.GetBytes(postData);
        loginRequest.ContentLength = byteArray.Length;
        Stream newStream = loginRequest.GetRequestStream(); //open connection
        newStream.Write(byteArray, 0, byteArray.Length); // Send the data.
        newStream.Close ( );
This works fine, but i need to download .xls file from there, it is located here (for example)

https://www.loginpage.com/export_excel.php?export_type=list
for this i tried this code

     HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Db("https://www.loginpage.com/export_excel.php?export_type=list");
        HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();
        Stream str = ws.GetResponseStream();
        byte[] inBuf = new byte[100000];
        int bytesReadTotal = 0;
        string path = @"d:\test.xlsx";
        FileStream fstr = new FileStream(path, FileMode.Db, FileAccess.Write);
        while (true)
        {
            int n = str.Read(inBuf, 0, 100000);
            if ((n == 0) || (n == -1))
            {
                break;
            }

            fstr.Write(inBuf, 0, n);

            bytesReadTotal += n;
        }
        str.Close ( );
        fstr.Close ( );
but its not working and now i am stuck with this

        string dLink = "https://www.loginpage.com/export_excel.php?export_type=list";
        HttpWebRequest fileRequest = (HttpWebRequest)HttpWebRequest.Db(dLink);
        fileRequest.CookieContainer = new CookieContainer();
        fileRequest.CookieContainer.Add(cookies);
        fileRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
        HttpWebResponse fileResponse = (HttpWebResponse)fileRequest.GetResponse();

        for (int i = 0; i < fileResponse.Headers.Count; ++i)
        richTextBox1.Text += "\nHeader Name: " + fileResponse.Headers.Keys[i] + ", Value :" + fileResponse.Headers[i];
Of course it is not downloading the file. I am trying to get headers now to just understand what I am getting from the web? I have already downloaded some files with my script from fil esharing pages like rghost or filehippo, but this one is not working.

c# http
shareimprove this question
edited Feb 7 '13 at 20:51

asked Feb 7 '13 at 19:03

Nerfair
82711634
2	  	
Have you considered trying to refactor your code and use WebClient to do the download..? or do you have to stick with HttpWebRequest..? –  MethodMan Feb 7 '13 at 19:09
1	  	
@DJKRAZE HttpWebRequest is usually more flexible than WebClient, since WebClient uses HttpWebRequest underneath. WebClient makes usage easier but also remove sometimes much needed flexibility. –  zespri Feb 7 '13 at 19:48
  	  	
I dont have to, i just need to download this file any possible way :) –  Nerfair Feb 7 '13 at 20:50
add a comment
2 Answers
activeoldestvotes
up vote
5
down vote
accepted
This should do the job!

        CookieContainer cookieJar = new CookieContainer();
        CookieAwareWebClient http = new CookieAwareWebClient(cookieJar);

        string postData = "name=********&password=*********&submit=submit";
        string response = http.UploadString("https://www.loginpage.com/", postData);

        // validate your login! 

        http.DownloadFile("https://www.loginpage.com/export_excel.php?export_type=list", "my_excel.xls");
I have used CookieAwareWebClient

public class CookieAwareWebClient : WebClient
{
    public CookieContainer CookieContainer { get; set; }
    public Uri Uri { get; set; }

    public CookieAwareWebClient()
        : this(new CookieContainer())
    {
    }

    public CookieAwareWebClient(CookieContainer cookies)
    {
        this.CookieContainer = cookies;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        if (request is HttpWebRequest)
        {
            (request as HttpWebRequest).CookieContainer = this.CookieContainer;
        }
        HttpWebRequest httpRequest = (HttpWebRequest)request;
        httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        return httpRequest;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
        WebResponse response = base.GetWebResponse(request);
        String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

        if (setCookieHeader != null)
        {
            //do something if needed to parse out the cookie.
            if (setCookieHeader != null)
            {
                Cookie cookie = new Cookie(); //create cookie
                this.CookieContainer.Add(cookie);
            }
        }
        return response;
    }
}
Source & Credit for : CookieAwareWebClient










  http://stackoverflow.com/questions/9841344/c-sharp-https-login-and-download-file



  https://social.msdn.microsoft.com/Forums/en-US/ab2e29c3-540b-48fc-ad90-b0d170f09352/how-do-i-login-to-a-website-and-download-files-in-c?forum=netfxnetcom


  ure you can do that in c#

the login page when you click sign in send the user name and password to a particular address like normal login webform you need to know this address and the paramters that send with it to allow you to login and the method"get\set" then you can directly use  httpwebrequest and response with this login address in normal cases this page send to you a cookie you can use request.cookiecontainer and use it with every page you request from this site something like this note this is the way of the site that i use may be your site has different way


HttpWebRequest request;
HttpWebResponse response;
CookieContainer cookies;
 
string url = string.Format("http://site.com/login?.login={0}&passwd={1}", cboUserName.Text, txtPassWord.Text);
request = (HttpWebRequest)WebRequest.Db(url);
request.AllowAutoRedirect = false;
request.CookieContainer = new CookieContainer();
response = (HttpWebResponse)request.GetResponse();
if (response.StatusCode != HttpStatusCode.Found)
{
//ToDo: if the page wasn't found raise Exception
//instead of this textmessage
MessageBox.Show("Something Wrong");
response.Close ( );
request.KeepAlive = false;
return;
}
cookies = request.CookieContainer;
response.Close ( );
request = (HttpWebRequest)WebRequest.Db(http://site.com/Reqyestedoage.html);
request.AllowAutoRedirect = false;
request.CookieContainer = cookies;
response = (HttpWebResponse)request.GetResponse();
using (Stream s = response.GetResponseStream())
{
StreamReader sr = new StreamReader(s);
string line;
while (!sr.EndOfStream)
{
//todo read the page contents
}
 
 
if you have the same senario then you just use the cookies container with every request from this site

i don't know about web.conversation but there is a webClient in C# also you can use it

hope this h


  
  */
