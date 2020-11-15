using AAV.Sys.Ext;
//using AsLink;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebScrap;

namespace EnvironmentCanadaScrap
{
  public partial class EnvntCanadaScraper
  {
    public static List<EnvironmentCanadaData> Past24hourAtButtonville(bool fromCache)
    {
      string s = "", sDate = "",
        html = fromCache ?
        WebScraper.GetHtmlFromCacheOrWeb(_urlPast24hr) :
        WebScraper.GetHtmlFromWeb(_urlPast24hr);

      List<EnvironmentCanadaData> ecdList = new List<EnvironmentCanadaData>();

      if (html == null || html.Length < 25000)
        return ecdList;

      HtmlDocument doc = new HtmlDocument(); doc.LoadHtml(html);       //note: var doc = new HtmlWeb().Load(file); allows  "//table/tbody/tr"
      foreach (HtmlNode tr in doc.DocumentNode.SelectNodes("//table/tbody"))
      {
        //77 Debug.WriteLine("\n============ tr.ChildNodes.Count {0}:", tr.ChildNodes.Count());

        foreach (HtmlNode r in tr.ChildNodes.Where(n => n.ChildNodes.Any()))
        {
          HtmlNodeCollection c = r.ChildNodes;
          int cnt = c.Count();

          //77 Debug.Write($"\n{c.Count()}:");
          int i = 0;
          foreach (HtmlNode t in c) Debug.Write($" {i++}:'{t.InnerText.Replace("\n", "").Replace("\t", "").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim()}' ");
          if (c.Count() == 3)
          {
            sDate = c[1].InnerText.Trim();
          }
          else if (c.Count() >= 17)
          {
            try
            {
              EnvironmentCanadaData e4 = new EnvironmentCanadaData { TempAir = -999 };
              e4.TakenAt = Convert.ToDateTime(sDate + ' ' + c[1].InnerText);

              e4.Conditions = c[3].InnerText;

              string[] c5 = c[5].InnerText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
              if (c5.Count() > 1)
                e4.TempAir = double.Parse(c5[1].Trim(' ').Trim('\n').Trim(' ').Trim('\n').Trim('↑').Trim('↓').Trim('(').Trim(')').Replace("(", "").Trim());
              else
                e4.TempAir = double.Parse(c[5].InnerText.Trim(' ').Trim('\n').Trim(' ').Trim('\n').Trim('↑').Trim('↓').Trim());

              string[] c7 = c[7].InnerText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
              if (c7.Count() > 1)
                e4.Humidity = double.Parse(c7[1].Trim(' ').Trim('\n').Trim(' ').Trim('\n').Trim('↑').Trim('↓').Trim('(').Trim(')').Replace("(", "").Trim());
              else
                e4.Humidity = double.Parse(c[7].InnerText);

              //var c9 = c[9].InnerText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
              //if (c9.Count() > 1)
              //  e4.DewPoint = double.Parse(c9[1].Trim(' ').Trim('\n').Trim(' ').Trim('\n').Trim('↑').Trim('↓').Trim('(').Trim(')').Replace("(", "").Trim());
              //else
              //  e4.DewPoint = double.Parse(c[9].InnerText);

              e4.Pressure   /**/ = double.Parse(c[cnt - 8].InnerText);
              e4.Visibility /**/ = double.Parse(c[cnt - 4].InnerText);
              if (cnt != 27 && !c[13].InnerText.Contains("*"))
                e4.Humidex    /**/ = double.Parse(c[13].InnerText);

              s = c[11].InnerText.Trim();
              string[] w = s.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
              switch (w.Length)
              {
                case 1: e4.WindDir = w[0]; e4.WindKmH = 0; break;
                case 2: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); break;
                case 3: break;
                case 4: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); e4.WindGust = Convert.ToDouble(w[3]); break;
                default: break;
              }

              if (e4 != null && e4.TempAir != -999)
                ecdList.Add(e4);
            }
            catch (Exception ex) { ex.Log(); }
          }
        }
      }

