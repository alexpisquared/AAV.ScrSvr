
using System;
using System.IO;

namespace AAV.Sys.Helpers
{
  public static class OneDrive // Core 3
  {
    public static string Root
    {
      get
      {
        var rv = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive"); 
        return Environment.MachineName.Equals("ASUS2") ? rv.Replace("C:", "D:") : rv; // :'cause Asus2 is on D:
      }
    }

    public static string Folder(string subFolder) => Path.Combine(Root, subFolder);
    public static string VpdbFolder
    {
      get
      {
        var vpDbFolder = Path.Combine(OneDrive.Root, @"Public\AppData\vpdb\");
        FSHelper.ExistsOrCreated(vpDbFolder);
        return vpDbFolder;
      }
    }
  }
}