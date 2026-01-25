namespace Db.EventLog.Main;

[Obsolete("Phasing out LOCAL db ...", false)]
public class DbLogHelper // ..actually, I think this one is more obsolete C:\g\TimeTracking\N50\TimeTracking50\Db.EventLog\Main\DbLogHelper.cs  Jan-2026
{
  public const string _dbSubP = @"Public\AppData\EventLogDb\";
  static readonly string _dbPath = OneDrive.Folder(_dbSubP);
  static List<PcLogic>? _pcLogics;
  static readonly Dictionary<(DateTime a, DateTime b, string pcname), SortedList<DateTime, EventOfInterestFlag>> _dict = [];

  public static List<PcLogic> AllPCsSynch()
  {
    if (_pcLogics != null) return _pcLogics;

    _pcLogics = [];
    //,,Trace.WriteLine($"-->AllPCs():\t");

    foreach (var dbpf in Directory.GetFiles(_dbPath, @"LocalDb(*).mdf", SearchOption.TopDirectoryOnly))
    {
      //,,
      Trace.Write($"--> {Path.GetFileNameWithoutExtension(dbpf),-22}:\t");
      FileAttributeHelper.AddAttribute(dbpf, FileAttributes.ReadOnly);
      try
      {
        using var db = A0DbModel.GetLclFl(dbpf);
        //,,
        Trace.WriteLine($"{db.PcLogics.Count()} rows.");
        _pcLogics.AddRange(db.PcLogics.Where(r => dbpf.ToUpper().Contains(r.MachineName.ToUpper())).ToList());
      }
      catch (Exception ex) { _ = ex.Log(Path.GetFileNameWithoutExtension(dbpf)); }
    }

    //,,Trace.WriteLine("");
    return _pcLogics;
  }
  public static async Task<List<PcLogic>> AllPCsAsync()
  {
    if (_pcLogics != null) return _pcLogics;

    _pcLogics = [];
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
        using var db = A0DbModel.GetLclFl(dbpf);
        await db.PcLogics.LoadAsync();
        //,,Trace.WriteLine($"{db.PcLogics.Local.Count()} rows.");
        _pcLogics.AddRange(db.PcLogics.Local.Where(r => dbpf.ToUpper().Contains(r.MachineName.ToUpper())).ToList());
      }
      catch (Exception ex) { _ = ex.Log(Path.GetFileNameWithoutExtension(dbpf)); }
      //finally { FileAttributeHelper.AddAttribute(dbpf, FileAttributes.ReadOnly); }
    }

    //,,Trace.WriteLine("");
    return _pcLogics;
  }
  public static SortedList<DateTime, EventOfInterestFlag> GetAllUpDnEvents(DateTime a, DateTime b, string pcname)
  {
    if (_dict.ContainsKey((a, b, pcname))) return _dict[(a, b, pcname)];

    var rv = new SortedList<DateTime, EventOfInterestFlag>();

    using (var db = A0DbModel.GetLclFl(OneDrive.Folder($@"{_dbSubP}LocalDb({pcname}).mdf")))
    {
      foreach (var eoi in db.EvOfInts.Where(r => a < r.TimeID && r.TimeID < b && r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)))
        rv.Add(eoi.TimeID, (EventOfInterestFlag)eoi.EvOfIntFlag);

      var lastRecTime = db.PcLogics.FirstOrDefault(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase))?.LogReadAt;
      if (lastRecTime != null && a < lastRecTime && lastRecTime < b)
        rv.Add(lastRecTime.Value, EventOfInterestFlag.___Pwr);
    }

    _dict.Add((a, b, pcname), rv);

    return rv;
  }
  public static async Task<int> UpdateDbWithPotentiallyNewEvents(SortedList<DateTime, EventOfInterestFlag> evlst, string pcname, string note)
  {
    var localdb = OneDrive.Folder($@"{_dbSubP}LocalDb({pcname}).mdf");
    try
    {
      FileAttributeHelper.RmvAttribute(localdb, FileAttributes.ReadOnly);

      using var db = A0DbModel.GetLclFl(localdb);
      if (FindNewEventsToSaveToDb(evlst, pcname, note, db) > 0)
        return (await db.TrySaveReportAsync()).rowsSavedCnt;
    }
    catch (Exception ex) { _ = ex.Log(); }
    finally { FileAttributeHelper.AddAttribute(localdb, FileAttributes.ReadOnly); }

    return -4;
  }

  public static int FindNewEventsToSaveToDb(SortedList<DateTime, EventOfInterestFlag> evlst, string pcname, string note, A0DbModel db)
  {
    var addedCount = 0;
    try
    {
      var now = DateTime.Now;

      var pcLogic = db.PcLogics.Any(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)) ?
                  db.PcLogics.First(r => r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)) :
                  db.PcLogics.Add(new PcLogic { MachineName = pcname, ColorRGB = "#888", DailyMaxHr = 8.5, CreatedAt = now });

      pcLogic.LogReadAt = now;
      pcLogic.Note = note;

      var timeRoundedList = new SortedList<DateTime, EventOfInterestFlag>();
      foreach (var ev in evlst)
      {
        var rounded = new DateTime(ev.Key.Year, ev.Key.Month, ev.Key.Day, ev.Key.Hour, ev.Key.Minute, ev.Key.Second);

        if (!timeRoundedList.Any(r => r.Key == rounded))
          timeRoundedList.Add(rounded, ev.Value);
      }

      foreach (var timeRoundedEv in timeRoundedList)
        if (!db.EvOfInts.Any(r => r.TimeID == timeRoundedEv.Key && r.MachineName.Equals(pcname, StringComparison.OrdinalIgnoreCase)))
        {
          _ = db.EvOfInts.Add(new EvOfInt { TimeID = timeRoundedEv.Key, EvOfIntFlag = (int)timeRoundedEv.Value, MachineName = pcname });
          addedCount++;
        }
    }
    catch (Exception ex) { _ = ex.Log(); }

    return addedCount;
  }

  public static SortedList<DateTime, int> GetAllEventsOfInterest(DateTime a, DateTime b, string pcname) // binary compat with C:\g\TimeTracking\N50\TimeTracking50\Db.EventLog\Main\DbLogHelper.cs   Jan-2026
  {
    var dict = GetAllUpDnEvents(a, b, pcname)
        .ToDictionary(kv => kv.Key, kv => (int)kv.Value);

    var sortedList = new SortedList<DateTime, int>(dict);
    return sortedList;
  }
}