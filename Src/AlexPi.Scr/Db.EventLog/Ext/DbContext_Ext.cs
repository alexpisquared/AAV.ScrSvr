using AAV.Sys.Ext;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Db.EventLog.Ext
{
  public static class DbContext_Ext // replacing DbSaveLib and all others!!! (Aug 2018)
  {
    [Obsolete("AAV-> Use Async counterpart instead!")]
    public static /*       */(bool success, int rowsSavedCnt, string report) TrySaveReport(this DbContext db, string info = "", [CallerMemberName] string callerName = "")
    {
      var success = false;
      var rowsSaved = -1;
      var report = "";

      try
      {
        var sw = Stopwatch.StartNew();
        lock (_thisLock) { rowsSaved = db.SaveChanges(); }

        report =
            rowsSaved == 0 ? $"No DB changes to save. {info}" :
            rowsSaved > 0 && sw.Elapsed.TotalSeconds < .01 ?
                $"{rowsSaved,7:N0} records saved. {info}" :
                $"row/sec: {rowsSaved,8:N0} / {sw.Elapsed.TotalSeconds,-5:N1}= {(rowsSaved / sw.Elapsed.TotalSeconds):N0}. {info}";

        success = true;
      }
      catch (DbEntityValidationException ex) { var msg = ValidationExceptionToString(ex); /**/	report = ex.Log(msg); }
      catch (DbUpdateException ex) {    /**/   var msg = DbUpdateExceptionToString(ex);   /**/	report = ex.Log(msg); }
      catch (Exception ex) {                                                              /**/  report = ex.Log(); }
      finally { Trace.TraceInformation($"::>{callerName}.TrySaveReportAsync({info}): {report}"); }

      return (success, rowsSaved, report);
    }
    public static async Task<(bool success, int rowsSavedCnt, string report)> TrySaveReportAsync(this DbContext db, string info = "", [CallerMemberName] string callerName = "")
    {
      var success = false;
      var rowsSaved = -1;
      var report = "";

      try
      {
        var sw = Stopwatch.StartNew();
        /*lock (_thisLock)*/
        { rowsSaved = await db.SaveChangesAsync(); }

        report =
            rowsSaved == 0 ? $"No DB changes to save. {info}" :
            rowsSaved > 0 && sw.Elapsed.TotalSeconds < .01 ?
                $"{rowsSaved,7:N0} records saved. {info}" :
                $"row/sec: {rowsSaved,8:N0} / {sw.Elapsed.TotalSeconds,-5:N1}= {(rowsSaved / sw.Elapsed.TotalSeconds):N0}. {info}";

        success = true;
      }
      catch (DbEntityValidationException ex) { var msg = ValidationExceptionToString(ex); /**/	report = ex.Log(msg); }
      catch (DbUpdateException ex) {    /**/   var msg = DbUpdateExceptionToString(ex);   /**/	report = ex.Log(msg); }
      catch (Exception ex) {                                                              /**/  report = ex.Log(); }
      finally { Trace.TraceInformation($"::>{callerName}.TrySaveReportAsync({info}): {report}"); }

      return (success, rowsSaved, report);
    }
    public static string SaveLogReportEta(this DbContext db, Stopwatch sw, int cur = 0, int max = 0, bool skipTrace = false)
    {
#if DEBUG
      db.GetDbChangesReport(1000000);
#endif

      var dbSaveReport = db.TrySaveReportAsync(cur < max ? "ETA:" : "end.");
      var rv = (max == 0 || cur == 0) ?
          $"{dbSaveReport,120}"
          : $"{sw.Elapsed:h\\:mm\\:ss} =>{1000d * cur / (sw.ElapsedMilliseconds + .000001),8:N1}{dbSaveReport}{string.Format($"{sw.Elapsed.TotalMinutes * (max - cur) / cur,7:N1} >> {DateTime.Now.AddMinutes(sw.Elapsed.TotalMinutes * (max - cur) / cur):dddHH:mm} ", sw.Elapsed.TotalMinutes * (max - cur) / cur, DateTime.Now.AddMinutes(sw.Elapsed.TotalMinutes * (max - cur) / cur))}";

      if (!skipTrace)
        Trace.TraceInformation(rv);

      //Bpr.BeepOk();
      return rv;
    }


    public static bool HasUnsavedChanges(this DbContext db) => db == null ? false : db.ChangeTracker.Entries().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
    public static string GetDbChangesReport(this DbContext db, int maxRowsToShow = 33)
    {
      var sw = Stopwatch.StartNew();

      var add = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Count();
      var mod = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Count();
      var del = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Count();

      var lineCur = 0;       //if (Debugger.IsAttached) Debugger.Break();
      var prompt = $" {del,5} - Deletes\n {add,5} - Inserts\n {mod,5} - Updates";

      foreach (var item in db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
      {
        foreach (var pn in item.CurrentValues.PropertyNames)
        {
          if (!Object.Equals(item.CurrentValues[pn], item.OriginalValues[pn]))
          {
            if (++lineCur > maxRowsToShow)
            {
              prompt += " ...";
              break;
            }
            else
              prompt += $"\n{pn,-17}\t{safeValue(item.OriginalValues[pn])} != {safeValue(item.CurrentValues[pn])}";
          }
        }
        if (lineCur > maxRowsToShow) break;
      }

      Trace.WriteLineIf(ExnLogr.AppTraceLevelCfg.TraceVerbose, $">>> DbChangesReport  {sw.ElapsedMilliseconds,6:N0} ms");
      return prompt;
    }

    public static string DbUpdateExceptionToString(this DbUpdateException ex)
    {
      var sb = new StringBuilder();

      foreach (var er in ex.Entries)
      {
        sb.Append($"\r\n:>DbUpdateException:  {ex.InnerMessages()}\t");
      }

      return sb.ToString();
    }
    public static string ValidationExceptionToString(this DbEntityValidationException ex)
    {
      var sb = new StringBuilder();

      foreach (var eve in ex.EntityValidationErrors)
      {
        sb.AppendLine($"- Entity of type \"{eve.Entry.Entity.GetType().FullName}\" in state \"{eve.Entry.State}\" has the following validation errors:");
        foreach (var ve in eve.ValidationErrors)
        {
          object value;
          if (ve.PropertyName.Contains("."))
          {
            var propertyChain = ve.PropertyName.Split('.');
            var complexProperty = eve.Entry.CurrentValues.GetValue<DbPropertyValues>(propertyChain.First());
            value = GetComplexPropertyValue(complexProperty, propertyChain.Skip(1).ToArray());
          }
          else
          {
            value = eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName);
          }

          sb.AppendLine($"-- Property: \"{ve.PropertyName}\", Value: \"{value}\", Error: \"{ve.ErrorMessage}\"");
        }
      }

      const int maxCombinedErrorMessageLength = 4000;
      return sb.Length < maxCombinedErrorMessageLength ? sb.ToString() : (sb.ToString().Substring(0, maxCombinedErrorMessageLength) + " ...");
    }
    public static string ServerDatabase(this DbContext db)
    {
      var kvpList = db.Database.Connection.ConnectionString.Split(';').ToList();
      var ds = getConStrValue(kvpList, "data source") + "\\";
      return $"{(ds.Equals(@"(localdb)\MSSQLLocalDB") ? "" : ds.Contains("database.windows.net") ? "Azure\\" : ds)}{getConStrValue(kvpList, "AttachDbFilename")}{getConStrValue(kvpList, "initial catalog")}";
    }


    static string getConStrValue(System.Collections.Generic.List<string> lst, string ss) => lst.FirstOrDefault(r => r.Split('=')[0].Equals(ss, StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
    static object GetComplexPropertyValue(DbPropertyValues propertyValues, string[] propertyChain)
    {
      var propertyName = propertyChain.First();

      return propertyChain.Length == 1
          ? propertyValues[propertyName]
          : GetComplexPropertyValue((DbPropertyValues)propertyValues[propertyName], propertyChain.Skip(1).ToArray());
    }
    static string safeValue(object v)
    {
      if (v == null) return "null";

      const int max = 42;
      return v is string
        ? ((string)v).Length <= max ? ((string)v) :
            $"\r\n  {((string)v).Substring(0, max).Replace("\n", " ").Replace("\r", " ")}...{((string)v).Length:N0}\r\n"
        : v.ToString();
    }
    static readonly object _thisLock = new object();
  }
}