      //old: for (int curpos = 0, i = 0; i < 25; i++)			{				var  e = process1hourButtonvilleLikeEntry(ref s, ref sDate, ref html, ref curpos);				if (e != null && e.Pressure > 0)					ecdList.Add(e);			}

      //`for (int i = 0; i < ecdList.Count; i++) Console.WriteLine("{0,2}) {1}", i, ecdList[i].ToString());

      return ecdList;
    }
    public static List<EnvironmentCanadaData> Fore24hourAtButtonville(bool fromCache)
    {
      string s = "", sDate = "",
        html = fromCache ?
        WebScraper.GetHtmlFromCacheOrWeb(_urlFore24hr) :
        WebScraper.GetHtmlFromWeb(_urlFore24hr);

      List<EnvironmentCanadaData> ecdList = new List<EnvironmentCanadaData>();

      if (html == null || html.Length < 25000)
        return ecdList;

      HtmlDocument doc = new HtmlDocument(); doc.LoadHtml(html);       //note: var doc = new HtmlWeb().Load(file); allows  "//table/tbody/tr"
      foreach (HtmlNode tr in doc.DocumentNode.SelectNodes("//table/tbody"))
      {
        //77 Debug.WriteLine("\n============ {0}:", tr.ChildNodes.Count());

        foreach (HtmlNode r in tr.ChildNodes)//.Where(n => n.ChildNodes.Count() > 0))
        {
          HtmlNodeCollection c = r.ChildNodes;
          //77 Debug.WriteLine("\n------------        {0}:", c.Count());
          int i = 0;
          foreach (HtmlNode t in c) Debug.Write(string.Format("{0,3}:{1} \t", i++, t.InnerText.Trim()));
          if (c.Count() == 3)
          {
            sDate = c[1].InnerText.Trim();
          }
          else if (c.Count() >= 11)
          {
            try
            {
              EnvironmentCanadaData e4 = new EnvironmentCanadaData { TempAir = -999 };
              e4.TakenAt = Convert.ToDateTime(sDate + ' ' + c[1].InnerText);

              e4.TempAir = double.Parse(c[3].InnerText.Trim(' ').Trim('\n').Trim(' ').Trim('\n').Trim('↑').Trim('↓').Trim());
              if (c.Count() > 11 && double.TryParse(c[11].InnerText, out double dbl)) e4.Humidex = dbl;
              e4.Conditions = c[5].InnerText.Trim();

              //e4.Humidity = double.Parse(c[7].InnerText);
              //e4.DewPoint = double.Parse(c[9].InnerText);
              //e4.Pressure = double.Parse(c[13].InnerText);
              //e4.Visibility = double.Parse(c[15].InnerText);

              s = c[9].InnerText.Trim();
              string[] w = s.Split(new char[] { ' ', ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
              switch (w.Length)
              {
                case 1: e4.WindDir = w[0]; e4.WindKmH = 0; break;
                case 2: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); break;
                case 3: break;
                case 4: e4.WindDir = w[0]; e4.WindKmH = Convert.ToDouble(w[1]); e4.WindGust = Convert.ToDouble(w[3]); break;
                default: break;
              }

              if (e4 != null && e4.TempAir != -999) ecdList.Add(e4);
            }
            catch (Exception ex) { Trace.WriteLine(ex.Message, ">>> " + System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodInfo.GetCurrentMethod().Name); }
          }
        }
      }

      //old: for (int curpos = 0, i = 0; i < 25; i++)			{				var  e = process1hourButtonvilleLikeEntry(ref s, ref sDate, ref html, ref curpos);				if (e != null && e.Pressure > 0)					ecdList.Add(e);			}

      //`for (int i = 0; i < ecdList.Count; i++) Console.WriteLine("{0,2}) {1}", i, ecdList[i].ToString());

      return ecdList;
    }
  }
}
