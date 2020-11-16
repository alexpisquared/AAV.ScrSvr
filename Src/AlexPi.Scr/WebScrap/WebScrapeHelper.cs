using System;

namespace WebScrap
{
  public class WebScrapeHelper // latest to use.
  {
    public static int GetIntBetween(string s1, string s2, ref string html, ref int curpos)
    {
      string s = GetStringBetween(s1, s2, ref html, ref curpos).Replace(",", "");

      return int.TryParse(s, out int d) ? d : -1;
    }
    public static double GetDoubleBetween(string s1, string s2, ref string html, ref int curpos)
    {
      string s = GetStringBetween(s1, s2, ref html, ref curpos);

      return double.TryParse(s, out double d) ? d : -1;
    }
    public static double? GetNlblDoubleBetween(string s1, string s2, ref string html, ref int curpos)
    {
      string s = GetStringBetween(s1, s2, ref html, ref curpos);

      if (double.TryParse(s, out double d)) return d; else return null;
    }
    public static string GetStringBetween(string s1, string s2, ref string html)
    {
      int curpos = 0;
      return GetStringBetween(s1, s2, ref html, ref curpos);
    }
    public static string GetStringBetween(string s1, string s2, ref string html, ref int curpos)
    {
      if (curpos < 0) //
        curpos = 0; //				return "";

      int l1 = s1.Length;
      int l2 = s2.Length;

      int p1 = html.IndexOf(s1, curpos);
      if (p1 < 0)
      {
        //curpos = -1;
        return "";
      }
      //Debug.WriteLine("");
      //Debug.WriteLine(html.Substring(p1, l1));
      //Debug.WriteLine(html.Substring(p1, 111));

      int p2 = html.IndexOf(s2, p1 + l1);
      if (p2 < p1)
      {
        //curpos = -2;
        return "";
      }

      curpos = p2 + l2;

      return html.Substring(p1 + l1, p2 - p1 - l1).Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
    }
    public static DateTime GetDateTimeBetween(string s1, string s2, ref string html, ref int curpos)
    {
      string s = GetStringBetween(s1, s2, ref html, ref curpos).Replace("EST", "").Replace("EDT", "");

      return DateTime.TryParse(s, out DateTime d) ? d : DateTime.MinValue;
    }
  }
}
