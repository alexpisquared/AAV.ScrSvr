//using System;
//using System.Diagnostics;
//using System.Collections.Generic;
////using AsLink;
//using AAV.Sys.Ext;

//namespace EnvironmentCanadaScrap
//{
//	public class EnvironmentCanadaMarineMeasure
//	{
//		readonly int lightWindMaximum = 15;//AppSettings.Instance.LightWindMaximum;
//		public int LagInMinutes = 0;// LoggedAt_NotUsed = DateTime.MinValue;//DateTime.Now;
//		public DateTime Observed = DateTime.MinValue;
//		public double? AirTemp = -273;//null;
//		public double? Pressure = -1;//null;
//		public double? WaveHeight = -1;//null;
//		public int? WavePeriod = -1;//null;
//		public int? SeaTemp = -273;//null;
//		public string WindDir = "";
//		public int? Wind = -1;//null;
//		public int? WindGust = -1;//null;
//		public string Notes = "";
//		public int? WindForecast = -1;//null;
//		//DateTime _dt2007 = new DateTime(2007, 1, 1);

//		bool valid = true;
//		public bool Valid
//		{
//			get { return valid; }
//		}

//		public EnvironmentCanadaMarineMeasure()
//		{
//		}
//		public EnvironmentCanadaMarineMeasure(DateTime Observed_, double AirTemp_, double Pressure_, double WaveHeight_, int WavePeriod_, int SeaTemp_, string WindDir_, int Wind_, string Notes_)
//		{
//			Observed = Observed_;
//			AirTemp = AirTemp_;
//			Pressure = Pressure_;
//			WaveHeight = WaveHeight_;
//			WavePeriod = WavePeriod_;
//			SeaTemp = SeaTemp_;
//			WindDir = WindDir_;
//			Wind = Wind_;
//			Notes = Notes_;
//		}

//		public static List<EnvironmentCanadaMarineMeasure> LoadFrom24hourWebPage(string webPage)
//		{
//			List<EnvironmentCanadaMarineMeasure> wc = new List<EnvironmentCanadaMarineMeasure>();

//			try
//			{

//				int pos = 0;
//				string[] da = WebScrap.WebScrapeHelper.GetStringBetween("</h3><span class=\"issuedTime\">", "</span><span class=\"issuedTime\">", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				string sdd = string.Format("{0} {1} {2} {3} {4} ", da[0], da[1], da[3], da[4], da[5]);
//				//					Observed = DateTime.Parse(sdd); Console.WriteLine(Observed.ToLongTimeString());

//				da = WebScrap.WebScrapeHelper.GetStringBetween("knots</a>)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;", " " }, StringSplitOptions.RemoveEmptyEntries);
//				//WindDir = da[0];
//				//Wind = Convert.ToInt32(da[1]);
//				//WindGust = Convert.ToInt32(da[3]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("kPa)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "<span ", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				//Pressure = Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Wave height&nbsp;<span>(m)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				//WaveHeight = da[0] == "N/A" ? -2 : Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Air temperature&nbsp;<span>(&deg;C)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				//AirTemp = Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Wave period&nbsp;<span>(s)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				//WavePeriod = Convert.ToInt32(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Water temperature&nbsp;<span>(&deg;C)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				//SeaTemp = Convert.ToInt32(da[0]);

//			}
//			catch (Exception ex)
//			{
//				AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
//			}

//			return wc;
//		}
//		public static EnvironmentCanadaMarineMeasure LoadFromWebPage(string webPage)
//		{
//			EnvironmentCanadaMarineMeasure wc = new EnvironmentCanadaMarineMeasure();
//			wc.ParseWebPage(webPage);
//			wc.LagInMinutes = (int)new TimeSpan(DateTime.Now.Ticks - wc.Observed.Ticks).TotalMinutes;
//			return wc;
//		}
//		public static EnvironmentCanadaMarineMeasure LoadFromCsvFileLine(string line)
//		{
//			EnvironmentCanadaMarineMeasure wc = new EnvironmentCanadaMarineMeasure();
//			wc.ParseCsvLine(line);
//			return wc;
//		}

