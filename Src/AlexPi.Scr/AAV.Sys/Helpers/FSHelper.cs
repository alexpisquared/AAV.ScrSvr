using AAV.Sys.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AAV.Sys.Helpers
{
  public static class FSHelper
  {
    public static bool ExistsOrCreated(string folder) // true if created or exists; false if unable to create.
    {
      try
      {
        if (!Directory.Exists(folder))
          Directory.CreateDirectory(folder);

        return true;
      }
      catch (IOException ex) { ex.Log($"Directory.CreateDirectory({folder})"); }
      catch (Exception ex) { ex.Log($"Directory.CreateDirectory({folder})"); throw; }
      
      return false;
    }
  }
}
