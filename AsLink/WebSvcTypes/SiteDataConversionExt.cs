using System;
using System.Diagnostics;
using System.Globalization;

namespace AsLink
{
    public static class SiteDataConversionExt
  {
    public static DateTime GetDateTimeLcl(this ulong val)
    {
      if (DateTime.TryParseExact(val.ToString(), new string[] { "yyyyMMddHHmm", "yyyyMMddHHmmss" }, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime rv))
        return rv.ToLocalTime(); //201712290100 20171228180800
      else
      {
        if (Debugger.IsAttached) Debugger.Break();
        return DateTime.Now;
      }
    }
    public static DateTime GetDateTimeLcl(this string val)
    {
      if (val == null)
        return DateTime.Now;

      if (DateTime.TryParseExact(val, new string[] { "yyyyMMddHHmm", "yyyyMMddHHmmss" }, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime rv))
        return rv.ToLocalTime(); //201712290100 20171228180800

      if (Debugger.IsAttached)
        Debugger.Break();

      return DateTime.Now;
    }
    public static DateTime GetDateTimeUtc(this ulong val)
    {
      if (DateTime.TryParseExact(val.ToString(), new string[] { "yyyyMMddHHmm", "yyyyMMddHHmmss" }, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime rv))
        return rv; //201712290100 20171228180800
      else
      {
        if (Debugger.IsAttached) Debugger.Break();
        return DateTime.Now;
      }
    }
    public static decimal GetDecimal(this string val, decimal ex = -123.456m)
    {
      if (val == null)
        return ex;

      if (decimal.TryParse(val, out decimal rv))
        return rv; //201712290100 20171228180800

      if (Debugger.IsAttached)
        Debugger.Break();

      return ex;
    }
  }
}
