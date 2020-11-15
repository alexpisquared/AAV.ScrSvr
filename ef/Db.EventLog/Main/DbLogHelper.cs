using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using Db.EventLog.DbModel;
using Db.EventLog.Main;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Db.EventLog
{
  public class DbLogHelper
  {
    public const string _dbSubP = @"Public\AppData\EventLogDb\";
    static readonly string _dbPath = OneDrive.Folder(_dbSubP);
    static List<PcLogic> _PcLogics;
    static readonly Dictionary<(DateTime a, DateTime b, string pcname), SortedList<DateTime, int>> _dict = new Dictionary<(DateTime a, DateTime b, string pcname), SortedList<DateTime, int>>();

    public static List<PcLogic> AllPCsSynch()
    {
      if (_PcLogics != null) return _PcLogics;

      _PcLogics = new List<PcLogic>();
      //,,Trace.WriteLine($"-->AllPCs():\t");

      foreach (var dbpf in Directory.GetFiles(_dbPath, @"LocalDb(*).mdf", SearchOption.TopDirectoryOnly))
      {
        //,,
        Trace.Write($"--> {Path.GetFileNameWithoutExtension(dbpf),-22}:\t");
        FileAttributeHelper.AddAttribute(dbpf, FileAttributes.ReadOnly);
        try
        {
          using (var db = A0DbModel.GetLclFl(dbpf))
          {
            //,,
            Trace.WriteLine($"{db.PcLogics.Count()} rows.");
            _PcLogics.AddRange(db.PcLogics.Where(r => dbpf.ToUpper().Contains(r.MachineName.ToUpper())).ToList());
          }
        }
        catch (Exception ex) { ex.Log(Path.GetFileNameWithoutExtension(dbpf)); }
      }

      //,,Trace.WriteLine("");
      return _PcLogics;
    }
    public static async Task<List<PcLogic>> AllPCsAsync()
    {
      if (_PcLogics != null) return _PcLogics;

      _PcLogics = new List<PcLogic>();
      //,,Trace.WriteLine($"-->AllPCs():\t");

      var mdfs = Directory.GetFiles(_dbPath, @"LocalDb(*).mdf", SearchOption.TopDirectoryOnly);
      Trace.WriteLine($"■■■\n{string.Join("\n", mdfs)}\n■■■");
      foreach (var dbpf in mdfs)
      {
        await Task.Delay(300);
        //,,Trace.Write($"--> {Path.GetFileNameWithoutExtension(dbpf),-22}:\t");
        FileAttributeHelper.AddAttribute(dbpf, FileAttributes.ReadOnly);
        try
        {
          using (var db = A0DbModel.GetLclFl(dbpf))
          {
            await db.PcLogics.LoadAsync();
            //,,Trace.WriteLine($"{db.PcLogics.Local.Count()} rows.");
            _PcLogics.AddRange(db.PcLogics.Local.Where(r => dbpf.ToUpper().Contains(r.MachineName.ToUpper())).ToList());
          }
        }
        catch (Exception ex) { ex.Log(Path.GetFileNameWithoutExtension(dbpf)); }
        //finally { FileAttributeHelper.AddAttribute(dbpf, FileAttributes.ReadOnly); }
      }

      //,,Trace.WriteLine("");
      return _PcLogics;
    }
    public static SortedList<DateTime, int> GetAllUpDnEvents(DateTime a, DateTime b, string pcname)
    {
      if (_dict.ContainsKey((a, b, pcname))) return _dict[(a, b, pcname)];

      var rv = new SortedList<DateTime, int>();

      using (var db = A0DbModel.GetLclFl(OneDrive.Folder($@"{_dbSubP}LocalDb({pcname}).mdf")))
      {
        foreach (var eoi in db.EvOfInts.Where(r => a < r.TimeID && r.TimeID < b && r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)))
        {
          rv.Add(eoi.TimeID, eoi.EvOfIntFlag);
        }

        var lastRecTime = db.PcLogics.FirstOrDefault(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase))?.LogReadAt;
        if (lastRecTime != null && a < lastRecTime && lastRecTime < b)
          rv.Add(lastRecTime.Value, (int)EvOfIntFlag.ShutAndSleepDn);
      }

      _dict.Add((a, b, pcname), rv);

      return rv;
    }
    public static async Task<int> UpdateDbWithPotentiallyNewEvents(SortedList<DateTime, int> evlst, string pcname, string note)
    {
      var localdb = OneDrive.Folder($@"{_dbSubP}LocalDb({pcname}).mdf");
      try
      {
        FileAttributeHelper.RmvAttribute(localdb, FileAttributes.ReadOnly);

        using (var db = A0DbModel.GetLclFl(localdb))
        {
          if (FindNewEventsToSaveToDb(evlst, pcname, note, db) > 0)
            return (await db.TrySaveReportAsync()).rowsSavedCnt;
        }
      }
      catch (Exception ex) { ex.Log(); }
      finally { FileAttributeHelper.AddAttribute(localdb, FileAttributes.ReadOnly); }

      return -4;
    }

    public static int FindNewEventsToSaveToDb(SortedList<DateTime, int> evlst, string pcname, string note, A0DbModel db)
    {
      var addedCount = 0;
      try
      {
        var now = DateTime.Now;

        var pcLogic = (db.PcLogics.Any(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase))) ?
                    db.PcLogics.First(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)) :
                    db.PcLogics.Add(new PcLogic { MachineName = pcname, ColorRGB = "#888", DailyMaxHr = 8.5, CreatedAt = now });

        pcLogic.LogReadAt = now;
        pcLogic.Note = note;


        var timeRoundedList = new SortedList<DateTime, int>();
        foreach (var ev in evlst)
        {
          var rounded = new DateTime(ev.Key.Year, ev.Key.Month, ev.Key.Day, ev.Key.Hour, ev.Key.Minute, ev.Key.Second);

          if (!timeRoundedList.Any(r => r.Key == rounded))
          {
            timeRoundedList.Add(rounded, ev.Value);
          }
        }

        foreach (var timeRoundedEv in timeRoundedList)
        {
          if (!db.EvOfInts.Any(r => r.TimeID == timeRoundedEv.Key && r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)))
          {
            db.EvOfInts.Add(new DbModel.EvOfInt { TimeID = timeRoundedEv.Key, EvOfIntFlag = timeRoundedEv.Value, MachineName = pcname });
            addedCount++;
          }
        }
      }
      catch (Exception ex) { ex.Log(); }

      return addedCount;
    }
  }
}