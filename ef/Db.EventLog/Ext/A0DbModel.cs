using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using Db.EventLog.Ext;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;

namespace Db.EventLog.DbModel
{
  public partial class A0DbModel : DbContext
  {
#if !ready
    public static readonly DateTime Started = DateTime.Now;
    const string _dbName = "EventLogDb";
    public static A0DbModel GetExprs => new A0DbModel(0, $@"data source=.\sqlexpress;initial catalog={_dbName};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework");
    public static A0DbModel GetLclFl(string localdbpathfile) { var db = new A0DbModel(localdbpathfile); checkCreateMigrate(db, localdbpathfile); return db; }
    //blic static A0DbModel GetAzure => new A0DbModel(0, $"data source=sqs.database.windows.net;initial catalog={_dbName};persist security info=True;user id=azuresqluser;password=\";lkj;lkj99\";MultipleActiveResultSets=True;App=EntityFramework");
    //blic static A0DbModel GetConfg => new A0DbModel(@"name=A0DbModel_element_in_config_file");
#endif

    A0DbModel(int nothing, string constr) : base(constr) { }

    A0DbModel(string localdbpathfile)
#if LocalDB
          : base("EventLogDb") {     // in the main sql instance 
#else
    {
      var dbfn = localdbpathfile ?? OneDrive.Folder($@"{DbLogHelper._dbSubP}LocalDb({Environment.MachineName}).mdf");

      var dir = Path.GetDirectoryName(dbfn);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      Database.Connection.ConnectionString = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbfn};Integrated Security=True;Connect Timeout=17;";
#endif
      //Trace.WriteLine($"::>{(localdbpathfile ?? "null")} -> {Path.GetFileNameWithoutExtension(dbfn)}");
    }


    static void checkCreateMigrate(A0DbModel db, string localdbpathfile)
    {
      try
      {
        Trace.Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    {db.ServerDatabase()} -");
        if (!File.Exists(localdbpathfile)) // if (db.Database.Exists())
        {
          Trace.WriteLine($" does NOT exist => creating on the first call.");
          DbInitializer.SetDbInitializer();
        }
        else
        {
          if (db.Database.CompatibleWithModel(false)) // started failing here in Jan2018 without much of a reason.
            Trace.WriteLine($" exists and is compatible => all set.");
          else
          {
            Trace.WriteLine($" exists BUT incompatible => migrating soon.");
            DbInitializer.SetMigraInitializer();
          }
        }
      }
      catch (Exception ex) { ex.Log(); }
    }
  }
}