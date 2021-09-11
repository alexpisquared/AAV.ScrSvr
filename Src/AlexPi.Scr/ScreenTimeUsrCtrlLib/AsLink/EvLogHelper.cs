using AAV.Sys.Ext;
using AAV.WPF.Ext;
using Db.EventLog.Main;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace AsLink
{
  public static partial class EvLogHelper //2021-09: old RO version. Tried to replace with C:\g\TimeTracking\N50\TimeTracking50\TimeTracker\AsLink\EvLogMngr.cs - too many diffs. 
  {
    const int _ssrUp = 7101, _ssrDn = 7102, _bootUp_12 = 12, _bootDn_13 = 13, _syTime_01 = 1; // when waking from hibernation: 12 is nowhere to be seen, 1 is there.

    static readonly string[] _paths = new[] { _app, _sys };

    public static async Task<double> GetWkSpanForTheDay(DateTime trgDate)
    {
      double rv = 0;
      await Task<SortedList<DateTime, int>>.Run(() => GetEoisForTheDay(trgDate)).ContinueWith(_ =>
      {
        var eois = _.Result;
        if (eois.Any())
        {
          if (eois.Count() == 1)
          {
            var start = eois.First().Key;
            if ((DateTime.Now - trgDate).TotalHours < 24) // if today
              rv = (DateTime.Now - start).TotalHours;
          }
          else if (eois.Count() > 1)
          {
            var finalEvent = eois.Last().Key;
            var lastScvrUp = DateTime.Now;
            if ((DateTime.Now - trgDate).TotalHours < 24) // if today
              lastScvrUp = DateTime.Now;
            else
              lastScvrUp = (
                          eois.Any(r => r.Value == (int)EvOfIntFlag.ScreenSaverrUp || r.Value == (int)EvOfIntFlag.ShutAndSleepDn) ?
                        eois.Where(r => r.Value == (int)EvOfIntFlag.ScreenSaverrUp || r.Value == (int)EvOfIntFlag.ShutAndSleepDn).Last() : eois.Last()).Key;

            rv = ((lastScvrUp < finalEvent ? lastScvrUp : finalEvent) - eois.First().Key).TotalHours;
          }


          //if ((DateTime.Now - trgDate).TotalHours < 24) // if today
          //{
          //    if (eois.Count() == 1)
          //    {
          //        var start = eois.First().Key;
          //        rv = (DateTime.Now - start).TotalHours;
          //    }
          //    else if (eois.Count() > 1)
          //    {
          //        var finalEvent = eois.Last().Key;
          //        var lastScvrUp = DateTime.Now;

          //        rv = ((lastScvrUp < finalEvent ? lastScvrUp : finalEvent) - eois.First().Key).TotalHours;
          //    }
          //    else
          //    {
          //        var finalEvent = eois.Last().Key;
          //        var lastScvrUp = (
          //                  eois.Any(r => r.Value == (int)EvOfIntFlag.ScreenSaverrUp || r.Value == (int)EvOfIntFlag.ShutAndSleepDn) ?
          //                eois.Where(r => r.Value == (int)EvOfIntFlag.ScreenSaverrUp || r.Value == (int)EvOfIntFlag.ShutAndSleepDn).Last() : eois.Last()).Key;

          //        rv = ((lastScvrUp < finalEvent ? lastScvrUp : finalEvent) - eois.First().Key).TotalHours;
          //    }

          //}
        }
      }, TaskScheduler.FromCurrentSynchronizationContext());

      return rv;
    }

    public static SortedList<DateTime, int> GetEoisForTheDay(DateTime trgDate) => GetAllUpDnEvents(trgDate, trgDate.AddDays(.999999));
    public static SortedList<DateTime, int> GetAllUpDnEvents(DateTime a, DateTime b)
    {
      var lst = new SortedList<DateTime, int>();

      try
      {
        collect(lst, qryBootAndWakeUps(a, b), (int)EvOfIntFlag.BootAndWakeUps);
        collect(lst, qryShutAndSleepDn(a, b), (int)EvOfIntFlag.ShutAndSleepDn);
        collect(lst, qryScrSvr(_ssrDn, a, b), (int)EvOfIntFlag.ScreenSaverrDn);
        collect(lst, qryScrSvr(_ssrUp, a, b), (int)EvOfIntFlag.ScreenSaverrUp);

        foreach (var path in _paths)
        {
          add1stLast(a, b, lst, path);
        }
      }
      catch (Exception ex) { ex.Pop(); }

      return lst;
    }

    static void add1stLast(DateTime a, DateTime b, SortedList<DateTime, int> lst, string path)
    {
      return; // no events found
      (var min, var max) = get1rstLastEvents(qryAll(path, a, b));
      if (min == DateTime.MaxValue)
        return; // no events found

      if (lst.Count < 1)
        lst.Add(min, (int)EvOfIntFlag.Day1stAmbiguos);
      else
      {
        if ((lst.Min(r => r.Key) - min).TotalSeconds > +30) // only if > 30 sec
          lst.Add(min, (int)EvOfIntFlag.Day1stAmbiguos);
        else
          Debug.WriteLine("+???");
      }

      if (lst.Count < 1)
        lst.Add(max, (int)EvOfIntFlag.ShutAndSleepDn);
      else
      {
        if ((lst.Max(r => r.Key) - max).TotalSeconds < -30)
          lst.Add(max, (int)EvOfIntFlag.ShutAndSleepDn); // any idea what is that for? It adds a ShuDn event ~5 min ago from now ... but why? (Mar2019)
        else
          Debug.WriteLine("-???");
      }
    }

    static void collect(SortedList<DateTime, int> lst, string qry, int evOfIntFlag)
    {
      using (var reader = GetELReader(qry))
      {
        for (var ev = reader.read(); ev != null; ev = reader.read())
        {
          //32 Debug.Write($" *** ev time: {ev.TimeCreated.Value:y-MM-dd HH:mm:ss.fff} - {evOfIntFlag}={(EvOfIntFlag)evOfIntFlag,}:"); Debug.Assert(!lst.ContainsKey(ev.TimeCreated.Value), $" -- already added {ev.TimeCreated.Value} - {evOfIntFlag}");

          if (lst.Any(r => r.Value == evOfIntFlag) && (ev.TimeCreated.Value - lst.Where(r => r.Value == evOfIntFlag).Max(r => r.Key)).TotalSeconds < 60) // if same last one is < 60 sec ago.
          {
            //32 Debug.WriteLine($" -- IGNORING  to allow power-offs (which are out of order in ev.log!!!) to flag the actual state.");
          }
          else
          {
            //32 Debug.WriteLine($" -- LOGGING.");
            lst.Add(ev.TimeCreated.Value, evOfIntFlag);
          }
        }
      }
    }

    static EventRecord read(this EventLogReader reader) { try { return reader.ReadEvent(); } catch (Exception ex) { ex.Log(); return null; } }

    static (DateTime min, DateTime max) get1rstLastEvents(string qry)
    {
      var lst = new List<DateTime>();
      using (var reader = GetELReader(qry))
      {
        for (var ev = reader.read(); ev != null; ev = reader.read())
          lst.Add(ev.TimeCreated.Value);
      }

      return lst.Count < 1 ? (DateTime.MaxValue, DateTime.MinValue) : (lst.Min(), lst.Max());
    }

    static EventLogReader GetELReader(string qry, string path = "System") => new EventLogReader(new EventLogQuery(path, PathType.LogName, qry));


    public static TimeSpan CurrentSessionDuration() // lengthy operation: 100 ms.
    {
      var lastWakeTime = GetDaysLastWakeBoot(DateTime.Today);
      var lastSsDnTime = GetDaysLastSsDnTime(DateTime.Today);
      var lastSsUpTime = GetDaysLastSsUpTime(DateTime.Today);
      var lastBootTime = GetDaysLastBootUpTime(DateTime.Today);

      var lastUp = DateTime.FromOADate(Math.Max(lastBootTime.ToOADate(), Math.Max(lastWakeTime.ToOADate(), lastSsDnTime.ToOADate())));

      return lastSsUpTime > lastUp ? TimeSpan.Zero : DateTime.Now - lastUp;
    }

    public static void ta()
    {
      var apl1hr = $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[TimeCreated[timediff(@SystemTime) &lt;= 299000000]]]</Select></Query></QueryList>";

      using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, apl1hr)))
      {
        for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
        {
          //77 Debug.Write($"\n {er.TimeCreated}  {er.TaskDisplayName}    {er.ProviderName}");

          for (var i = 0; i < er.Properties.Count; i++)
          {
            Debug.Write($"\t {er.Properties[i].Value} ");
          }
        }
      }
    }

    public static string DailyReport(DateTime hr00ofTheDate, out TimeSpan tup, out TimeSpan tdn)
    {
      try
      {
        tup = GetTotalPowerUpTimeForTheDay(hr00ofTheDate);
        tdn = GetTotalIdlePlusScrsvrUpTimeForTheDate(hr00ofTheDate);
        return ($"{hr00ofTheDate:ddd}:{tup,5:h\\:mm} -{tdn,5:h\\:mm} ={(tup - tdn),5:h\\:mm}");
      }
      catch (Exception ex) { tup = tdn = TimeSpan.MinValue; return ex.Message; } // ex.Log(); }
    }

    public static DateTime GetDays1rstGenUp(DateTime hr00ofTheDate)
    {
      var t = new[] { GetDays1rstBootUpTime(hr00ofTheDate), GetDays1rstSsDnTime(hr00ofTheDate), GetDays1rstWakeTime(hr00ofTheDate) };


      if (t.Count(r => r == hr00ofTheDate) == 3) return hr00ofTheDate; // never off
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      if (t.Count(r => r == hr24ofTheDate) == 3) return hr24ofTheDate; // never on

      var min = t.Where(r => r > hr00ofTheDate).Min();
      var max = t.Where(r => r < hr24ofTheDate).Max();

      //if (ts == hr00ofTheDate && ti == hr00ofTheDate && tb == hr00ofTheDate) return hr00ofTheDate; // never off

      //if (ts == hr24ofTheDate && ti == hr24ofTheDate && tb == hr24ofTheDate) return hr24ofTheDate; // never on


      //if (ts != hr00ofTheDate && ti != hr00ofTheDate) return DateTime.FromOADate(Math.Min(ts.ToOADate(), ti.ToOADate()));
      //if (ts != hr24ofTheDate && ti != hr24ofTheDate) return DateTime.FromOADate(Math.Min(ts.ToOADate(), ti.ToOADate()));

      //todo:

      return min; //
    }
    public static DateTime GetDays1rstGenDn(DateTime hr00ofTheDate)
    {
      var ts = GetDaysLastSsUpTime(hr00ofTheDate);
      var ti = GetDaysLastSleepTime(hr00ofTheDate);
      if (ts == hr00ofTheDate && ti == hr00ofTheDate) return hr00ofTheDate; // never off

      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      if (ts == hr24ofTheDate && ti == hr24ofTheDate) return hr24ofTheDate; // never on


      if (ts != hr00ofTheDate && ti != hr00ofTheDate) return DateTime.FromOADate(Math.Max(ts.ToOADate(), ti.ToOADate()));
      if (ts != hr24ofTheDate && ti != hr24ofTheDate) return DateTime.FromOADate(Math.Max(ts.ToOADate(), ti.ToOADate()));

      //todo:

      return hr00ofTheDate; //
    }

    public static DateTime GetDays1rstSsDnTime(DateTime hr00ofTheDate)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr24ofTheDate;
      var apl1hr = $@"<QueryList><Query Id='0' Path='{_aavLogName}'><Select Path='{_aavLogName}'>*[System[Provider[@Name='{_aavSource}'] and (Level=4 or Level=0) and ( (EventID = {_ssrDn})  and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{hr24ofTheDate.ToUniversalTime():o}'] )]]</Select></Query></QueryList>";
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery(_aavLogName, PathType.LogName, apl1hr)))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv > er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }
    public static DateTime GetDays1rstWakeTime(DateTime hr00ofTheDate)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr24ofTheDate;
      var sleeps = $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{hr24ofTheDate.ToUniversalTime():o}'] ]]</Select></Query></QueryList>";

      try
      {
        using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, sleeps)))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv > er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }
    public static DateTime GetDays1rstBootUpTime(DateTime hr00ofTheDate)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr24ofTheDate;
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, /*qryBootUpsOnly*/qryBootUpTmChg(hr00ofTheDate, hr24ofTheDate)))) //sep 2018
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv > er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }
    public static DateTime GetDaysLastBootUpTime(DateTime hr00ofTheDate, bool ignoreReboots = true)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr00ofTheDate;
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, qryBootUpsOnly(hr00ofTheDate, hr24ofTheDate))))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
          {
            if (!ignoreReboots)
            {
              var bootUpTime = er.TimeCreated.Value;
              using (var reader2 = new EventLogReader(new EventLogQuery("System", PathType.LogName, BootDnWithin5min(bootUpTime))))
              {
                if ((EventLogRecord)reader2.read() != null) // this is a reboot - ignore it since it is not a session start.
                  continue;
              }
            }

            if (rv < er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
          }
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }

    static string BootDnWithin5min(DateTime bootUpTime, int min = -5) => $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Kernel-General'] and (Level=4 or Level=0) and (EventID={_bootDn_13}) and TimeCreated[@SystemTime&gt;='{bootUpTime.AddMinutes(min).ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{bootUpTime.ToUniversalTime():o}']]]</Select></Query></QueryList>";
    static string qryScrSvr(int upOrDn, DateTime a, DateTime b) => $@"<QueryList><Query Id='0' Path='{_aavLogName}'><Select Path='{_aavLogName}'>*[System[Provider[@Name='{_aavSource}'] and (Level=4 or Level=0) and ( EventID={upOrDn} and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}'] )]]</Select></Query></QueryList>";
    static string qryBootUpsOnly(DateTime a, DateTime b) => $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Kernel-General'] and (Level=4 or Level=0) and (EventID={_bootUp_12}) and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}']]]</Select></Query></QueryList>";
    static string qryBootUpTmChg(DateTime a, DateTime b) => $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Kernel-General'] and (Level=4 or Level=0) and (EventID={_bootUp_12} or EventID={_syTime_01}) and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}']]]</Select></Query></QueryList>";
    static string qryPowerUpsDns(DateTime a, DateTime b) => $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Kernel-General' or @Name='Microsoft-Windows-Kernel-Power'] and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}' and @SystemTime&lt;='{b.ToUniversalTime():o}']]]</Select></Query></QueryList>";
    static string qryAll(string path, DateTime a, DateTime b) => $@"<QueryList><Query Id='0' Path='{path}'><Select Path='{path}'>*[System[TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}' and @SystemTime&lt;='{b.ToUniversalTime():o}']]]</Select></Query></QueryList>";

    static string qryBootAndWakeUps(DateTime a, DateTime b) =>
