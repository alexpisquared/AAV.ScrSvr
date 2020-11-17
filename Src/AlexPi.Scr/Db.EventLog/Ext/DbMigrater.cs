using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Db.EventLog.DbModel;
using System.Diagnostics;
using System.Threading.Tasks;
using AAV.Sys.Ext;

namespace Db.EventLog.Ext
{
  public class DbMigrater : MigrateDatabaseToLatestVersion<A0DbModel, DbMigrationsConfiguration>
  {
    public override void InitializeDatabase(A0DbModel db)
    {
      var cs = (string)((dynamic)db.Database.Connection).ConnectionString;
      try
      {
        Trace.WriteLine($"DbMigrater.InitDb() - {cs}");
        var migrator = new DbMigrator(new DbMigrationsConfiguration());
        migrator.Update();

        var now = DateTime.Now;

        db.PcLogics.Load();
        foreach (var ur in db.PcLogics.Local) ur.Info = now.ToString(); ;
        
        Task.Run(async () => await db.TrySaveReportAsync());
      }
      catch (Exception ex) { ex.Log(cs); }
    }
  }

  public class DbMigrationsConfiguration : DbMigrationsConfiguration<A0DbModel>
  {
    public DbMigrationsConfiguration()
    {
      AutomaticMigrationsEnabled = true;
      AutomaticMigrationDataLossAllowed = true;
    }
    protected override void Seed(A0DbModel db)
    {
      var cs = (string)((dynamic)db.Database.Connection).ConnectionString;
      try
      {

        Trace.WriteLine($"DbMigrationsConfiguration.Seed() - {cs}");
        /// called upon a migration
        /// add additional seeding if needed... 
        /// ...although MigrateDatabaseToLatestVersion.InitializeDatabase seems OK too.
        /// db.TrySaveReport();
      }
      catch (Exception ex) { ex.Log(cs); }
    }
  }
}
