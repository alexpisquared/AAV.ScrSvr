using Db.EventLog.Ext;
using Db.EventLog.DbModel;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace Db.EventLog.Main
{
  public class DataTransfer
  {
    readonly DateTime _now = DateTime.Now;
    public (int newRows, string report, Stopwatch swstopwatch) CopyChunkyAzureSync(A0DbModel src, A0DbModel trg)
    {
      var rowsAdded = 0;
      var sw = Stopwatch.StartNew();

      var srcSvrDb = src.ServerDatabase();
      var trgSvrDb = trg.ServerDatabase();

      foreach (var u in src.PcLogics)
      {
        if (!trg.PcLogics.Any(r => u.MachineName.Equals(r.MachineName, StringComparison.OrdinalIgnoreCase)))
        {
          trg.PcLogics.Add(new PcLogic { MachineName = u.MachineName, ColorRGB = u.ColorRGB, CreatedAt = u.CreatedAt ?? _now, Note = u.Note ?? $"data transfer from  '{srcSvrDb}'.", Info = u.Info, LogReadAt = u.LogReadAt });
          rowsAdded++;
        }
      }

      trg.EvOfInts.Load(); // chunky vs chattee

      var thisPcEvents = src.EvOfInts.Where(r => Environment.MachineName.Equals(r.MachineName, StringComparison.OrdinalIgnoreCase));

      foreach (var s in thisPcEvents)
      {
        if (trg.EvOfInts.Local.Any(r => s.TimeID == r.TimeID && s.MachineName.Equals(r.MachineName, StringComparison.OrdinalIgnoreCase)))
          Debug.WriteLine($"  **> skipping match:  {s.TimeID}   {s.MachineName} ");
        else
        {
          trg.EvOfInts.Local.Add(new EvOfInt
          {
            MachineName = s.MachineName,
            TimeID = s.TimeID,
            EvOfIntFlag = s.EvOfIntFlag
          });
          rowsAdded++;
        }
      }

      var rowsSaved = trg.SaveChanges();

      Debug.Assert(rowsAdded == rowsSaved, "ap: rowsChanged != rowsSaved");

      return (rowsSaved, $"Success: {thisPcEvents.Count()} this PC rows found, {rowsAdded} copied from \n {srcSvrDb} to \n {trgSvrDb} in {sw.Elapsed:mm\\:ss} => {trg.EvOfInts.Count()} rows now.", sw);
    }
  }
}