//Both wake and boot up:           Kernel-General 12 - up   	OR      Power-TroubleShooter 1 
$@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[ (
(Provider[@Name='Microsoft-Windows-Kernel-General'] and (EventID={_bootUp_12} or EventID={_syTime_01})) or 
(Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and EventID={_syTime_01}) )  
and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}'] ]] </Select></Query></QueryList>";//   <QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[ Provider[@Name='Microsoft-Windows-Kernel-General'] and (Level=4 or Level=0) and (EventID={_bootUp}) and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}'] ]]</Select></Query></QueryList>";

    static string qryShutAndSleepDn(DateTime a, DateTime b) =>
//Both sleep and shut down:
$@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[ (
(Provider[@Name='User32'] and EventID=1074) or
(Provider[@Name='Microsoft-Windows-Kernel-Power'] and EventID=42 ) )
and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}'] ]] </Select></Query></QueryList>";//   <QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Kernel-General'] and (Level=4 or Level=0) and (EventID={_bootUp}) and TimeCreated[@SystemTime&gt;='{a.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{b.ToUniversalTime():o}']]]</Select></Query></QueryList>

    /*
 pwr off:
Kernel-Power 42 - entering sleep (on shutdown OR Fn-F1)

Reboot:
Kernel-General 13 - shutting dn
Kernel-Power 109 - right before KG 13 on reboot
Kernel-Power 107 - WRONG - avoid
Kernel-General 12 - up


//Both sleep and shut down:
<QueryList>
  <Query Id='0' Path='System'>
    <Select Path='System'>*[System[Provider[@Name='User32'] and (EventID=1074)]]</Select>
  </Query>
</QueryList>

//Both wake and boot up:           Kernel-General 12 - up   	OR      Power-TroubleShooter 1 
<QueryList>
  <Query Id='0' Path='System'>
    <Select Path='System'>*[System[        
        (Provider[@Name='Microsoft-Windows-Kernel-General'] and EventID={_bootUp_12})
        or 
        (Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and EventID={_syTime_01})
        ]]        
        </Select>
  </Query>
</QueryList>

*/


    static int _ssto = -1; public static int Ssto // ScreenSaveTimeOut 
    {
      get
      {
        if (_ssto < 0)
        {
          try
          {
            const string key = "ScreenSaveTimeOut";
            if (_ssto == -1)
            {
              if (!int.TryParse(Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", key, 299).ToString(), out _ssto) || _ssto == 299)
                if (!int.TryParse(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Control Panel\Desktop", key, 298).ToString(), out _ssto))
                  _ssto = 300;

              ////https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.registrykey.getvalue?view=netframework-4.8
              //var registryKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\Control Panel\Desktop"); // UnauthorizedAccessException: Access to the registry key 'HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Control Panel\Desktop' is denied.
              //Console.WriteLine("Unexpanded: \"{0}\"", registryKey.GetValue(key, "No Value", RegistryValueOptions.DoNotExpandEnvironmentNames));
              //Console.WriteLine("  Expanded: \"{0}\"", registryKey.GetValue(key));
            }
          }
          catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); _ssto = 299; }
        }

        return _ssto;
      }
    }


    /*
     public static DateTime GetDays1rstSsUpTime(DateTime hr00ofTheDate)
     {
       if (hr00ofTheDate == DateTime.Today) return DateTime.Now;

       var hr24ofTheDate = hr00ofTheDate.AddDays(1);
       var rv = hr00ofTheDate;
       var apl1hr = $@"<QueryList><Query Id='0' Path='{_aavLogName}'><Select Path='{_aavLogName}'>*[System[Provider[@Name='{_aavSource}'] and (Level=4 or Level=0) and ( (EventID = {_scrsvrUp}) and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{hr24ofTheDate.ToUniversalTime():o}'] )]]</Select></Query></QueryList>";
       try
       {
         using (var reader = new EventLogReader(new EventLogQuery(_aavLogName, PathType.LogName, apl1hr)))
         {
           for (EventLogRecord er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
             if (rv < er.TimeCreated.Value)
               rv = er.TimeCreated.Value;
         }
       }
       catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

       return rv.AddSeconds(-Sstopgp); // actually - earlier.
     }
     public static DateTime GetDays1rstSleepTime(DateTime hr00ofTheDate)
     {
       if (hr00ofTheDate == DateTime.Today) return DateTime.Now;

       var hr24ofTheDate = hr00ofTheDate.AddDays(1);
       var rv = hr00ofTheDate;
       var enteringSleep = $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[(EventID=42 and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{hr24ofTheDate.ToUniversalTime():o}'] )]]</Select></Query></QueryList>";
       try
       {
         using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, enteringSleep)))
         {
           for (EventLogRecord er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
             if (rv < er.TimeCreated.Value)
               rv = er.TimeCreated.Value;
         }
       }
       catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

       return rv;
     }
     */

    public static DateTime GetDaysLastWakeBoot(DateTime hr00ofTheDate)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr00ofTheDate;
      var qry = qryBootAndWakeUps(hr00ofTheDate, hr24ofTheDate);
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, qry)))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv < er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }
    public static DateTime GetDaysLastSsUpTime(DateTime hr00ofTheDate)
    {
      //sep 2019: if (hr00ofTheDate == DateTime.Today) return DateTime.Now;

      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr00ofTheDate;

      try
      {
        using (var reader = new EventLogReader(new EventLogQuery(_aavLogName, PathType.LogName, qryScrSvr(_ssrUp, hr00ofTheDate, hr24ofTheDate))))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv < er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv.AddSeconds(-Ssto); // actually - earlier.
    }
    public static DateTime GetDaysLastSsDnTime(DateTime hr00ofTheDate)
    {
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr00ofTheDate;
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery(_aavLogName, PathType.LogName, qryScrSvr(_ssrDn, hr00ofTheDate, hr24ofTheDate))))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv < er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }
    public static DateTime GetDaysLastSleepTime(DateTime hr00ofTheDate)
    {
      if (hr00ofTheDate == DateTime.Today) return DateTime.Now;

      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var rv = hr00ofTheDate;
      var enteringSleep = $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[(EventID=42 and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}'] and TimeCreated[@SystemTime&lt;='{hr24ofTheDate.ToUniversalTime():o}'] )]]</Select></Query></QueryList>";
      try
      {
        using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, enteringSleep)))
        {
          for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
            if (rv < er.TimeCreated.Value)
              rv = er.TimeCreated.Value;
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, MethodInfo.GetCurrentMethod().ToString()); }

      return rv;
    }

    public static TimeSpan GetTotalIdlePlusScrsvrUpTimeForTheDate(DateTime hr00ofTheDate)
    {
      var sw = Stopwatch.StartNew();
      DateTime now = DateTime.Now, t1 = DateTime.MinValue, t2 = DateTime.MinValue;
      TimeSpan ttlDnTime = TimeSpan.FromTicks(0), ttlUpTime = TimeSpan.FromTicks(0), ttlIdleTm = TimeSpan.FromTicks(0);
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var apl1hr = $@"<QueryList><Query Id='0' Path='{_aavLogName}'><Select Path='{_aavLogName}'>*[System[Provider[@Name='{_aavSource}'] and (Level=4 or Level=0) and ( (EventID &gt;= {_ssrUp} and EventID &lt;= {_ssrDn}) ) and TimeCreated[@SystemTime&gt;='{hr00ofTheDate.ToUniversalTime():o}']]]</Select></Query></QueryList>";

      var tssec = TimeSpan.FromSeconds(Ssto);

      using (var reader = new EventLogReader(new EventLogQuery(_aavLogName, PathType.LogName, apl1hr)))
      {
        for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
        {
          if (er.TimeCreated.Value > hr24ofTheDate)
          {
            if (t1 > t2) // if last event was up - add this range to uptime
              ttlUpTime += (hr24ofTheDate - t1);
            else
              ttlDnTime += (hr24ofTheDate - t2);

            break;
          }

          if (er.Id == _ssrUp)
          {
            ttlIdleTm += tssec;
            t1 = er.TimeCreated.Value;
            var dt = t2 == DateTime.MinValue ? t1 - hr00ofTheDate : t1 - t2;
            Debug.Write($"\r\n {er.TimeCreated:ddd HH:mm} -           :  dt: {dt,8:h\\:mm\\:ss}  ");
            ttlDnTime += dt;
          }
          else if (er.Id == _ssrDn)
          {
            t2 = er.TimeCreated.Value;
            var dt = t1 == DateTime.MinValue ? t2 - hr00ofTheDate : t2 - t1;
            ttlUpTime += dt;
            Debug.Write($"\r\n           - {er.TimeCreated:ddd HH:mm} :  dt: {dt,8:h\\:mm\\:ss}  ");
          }

          Debug.Write($"   ttl up + ttl dn :   {ttlUpTime,8:h\\:mm\\:ss}  +  {ttlDnTime,8:h\\:mm\\:ss}  =  {(ttlUpTime + ttlDnTime),8:h\\:mm\\:ss}");
        }
      }

      if (now < hr24ofTheDate) // if this is today 
      {
        if (t1 > t2) // if last event was up - add this range to uptime
          ttlUpTime += (now - t1);
        else
          ttlDnTime += (now - t2);
      }

      Debug.Write($"  ==> {ttlUpTime,8:h\\:mm\\:ss}  +  {ttlIdleTm,8:h\\:mm\\:ss}  =  {(ttlUpTime + ttlIdleTm),8:h\\:mm\\:ss}  \t  {sw.ElapsedMilliseconds,5}ms ");

      return ttlUpTime + ttlIdleTm;
    }
    public static TimeSpan GetTotalPowerUpTimeForTheDay(DateTime hr00ofTheDate)
    {
      Debug.WriteLine("");
      var sw = Stopwatch.StartNew();
      DateTime now = DateTime.Now, prevWaken = DateTime.MinValue;
      var ttlwd = TimeSpan.FromTicks(0);
      var hr24ofTheDate = hr00ofTheDate.AddDays(1);
      var sleeps = $@"<QueryList><Query Id='0' Path='System'><Select Path='System'>*[System[Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and TimeCreated[@SystemTime &gt;= '{hr00ofTheDate.ToUniversalTime():o}' ]]]</Select></Query></QueryList>";

      using (var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, sleeps)))
      {
        for (var er = (EventLogRecord)reader.read(); null != er; er = (EventLogRecord)reader.read())
        {
          if (er.Properties[0] == null || !(er.Properties[0].Value is DateTime) || er.Properties[1] == null || !(er.Properties[1].Value is DateTime)) throw new Exception("Not a datetime !!! (AP)");

          var sleepAt = (DateTime)er.Properties[0].Value;
          var wakenAt = (DateTime)er.Properties[1].Value;

          if (prevWaken == DateTime.MinValue && sleepAt > hr00ofTheDate) //ie: worked past midnight
          {
            ttlwd += (sleepAt - GetDays1rstBootUpTime(hr00ofTheDate));
          }

          //if (wakenTime > hr00ofTheDate)
          {
            if (prevWaken != DateTime.MinValue)
              ttlwd += (sleepAt - prevWaken);

            prevWaken = wakenAt;
          }

          //77 Debug.Write($"\n {er.TimeCreated:ddd HH:mm}:    {sleepAt,5:H:mm} ÷ {wakenAt,5:H:mm} = {(wakenAt - sleepAt),5:h\\:mm}    ttl: {ttlwd,5:h\\:mm}");

          if (wakenAt > hr24ofTheDate)
            break;
        }
      }

      if (now < hr24ofTheDate) // if today - then consider now as t2.
        ttlwd += (now - prevWaken);
      else
        Debug.Write($"");

      Debug.Write($"  ==> {hr00ofTheDate:ddd}  {ttlwd,5:h\\:mm} ({ttlwd.TotalHours:N1}) \t  {sw.ElapsedMilliseconds,5}ms ");

      //nogo: requires to be ran as admin: using (var reader = new EventLogReader(new EventLogQuery("Security", PathType.LogName, $@"<QueryList><Query Id='0' Path='Security'><Select Path='Security'>*[System[(EventID=4802 or EventID=4803) and TimeCreated[@SystemTime&gt;='{date.ToUniversalTime():o}']]]</Select></Query></QueryList>"))) //tu: logging screensaver vents:  https://i.stack.imgur.com/WtXOv.png

      return ttlwd;
    }

    #region Old Obsolete but in use

    public static class FuzzyLogic
    {
      static readonly EventLog _eventLog = new EventLog("System");//, "R9-N35FM");

      public static DateTime FirstPowerOnTimeForTheDay(DateTime trg)
      {
        var ermsg = "";

        try
        {
          if (trg == DateTime.MinValue || trg > DateTime.Today) return DateTime.Now;

          var rv = trg.Date.AddDays(1).AddMilliseconds(-1);

          for (var i = _eventLog.Entries.Count - 1; i > 0; i--)
          {
            //ermsg = string.Format("going for {0} out of {1} =elog.Entries.Count", i, elog.Entries.Count);

            //if (elog.Entries[i].Source == "Microsoft-Windows-Power-Troubleshooter" || // The system has resumed from sleep.
            //	 (elog.Entries[i].Source == "EventLog" && elog.Entries[i].InstanceId == 2147489653))//Started
            {
              if (_eventLog.Entries[i].TimeGenerated > trg)
              {
                rv = _eventLog.Entries[i].TimeGenerated;
              }
              else
                break;
            }
          }

          return rv;
        }
        catch (Exception ex) { MessageBox.Show(ermsg + ex, MethodInfo.GetCurrentMethod().ToString()); }

        try { return new DailyBoundaries(trg).DayStart; }
        catch (Exception ex) { MessageBox.Show(ex.ToString(), MethodInfo.GetCurrentMethod().ToString()); }

        return DateTime.Today.AddHours(9);
      }
      public static DateTime LastPowerOffTimeForTheDay(DateTime trg)
      {
        var ermsg = "";

        try
        {
          if (trg == DateTime.MinValue || trg >= DateTime.Today) return DateTime.Now;

          for (var i = _eventLog.Entries.Count - 1; i > 0; i--)
          {
            //	ermsg = string.Format("going for {0} out of {1} =elog.Entries.Count", i, elog.Entries.Count);

            //if (elog.Entries[i].Source == "Microsoft-Windows-Kernel-Power" && elog.Entries[i].InstanceId == 42 ||// The system is entering sleep.
            //	 (elog.Entries[i].Source == "EventLog" && elog.Entries[i].InstanceId == 2147489654)) // Off 
            {
              if (_eventLog.Entries[i].TimeGenerated < trg.AddDays(1))
              {
                return _eventLog.Entries[i].TimeGenerated;
              }
            }
          }

          return trg.Date;
        }
        catch (Exception ex) { MessageBox.Show(ermsg + ex, MethodInfo.GetCurrentMethod().ToString()); }

        try { return new DailyBoundaries(trg).DayFinish; }
        catch (Exception ex) { MessageBox.Show(ex.ToString(), MethodInfo.GetCurrentMethod().ToString()); }

        return DateTime.Today.AddHours(18);
      }
    }

    public static class C2
    {
      public static List<DateTime> GetAllForDay(DateTime trg, string evn)
      {
        var rv0 = new List<DateTime>();
        try
        {
          if (trg == DateTime.MinValue || trg > DateTime.Today) return rv0;

          var rv = trg.Date.AddDays(1).AddMilliseconds(-1);
          var elog = new EventLog(evn);

          for (var i = elog.Entries.Count - 1; i > 0; i--)
          {
            if (elog.Entries[i].TimeGenerated > trg)
              rv0.Add(elog.Entries[i].TimeGenerated);
            else
              break;
          }
        }
        catch (Exception ex) { MessageBox.Show(ex.ToString(), MethodInfo.GetCurrentMethod().ToString()); }

        return rv0;
      }
    }

    public class DailyBoundaries
    {
      public DailyBoundaries(DateTime trg)
      {
        var msToTrg = (long)(DateTime.Now - trg).TotalMilliseconds;

        var queryString =
            "<QueryList>" +
            "  <Query Id=\"0\" Path=\"System\">" +
            "    <Select Path=\"System\">" +
            "        *[System[TimeCreated[timediff(@SystemTime) &gt; " + (msToTrg - 86400000).ToString() + "] and TimeCreated[timediff(@SystemTime) &lt; " + (msToTrg).ToString() + "] ]]" +
            "    </Select>" +
            "  </Query>" +
            "</QueryList>";

        var eventsQuery = new EventLogQuery("System", PathType.LogName, queryString);
        var logReader = new EventLogReader(eventsQuery);

        //			DisplayEventAndLogInformation(logReader);
        populateStartFinish(logReader);
      }

      DateTime start = DateTime.MinValue, finish = DateTime.MinValue;

      public DateTime DayFinish { get => finish; set => finish = value; }
      public DateTime DayStart { get => start; set => start = value; }

      public static List<DateTime?> LastHour
      {
        get
        {
          var l = new List<DateTime?>();
          for (var i = 0; i < 12; i++)
            l.Add(DateTime.Today.AddHours(i));

          return l;
        }
      }
      public static List<EventRecord> GetERList(string logName, string queryString)
      {
        var sw = new Stopwatch();
        sw.Start();

        var logReader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, queryString));
        var l = new List<EventRecord>();
        for (var er = logReader.read(); null != er; er = logReader.read())
          l.Add(er);

        sw.Stop();
        Console.WriteLine("{0,-33} - {1,22} = {2,-22}   (took: {3})", queryString, logName, "", sw.Elapsed);

        return l;
      }
      public static List<DateTime?> GetDTList(string logName, string queryString)
      {
        var sw = new Stopwatch();
        sw.Start();

        var logReader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, queryString));
        var i = 0;
        var l = new List<DateTime?>();
        for (var er = logReader.read(); null != er; er = logReader.read())
          l.Add(er.TimeCreated);

        sw.Stop();
        Console.WriteLine("{0,-33} - {1,22} = {2,-22}   (took: {3})", queryString, logName, i, sw.Elapsed);

        return l;
      }
      public static void Test2(string logName, string queryString)
      {
        var logReader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, queryString));

        var sw = new Stopwatch();
        sw.Start();
        var i = 0;
        for (var er = logReader.read(); null != er; er = logReader.read()) i++;

        sw.Stop();
        Console.WriteLine("{0,-33} - {1,22} = {2,-22}   (took: {3})", queryString, logName, i, sw.Elapsed);
      }
      public static void Test3(DateTime td, string lvl, string logName)
      {
        var d1 = (long)(DateTime.Now - td).TotalMilliseconds;

        var queryString =
            "<QueryList>" +
            "  <Query Id=\"0\" Path=\"" + logName + "\">" +
            "    <Select Path=\"System\">" +
            "        *[System[" + lvl + " TimeCreated[timediff(@SystemTime) &gt; " + (d1 - 86400000).ToString() + "] and TimeCreated[timediff(@SystemTime) &lt; " + (d1).ToString() + "] ]]" +
            "    </Select>" +
            "  </Query>" +
            "</QueryList>";

        var eventsQuery = new EventLogQuery(logName, PathType.LogName, queryString);
        var logReader = new EventLogReader(eventsQuery);

        //			DisplayEventAndLogInformation(logReader);
        //			populateStartFinish(logReader);
        var sw = new Stopwatch();
        sw.Start();
        var i = 0;
        for (var er = logReader.read(); null != er; er = logReader.read()) i++;

        sw.Stop();
        Console.WriteLine("{0,-22} - {1,22} = {2,-22}   (took: {3})", td, logName, i, sw.Elapsed);
      }

      //public static Path GetNewTickPath(string logName, string queryString, double cpp, double rad_In, double radOut, Brush br)
      //{
      //	List<EventRecord> erListSys = GetERList(logName, queryString);
      //	return DailyBoundaries.GetNewTickPath(erListSys, cpp, rad_In, radOut, br);
      //}
      //public static Path GetNewTickPath(List<EventRecord> erLast, double cpp, double rad_In, double radOut, Brush br)
      //{
      //	if (erLast.Count == 0)
      //		return new Path();

      //	Debug.WriteLine("\n");
      //	GeometryGroup geometryGroup = new GeometryGroup();
      //	foreach (EventRecord er in erLast)
      //	{
      ////77 Debug.WriteLine(string.Format("er:> {0}  '{1}'  Id:{2},  Props:{3}", er.TimeCreated, er.ProviderName, er.Id, er.Properties.Count));

      //		double ang = Math.PI * 2.0 * (((DateTime)er.TimeCreated).TimeOfDay.TotalHours - 3);
      //		double angCos = Math.Cos(ang / 12.0);
      //		double angSin = Math.Sin(ang / 12.0);
      //		double x1 = cpp + (er.ProviderName.Contains("Power") ? rad_In * .95 : (rad_In)) * angCos;// *(int)er.Level / 5;
      //		double y1 = cpp + (er.ProviderName.Contains("Power") ? rad_In * .95 : (rad_In)) * angSin;// *(int)er.Level / 5;
      //		double x2 = cpp + (radOut) * angCos;
      //		double y2 = cpp + (radOut) * angSin;
      //		geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(x1, y1), EndPoint = new Point(x2, y2) });

      //		if (er.ProviderName.Contains("Power") || er.ProviderName.Contains("Microsoft-Windows-Kernel-General"))
      //		{
      //			for (int p = 0; p < er.Properties.Count; p++) Debug.WriteLine(string.Format("     {0}  ", er.Properties[p].Value));
      //			double ratio = 0.05;
      //			if (er.ProviderName.Contains("Microsoft-Windows-Power-Troubleshooter")) // start
      //			{
      //				geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(x2 - ratio * rad_In * angSin, y2 + ratio * rad_In * angCos), EndPoint = new Point(x2, y2) });						//geometryGroup.Children.Add(new PathGeometry { StartPoint = new Point(x1, y1), EndPoint = new Point(x2, y2) });
      //			}
      //			else if (er.ProviderName.Contains("Microsoft-Windows-Kernel-Power") && er.Id != 89) // finish
      //			{
      //				geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(x2 + ratio * rad_In * angSin, y2 - ratio * rad_In * angCos), EndPoint = new Point(x2, y2) });
      //			}
      //			else if (er.ProviderName.Contains("Kernel-General") && er.Id == 12) // start
      //			{
      //				geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(x2 - ratio * rad_In * angSin, y2 + ratio * rad_In * angCos), EndPoint = new Point(x2, y2) });						//geometryGroup.Children.Add(new PathGeometry { StartPoint = new Point(x1, y1), EndPoint = new Point(x2, y2) });
      //			}
      //			else
      //		//77 Debug.WriteLine(string.Format("       {0} - not used", er.ProviderName));
      //		}
      //		//geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(h1, v1), EndPoint = new Point(x2, y2) });
      //	}

      //	Path path = new Path
      //	{
      //		Stroke = br,
      //		StrokeThickness = .66,
      //		Fill = Brushes.Yellow, // new SolidColorBrush { Color = Color.FromArgb(255, 104, 104, 255) },
      //		Data = geometryGroup
      //	};

      //	return path;
      //}

      void populateStartFinish(EventLogReader logReader)
      {
        var sw = new Stopwatch();
        sw.Start();
        start = DateTime.MinValue;
        for (var er = logReader.read(); null != er; er = logReader.read())
        {
          if (start == DateTime.MinValue && er.TimeCreated != null)
            start = (DateTime)er.TimeCreated;

          if (er.TimeCreated != null)
            finish = (DateTime)er.TimeCreated;
        }

        sw.Stop();
        Console.WriteLine("{0} - {1} = {2,-22}   (took: {3})", start, finish, finish - start, sw.Elapsed);
      }
    }

    #endregion

    public static async Task<int> UpdateEvLogToDb(int daysback, string msg) //todo: should not it be in the Db.EventLog project? (Jun2019)
    {
      try
      {//Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.Started):mm\\:ss\\.ff}    UpdateEvLogToDb(): {msg}");

        if (!/*VerHelper.*/IsVIP) return -1; // let go ctrl-alt-del

        var dailyEvents = AsLink.EvLogHelper.GetAllUpDnEvents(DateTime.Today.AddDays(-daysback), DateTime.Now);
        return dailyEvents.Count > 0 ? await Db.EventLog.DbLogHelper.UpdateDbWithPotentiallyNewEvents(dailyEvents, Environment.MachineName, msg) : -2;
      }
      catch (Exception ex) { ex.Log(); }
      return -888;
    }

    public static bool IsVIP
    {
      get
      {
        var un = Environment.UserName.ToLower();
        return un.Contains("zzz") || un.Contains("pigid") || un.Contains("lex");
      }
    }
  }
}