using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AAV.Sys.Helpers
{
  public static class SqlConStrHelper
  {
    public enum DbLocation { Azure, Local, DbIns };

    public static string? DbNameFind(string conStr) => conStr?.Split(';').ToList().FirstOrDefault(r => r.Split('=')[0].Equals("initial catalog"
      , StringComparison.OrdinalIgnoreCase
      ) || r.Split('=')[0].Equals("AttachDbFilename"
        , StringComparison.OrdinalIgnoreCase
        ))?.Split('=')[1];

    public static string ConStr(string dbName, string dbgRLs, DbLocation dbLocation, string username, string password)
    {
      switch (dbLocation)
      {
        case DbLocation.Azure:
          return $"data source=sqs.database.windows.net;initial catalog={dbName}{dbgRLs};persist security info=True;user id={username};password=\"{password}\";MultipleActiveResultSets=True;App=EntityFramework";
        case DbLocation.Local:
          var dbpathfilename = OneDrive.Folder($@"Public\AppData\TimeTrack\{dbName}{dbgRLs}.mdf");
          Debug.Assert(File.Exists(dbpathfilename));
          return $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbpathfilename};Integrated Security=True;Connect Timeout=10;";
        case DbLocation.DbIns:
          return $@"data source=.\sqlexpress;initial catalog={dbName}{dbgRLs}; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        default:
          throw new ArgumentOutOfRangeException(dbLocation.GetType().Name);
      }
    }
  }
}