using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;

namespace Db.EventLog.Main
{
  public static class FileAttributeHelper
  {
    [Obsolete("AAV-> Nust be done before conn opened!!!", true)]
    public static void UnReadOnly(DbContext dbc, out string mdfDataFile, out FileAttributes fa)
    {
      mdfDataFile = GetLocalDbFilePath(dbc);
      fa = File.GetAttributes(mdfDataFile);
      if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
      {
        File.SetAttributes(mdfDataFile, (fa = RmvAttribute(fa, FileAttributes.ReadOnly)));
      }

      Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} ::>>{mdfDataFile}: {File.GetAttributes(mdfDataFile)}");
    }

    public static string GetLocalDbFilePath(DbContext dbc)
    {
      string mdfDataFile;
      var cs = (string)((dynamic)dbc.Database.Connection).ConnectionString;
      mdfDataFile = cs.Split(';')[1].Split('=')[1];
      return mdfDataFile;
    }

    public static void AddAttribute(string file, FileAttributes attributeToAdd) => File.SetAttributes(file, AddAttribute(File.GetAttributes(file), attributeToAdd));
    public static void RmvAttribute(string file, FileAttributes attributeToRmv)
    {
      var fa = File.GetAttributes(file);
      if ((fa & attributeToRmv) == attributeToRmv)
      {
        File.SetAttributes(file, RmvAttribute(fa, attributeToRmv));
      }
    }
    public static FileAttributes RmvAttribute(FileAttributes attributes, FileAttributes attributesToRmv) => attributes & ~attributesToRmv;
    public static FileAttributes AddAttribute(FileAttributes attributes, FileAttributes attributesToAdd) => attributes | attributesToAdd;
  }
}
