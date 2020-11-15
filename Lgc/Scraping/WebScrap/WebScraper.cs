using System;
using System.ComponentModel;
// encoding
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;

namespace WebScrap
{
	public partial class WebScraperBase
	{
		protected static WebClient getProxyAndCreds(Uri uri)
		{
			WebClient wc = new WebClient();
			wc.Proxy = WebRequest.DefaultWebProxy; //TU: 1/2
			wc.Proxy.Credentials = getICredentials(uri);
			return wc;
		}
		protected static WebRequest createWebRequest(Uri uri)
		{
			WebRequest wr = HttpWebRequest.Create(uri); //TU: 0/2
			wr.Proxy = WebRequest.DefaultWebProxy; //TU: 1/2

			//does not owrk on hte new Lenovo T510; need more smarts and exlicit credentials: //todo: why Default... does not work?
			wr.Proxy.Credentials = getICredentials(uri);

			return wr;
		}
		protected static ICredentials getICredentials(Uri uri)
		{
			return
				WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("AAVproLtd") ? new NetworkCredential("apigida", sABC + " " + s123.ToString(), "AAVproLtd") :
				WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("10.0.10.") ? new NetworkCredential("alexp", sABC.Substring(1) + sABC.Substring(1), "gemc") :
				WebRequest.DefaultWebProxy.GetProxy(uri).Host.ToLower().Contains("10.1.0.1") ? new NetworkCredential("alexp", sABC.Substring(1) + sABC.Substring(1), "gemc") :
				CredentialCache.DefaultNetworkCredentials; //TU: 2/2 //note: is not required for getiing pics and files - html only (3-jun-2010).
		}
		protected static void safeFile_WriteAllText(string file, string text)
		{
			try
			{
				File.WriteAllText(file, text);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);
			}
		}
		const string sABC = ";lkj";
		const int s123 = 345;
	}

	[Obsolete("Use AsLink", true)]
	public partial class WebScraper : WebScraperBase
	{
		public static string Check(string url) //check if onternet is available
		{
			string strOutput = "Disconnected";// null;

			try
			{
				WebRequest request = createWebRequest(new Uri(url));
				request.Timeout = 5000; //ms

#if DEBUG
				//if (Environment.MachineName == "APL-APIGIDA")                    return GetHtmlFromCache(url);
#endif

				using (WebResponse response = request.GetResponse())
				{
					using (StreamReader stream = new StreamReader(response.GetResponseStream()))
					{
						string nootUsed = stream.ReadToEnd();
					}
				}

				return "Connected";
			}
			catch (WebException ex)
			{
				strOutput = "Disconnected (WebException: " + ex.Message + ")";
			}
			catch (Exception ex)
			{
				strOutput = "Disconnected (Exception: " + ex.Message + ")";
			}

			return strOutput; //null on error.
		}
		public static string GetHtml(string url)
		{
			return GetHtml(url, true);
		}
		public static string GetHtml(string url, bool showErrorReport)
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
				myWebResponse.Close();
  Console.WriteLine("\nThe HttpHeaders are \n{0}",myWebRequest.Headers);

#endif

				//return tryWebClient(new Uri(url));

				WebRequest request = createWebRequest(new Uri(url));

#if DEBUG
				//if (Environment.MachineName == "APL-APIGIDA")                    return GetHtmlFromCache(url);
#endif

				using (WebResponse response = request.GetResponse())
				{
					using (StreamReader stream = new StreamReader(response.GetResponseStream()))
					{
						return stream.ReadToEnd();
					}
				}
			}
			catch (Exception ex)
			{
				er = ex.Message;
				if (showErrorReport) Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);
			}
			finally
			{
				Trace.WriteLine(url + "     <<= " + er, er.Length == 0 ? "~*** + " : "~*** - ");
			}

			return strOutput; //null on error.
		}
		public static XDocument GetXml(string url)
		{
			ConsoleColor fc = Console.ForegroundColor;
			XDocument feedXML = null;
			for (int i = 0; i < 3 && feedXML == null; i++)
			{
				try
				{
					//feedXML = XDocument.Load(fe.Url);  ==> for behind the proxy domains:
					WebClient wc = getProxyAndCreds(new Uri(url));
					MemoryStream ms = new MemoryStream(wc.DownloadData(url));
					XmlTextReader rdr = new XmlTextReader(ms);
					feedXML = XDocument.Load(rdr);
				}
				catch (Exception ex)
				{
					//todo: log the error somehow
					//if (MessageBox.Show(string.Format("{0}\n\n {1}\n\n EXCEPTION: {2} \n\n\n Ignore? ", fe.Note, fe.Url, ex.Message), Application.ResourceAssembly.FullName, 
					//  MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.Cancel)
					Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);

					feedXML = null;
				}
				finally
				{
					Console.ForegroundColor = fc;
				}
			}

			return feedXML;
		}

		public static string ScrapAndRemoveHtml(string url)
		{
			string strOutput = WebScraper.GetHtml(url);

			strOutput = WebScraper.RemoveHtml(strOutput);

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
				Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);
			}

			//Debug.WriteLine(strOutput);

			return strOutput;
		}

		public static bool SaveWebFileToLocalFile(string srcUrl, string trgFile, bool logErrors)
		{
			try
			{
#if DEBUG
				//if (Environment.MachineName == "APL-APIGIDA")                  return GetWebImageFromCache(url);
#endif
				getProxyAndCreds(new Uri(srcUrl)).DownloadFile(srcUrl, trgFile);
				//				getProxyAndCreds().DownloadFile(url, System.Web.HttpUtility.UrlDecode(fn));
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);
				return false;
			}
		}
		public static bool SaveWebFileToLocalFileAsync(string srcUrl, string trgFile, bool logErrors)
		{
			Debug.WriteLine(srcUrl, "DnLd Started: ");
			try
			{
				WebClient wc = getProxyAndCreds(new Uri(srcUrl));

				wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
				wc.DownloadFileCompleted += new AsyncCompletedEventHandler(AsyncCompletedCallBack);
				wc.DownloadFileAsync(new Uri(srcUrl), trgFile);
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex, MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name);
				return false;
			}
		}
		private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
		{
			//Debug.WriteLine(e.ProgressPercentage.ToString(), "DnLd % colpete: ");			//MessageBox.Show("Download finished...");
			//this.prgDownload.Value = e.ProgressPercentage;
			//this.lblSize.Text = "Total Size: " + (e.TotalBytesToReceive / 1048576).ToString() + "MB";
			//this.lblReceived.Text = "Bytes Received: " + (e.BytesReceived / 1048576).ToString() + "MB";
			//this.lblPercent.Text = "Progress Percentage: " + e.ProgressPercentage.ToString();
		}
		private static void AsyncCompletedCallBack(object sender, AsyncCompletedEventArgs c)
		{
			Debug.WriteLine(c.ToString(), "DnLd Finished: ");			//MessageBox.Show("Download finished...");
		}

		[Obsolete]
		public static string GetHtmlCached(string url, TimeSpan rotTime)
		{
			string fn = GetCachedFileNameFromUrl(url);
			if (File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
				return File.ReadAllText(fn);
			else
			{
				string html = WebScrap.WebScraper.GetHtml(url);
				if (!string.IsNullOrEmpty(html))
					safeFile_WriteAllText(fn, html);
				else
					if (File.Exists(fn))
						return File.ReadAllText(fn);

				return html;
			}
		}
		[Obsolete]
		public static string GetHtmlCached(string url, TimeSpan rotTime, long minAcceptableLength, bool discardOldCache)
		{
			string fn = GetCachedFileNameFromUrl(url);
			if (!discardOldCache)
			{
				if (File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
				{
					FileInfo fi = new FileInfo(fn);
					if (fi.Length > minAcceptableLength)
						return File.ReadAllText(fn);
				}
			}

			string html = WebScrap.WebScraper.GetHtml(url);
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

		public static string GetHtml(bool fromCache, string url)
		{
			string html;
			if (fromCache)
				html = WebScraper.GetHtmlFromCacheOrWeb(url);
			else
				html = WebScraper.GetHtmlFromWeb(url);
			return html;
		}
		public static string GetHtmlFromCacheOnly(string url)
		{
			string fn = GetCachedFileNameFromUrl(url);
			return File.Exists(fn) ? File.ReadAllText(fn) : null;
		}
		public static string GetHtmlFromCacheOrWeb(string url)
		{
			var fn = GetCachedFileNameFromUrl(url);
			if (File.Exists(fn))
				return File.ReadAllText(fn);

			Beep(8000, 44);
			return GetHtmlFromWeb(url);
		}
		public static string GetHtmlFromCacheOrWeb(string url, out TimeSpan? age)
		{
			var fn = GetCachedFileNameFromUrl(url);
			if (File.Exists(fn))
			{
				age = DateTime.Now - new FileInfo(fn).LastWriteTime;
				return File.ReadAllText(fn);
			}

			age = null;
			Beep(8000, 44);
			return GetHtmlFromWeb(url);
		}

		public static string GetHtmlFromWeb(string url)
		{
			var html = WebScrap.WebScraper.GetHtml(url);

			if (!string.IsNullOrEmpty(html))
				safeFile_WriteAllText(GetCachedFileNameFromUrl(url), html);

			return html;
		}

		public static string CacheHtmlToUniqueFile(string url)
		{
			string fn = GetCachedFileNameFromUrl(url) + DateTime.Now.ToString("-[yyyy.MM.dd-HH]-") + ".HTML"; ;
			if (File.Exists(fn))
				return File.ReadAllText(fn);
			else
			{
				string html = WebScrap.WebScraper.GetHtml(url);
				safeFile_WriteAllText(fn, html);
				return html;
			}
		}

		public static string GetCachedFileNameFromUrl(string url)
		{
			string fn = url.Replace("/", "-").Replace(":", "-").Replace("http", "").Replace("?", "!").Replace("---", "");
			string folder = Path.Combine(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SkyDrive", "UserFolder", null).ToString(), @@".OfLn\web.cache\", fn.Split('-')[0]); //TODO: separate perm vs deletable...
			fn = fn.Substring(fn.IndexOf("-") + 1);

			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			return Path.Combine(folder, fn);
		}
		//public static string GetCachedFileNameFromUrl(string url, bool downloadIfFileNotExists)
		//{
		//	string fn = GetCachedFileNameFromUrl(url);

		//	if (downloadIfFileNotExists && !File.Exists(fn))// && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
		//	{
		//		SaveWebImageToFile(url, fn);
		//	}

		//	return fn;
		//}
		//public static string GetCachedFileNameFromUrl(string url, TimeSpan rotTime)
		//{
		//	string fn = GetCachedFileNameFromUrl(url);

		//	if (!File.Exists(fn) && File.GetLastWriteTime(fn).Add(rotTime) > DateTime.Now)
		//	{
		//		SaveWebImageToFile(url, fn);
		//	}

		//	return fn;
		//}

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern bool Beep(int freq, int dur);
	}

}
