using StandardLib.Extensions;
using Db.EventLog.DbModel;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Db.EventLog.Ext
{
  public class DbInitializer : DropCreateDatabaseIfModelChanges<A0DbModel>
  {
    public bool IsDbReady { get; set; }
    public static void SetMigraInitializer() => Database.SetInitializer(new DbMigrater());
    public static void SetDbInitializer()
    {
      try
      {
        Database.SetInitializer(new CreateDatabaseIfNotExists<A0DbModel>());
        Database.SetInitializer(new DropCreateDatabaseIfModelChanges<A0DbModel>());
        Database.SetInitializer(new DbInitializer());
      }
      catch (Exception ex) { ex.Log(); throw; }
    }

    protected override void Seed(A0DbModel db)
    {
      base.Seed(db);
      var now = DateTime.Now;

      try
      {
        db.PcLogics.Add(new PcLogic { MachineName = Environment.MachineName, ColorRGB = "#888", DailyMaxHr = 8.5, Note = "Db Ini-d" });
        db.EvOfInts.Add(new DbModel.EvOfInt { EvOfIntFlag = 1024, MachineName = Environment.MachineName, TimeID = DateTime.Now });
        Task.Run(async () => await db.TrySaveReportAsync());
      }
      catch (Exception ex) { ex.Log(); throw; }
    }
  }
}