using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AAV.Sys.Helpers
{
  public static class VerHelper
  {
    public static string CurVerStr(string NetFrwk /*= ".NET 4.8"*/) => $"{NetFrwk} - {getTimedVerString()} - {CompileMode}";

    static string getTimedVerString()
    {
      var calng = new FileInfo(Assembly.GetCallingAssembly().Location).LastWriteTime;      // from .NET 4.8 version
      var entry = new FileInfo(Assembly.GetEntryAssembly()?.Location).LastWriteTime;
      var execg = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
      var max = entry > execg ? entry : execg;

      return TimeAgo(max);
    }

    public static string TimeAgo(DateTimeOffset max, bool versionMode = false)
    {
      var dt = (DateTimeOffset.Now - max);
      if (dt < TimeSpan.Zero)
        return "Never";

      return
        dt.TotalSeconds < 10        /**/ ? $"Now!!!"
        : dt.TotalSeconds < 100     /**/ ? $"{dt.TotalSeconds:N0} seconds ago"
        : dt.TotalMinutes < 60      /**/ ? $"{dt.TotalMinutes:N0} minutes ago"
        : dt.TotalHours < 24        /**/ ? $"{dt.TotalHours:N1} hours ago"
        : dt.TotalDays < 10         /**/ ? $"{dt.TotalDays:N1} days ago"
        :                           /**/ (versionMode ? $"{max:yyyy.M.d})" : $"{max:yyyy-MM-dd})");
    }
    public static string Elapsed(TimeSpan elapsed) => elapsed.TotalMilliseconds < 100   /**/ ? $"{elapsed.TotalMilliseconds:N0} ms" :
        elapsed.TotalSeconds < 2          /**/ ? $"{elapsed.TotalSeconds:N2} sec" :
        elapsed.TotalSeconds < 10         /**/ ? $"{elapsed.TotalSeconds:N1} sec" :
        elapsed.TotalSeconds < 100        /**/ ? $"{elapsed.TotalSeconds:N0} sec" :
        elapsed.TotalMinutes < 10         /**/ ? $"{elapsed.TotalMinutes:N1} min" :
        elapsed.TotalMinutes < 60         /**/ ? $"{elapsed.TotalMinutes:N0} min" :
        elapsed.TotalHours < 12           /**/ ? $"{elapsed:hh\\:mm} hr" :
        elapsed.TotalHours < 24           /**/ ? $"{elapsed.TotalHours:N1} hr" :
        elapsed.TotalDays < 2             /**/ ? $"{elapsed.TotalDays:N2} day" :
        elapsed.TotalDays < 10            /**/ ? $"{elapsed.TotalDays:N1} day" :
        elapsed.TotalDays < 100           /**/ ? $"{elapsed.TotalDays:N0} day" :
        elapsed.TotalDays < 1000          /**/ ? $"{(elapsed.TotalDays / 365.25):N2} yr" :
        elapsed.TotalDays < 3652          /**/ ? $"{(elapsed.TotalDays / 365.25):N1} yr" :
                                          /**/   $"{(elapsed.TotalDays / 365.25):N0} yr";
    public static string EtaIn(DateTimeOffset eta)
    {
      var dt = (eta - DateTimeOffset.Now);
      if (dt < TimeSpan.Zero)
        return TimeAgo(eta);

      return
        dt.TotalSeconds < 1   /**/ ? $"Now!!!" :
        dt.TotalSeconds < 100 /**/ ? $"{dt.TotalSeconds:N0} sec" :
        dt.TotalMinutes < 10  /**/ ? $"{dt.TotalMinutes:N1} min" :
        dt.TotalMinutes < 60  /**/ ? $"{dt.TotalMinutes:N0} min" :
        dt.TotalHours < 24    /**/ ? $"{dt.TotalHours:N1} hr" :
        dt.TotalDays < 100    /**/ ? $"{dt.TotalDays:N1} day" :
                              /**/   $"{eta:yyyy-MM-dd})";
    }
    public static string EtaAt(DateTimeOffset eta)
    {
      var dt = (eta - DateTimeOffset.Now);
      if (dt < TimeSpan.Zero)
        return TimeAgo(eta);

      var midnight = DateTimeOffset.Now.Add(TimeSpan.FromDays(1) - DateTimeOffset.Now.TimeOfDay);

      return
        eta < midnight      /**/ ? (dt.TotalMinutes < 10 ? $"{eta:H:mm:ss}" : $"{eta:H:mm}")
        : dt.TotalDays < 07 /**/ ? $"{eta:ddd-dd H:mm}"
        : dt.TotalDays < 30 /**/ ? $"{eta:MMM-dd}"
                            /**/ : $"{eta:yyyy-MMM-dd}";
    }

    public static Version CurVer =>
#if usingSystemDeploymentApplication //  <== define for Release
          System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed ?
          System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion : // z.ves.: ClickOnce deployed.
#endif
          Assembly.GetEntryAssembly()?.GetName()?.Version ?? new Version("No Kidding...");       // BOM.OLP.DAQ -.EXE !!!//          Assembly.GetCallingAssembly().GetName().Version;		// BOM.OLP.ViewModel.DAQ - use this as the most likely to change, thus be reflective of the latest state.//          Assembly.GetExecutingAssembly().GetName().Version;	// BOM.OLP.Common// FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("MM.dd HH"); 

#if DEBUG
    public static string CompileMode => Debugger.IsAttached ? "Dbg-Atchd )(*&^%$" : "Dbg!!!";
#else
public static string CompileMode =>        Debugger.IsAttached ? "Rls-Atchd" : "Rls";
#endif

    public static bool IsVIP => Environment.UserName.ToUpperInvariant().Contains("ALEX");
    public static bool IsMyHomePC => new string[] { "LN1", "VAIO1", "ASUS2", "RAZER1", "NUC2" }.Contains(Environment.MachineName.ToUpperInvariant());
  }
}
