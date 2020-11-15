//using AAV.Sys.Ext;
//using AsLink;
//using System;
//using System.IO;
//using System.IO.IsolatedStorage;
//using System.Reflection;

//namespace StoryTestRunnerV2.Helpers
//{
//  [Obsolete(@"Use 'C:\c\AsLink\UniSerializer.cs' instead!!!")]
//  public static class IsoHelper
//  {
//    [Obsolete(@"Use 'C:\c\AsLink\UniSerializer.cs' instead!!!")]
//    public static void SaveIsoFile(string filename, string json)
//    {
//      try
//      {
//        using (var strm = new IsolatedStorageFileStream(filename, FileMode.Create, IsoStore.GetIsolatedStorageFile()))
//        {
//          using (var streamWriter = new StreamWriter(strm))
//          {
//            streamWriter.Write(json);
//            streamWriter.Close ( );
//          }
//        }
//      }
//      catch (Exception ex) { ex.Log(); }
//    }

//    [Obsolete(@"Use 'C:\c\AsLink\UniSerializer.cs' instead!!!")]
//    public static string ReadIsoFile(string filename)
//    {
//      try
//      {
//        var isf = IsoStore.GetIsolatedStorageFile(); //.Trace.WriteLine("ISO:/> "+isf.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isf)); 

//        if (isf.GetFileNames(filename).Length <= 0) return null;

//        using (var stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, isf))
//        {
//          if (stream.Length <= 0) return null;

//          //.Trace.WriteLine("ISO:/> "+stream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream).ToString()); //Retrieve the actual path of the file using reflection.


//          using (StreamReader streamReader = new StreamReader(stream))
//          {
//            var rv = streamReader.ReadToEnd();
//            streamReader.Close ( );
//            return rv;
//          }
//        }
//      }
//      catch (Exception ex) { ex.Log(); }

//      return null;
//    }
//    public static string GetIsoFolder()
//    {
//      try
//      {
//        var isf = IsoStore.GetIsolatedStorageFile();
//        return isf.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(isf).ToString(); //instead of creating a temp file and get the location you can get the path from the store directly: 
//      }
//      catch (Exception ex) { ex.Log(); }

//      return null;
//    }
//  }
//}