//		public void ParseWebPage(string webPage)
//		{
//			try
//			{
//				int pos = 0;
//				string[] da = WebScrap.WebScrapeHelper.GetStringBetween("</h3><span class=\"issuedTime\">", "</span><span class=\"issuedTime\">", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				string sdd = string.Format("{0} {1} {2} {3} {4} ", da[0], da[1], da[3], da[4], da[5]);
//				Observed = DateTime.Parse(sdd); Console.WriteLine(Observed.ToLongTimeString());

//				da = WebScrap.WebScrapeHelper.GetStringBetween("knots</a>)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;", " " }, StringSplitOptions.RemoveEmptyEntries);
//				WindDir = da[0];
//				Wind = Convert.ToInt32(da[1]);
//				WindGust = Convert.ToInt32(da[3]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("kPa)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "<span ", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				Pressure = Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Wave height&nbsp;<span>(m)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				WaveHeight = da[0] == "N/A" ? -2 : Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Air temperature&nbsp;<span>(&deg;C)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				AirTemp = Convert.ToDouble(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Wave period&nbsp;<span>(s)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				WavePeriod = Convert.ToInt32(da[0]);

//				da = WebScrap.WebScrapeHelper.GetStringBetween("Water temperature&nbsp;<span>(&deg;C)</span></th>", "<td", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				da = WebScrap.WebScrapeHelper.GetStringBetween(">", "</td>", ref webPage, ref pos).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
//				SeaTemp = Convert.ToInt32(da[0]);

//			}
//			catch (Exception ex)
//			{
//                Notes = ex.Log(); ;
//			}


//		}

//		public void ParseCsvLine(string line)
//		{
//			string[] sa = line.Split(',');

//			if (sa[0].Length > 0 && !int.TryParse(sa[0], out LagInMinutes)) { LagInMinutes = -1; Notes += string.Format("sa[0] = '{0}' ", sa[0]); }
//			try
//			{
//				Observed = DateTime.Parse(sa[1]);
//				///!!!!!!!!! Observed = Observed > _dt2007 ? Observed.AddYears(-1) : Observed;
//			}
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[1], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			try { AirTemp = Convert.ToInt32(sa[2]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[2], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			try { Pressure = Convert.ToDouble(sa[3]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[3], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			try { WaveHeight = Convert.ToDouble(sa[4]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[4], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			try { WavePeriod = Convert.ToInt32(sa[5]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[5], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			try { SeaTemp = Convert.ToInt32(sa[6]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[6], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			WindDir = sa[7];
//			try { Wind = Convert.ToInt32(sa[8]); }
//			catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[8], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			if (sa.Length > 10 && sa[10].Length > 0)
//			{
//				try { WindForecast = Convert.ToInt32(sa[10]); }
//				catch (Exception ex) { valid = false; Notes += string.Format("'{0}': {1}. ", sa[10], ex.Message); Console.WriteLine("**Err: " + Notes); }
//			}
//			if (sa.Length > 11)
//				Notes = sa[11];

//			if (SeaTemp == 0 && Pressure == 0)
//				valid = false;
//		}

//		public string CsvLine
//		{
//			get
//			{
//				int exVal = 9;

//				int? i;
//				i = exVal;

//				int j = i ?? 4;

//				bool daytime = 8 <= Observed.Hour && Observed.Hour <= 20;
//				string graph = Wind <= lightWindMaximum ?
//					 "" : //new string((daytime ? '+' : '-'), (Wind + 1) / 2) :
//					 new string((daytime ? 'D' : 'n'), ((Wind ?? 0) - lightWindMaximum) / 2);

//				return string.Format("{0,3:N0},{1},{2:0#},{3,5:N1},{4:N1},{5:0#},{6:0#},{7,-5},{8:0#},{9,-12},{10,2},{11}",
//					 LagInMinutes, //Observed.ToString("yyyy-MM-dd HH:mm:ss"),
//					 Observed.ToString("yyyy-MM-dd HH:mm"),
//					 AirTemp,
//					 Pressure,
//					 WaveHeight,
//					 WavePeriod,
//					 SeaTemp,
//					 WindDir,
//					 Wind,
//					 graph,
//					 WindForecast,
//					 Notes);
//			}
//		}

//	}
//}
