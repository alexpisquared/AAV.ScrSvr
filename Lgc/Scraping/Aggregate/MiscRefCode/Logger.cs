//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;

//namespace AAV.CustomControlLibrary
//{
//  public static class Logger
//  {
//    static bool _showMsgBox = true;
//    public static void LogXmlMsgAsync(string msg) => Task.Factory.StartNew(() => LogXmlMsg(msg, 20));
//    public static void LogXmlMsg(string msg, int delaySec)
//    {
//      //Default to the version of the currently executing Assembly
//      //Version v = Assembly.GetExecutingAssembly().GetName().Version;
//      ////Check to see if we are ClickOnce Deployed. //i.e. the executing code was installed via ClickOnce
//      //if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
//      //{
//      //  //Collect the ClickOnce Current Version
//      //  v = ApplicationDeployment.CurrentDeployment.CurrentVersion;
//      //}

//      var fn = LogFolder + "ErrorLog.xml";
//      var assemName = Assembly.GetExecutingAssembly().GetName();

//      try
//      {
//        var now = DateTime.Now;
//        Thread.Sleep(delaySec * 1000);
//        if (Directory.Exists(Path.GetDirectoryName(fn)))
//          File.AppendAllText(fn,
//            string.Format("<row><time>{0:ddd MMM-dd HH:mm:ss}</time><asm>{1}</asm><ver>{2} (Beta 0.917)</ver><er>{3}</er><msg>{4}</msg><bindump>{5}</bindump><tick>{6}</tick></row>\r\n",
//            now, assemName.Name, assemName.Version, "N/A", msg, Crypto.EncryptStringAES(Environment.UserName, "0xFF8040"), now.Ticks));
//      }
//      catch (Exception ex)
//      {
//        LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
//      }
//    }

//    public static string LogException(Exception ex, MethodBase methodBase) => LogException(ex, methodBase, "");
//    public static string LogException(Exception ex, MethodBase methodBase, string dtlMsg)
//    {
//      var s = string.Format("\nEx:{0:MMM-dd ddd HH:mm:ss}  in \n\t{1}.{2}():\n\t{3}{4}{5}\n{6}",
//              DateTime.Now,
//              methodBase.DeclaringType.Name, methodBase.Name,
//              ex.Message,
//              ex.InnerException == null ? "" : "\n\t" + ex.InnerException.Message,
//              string.IsNullOrEmpty(dtlMsg) ? "" : "\n\t" + dtlMsg,
//              ex.StackTrace);

//      Trace.WriteLine(s);

//      if (_showMsgBox && (Environment.UserName.ToLower().Contains("igid") || Environment.UserName.ToLower().Contains("alex")))
//      {
//        try
//        {
//          System.Windows.Clipboard.SetText(methodBase.Name);
//          if (MsgBox.StdMsgBox(dtlMsg + "\r\n\n" + s + "\r\n\nCheck Clipboard for details.\r\n\nNo - no more of this pop-ups.", System.Windows.MessageBoxButton.YesNoCancel) == MessageBoxResult.No)
//            _showMsgBox = false;
//        }
//        catch (Exception ex2)
//        {
//          //Trace.WriteLine("     *&^%$#@!~:" + ex2.Message+"\r\n");
//          if (MsgBox.StdMsgBox(dtlMsg + "\r\n\n" + s + "\r\n\nNo - no more of this pop-ups.\r\n\n...Clipboard is not available: " + ex2.Message, System.Windows.MessageBoxButton.YesNoCancel) == MessageBoxResult.No)
//            _showMsgBox = false;
//        }
//      }

//      return s;
//    }


//    public static string LogFolder
//    {
//      get
//      {
//        if ((Environment.UserName.ToLower().Contains("igid") || Environment.UserName.ToLower().Contains("alex")))
//        {
//          try
//          {
//            if (!Directory.Exists(@"C:\Temp")) Directory.CreateDirectory(@"C:\Temp");
//            if (!Directory.Exists(@"C:\Temp\error.log")) Directory.CreateDirectory(@"C:\Temp\error.log");
//          }
//          catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }

//          return @"C:\temp\error.log\";
//        }
//        else
//        {
//          return @"\\dione\DevelopmentWWW2\DeveloperApps\apigida\EPPlus\Application Files\";
//        }

//        //return //Environment.UserName == "apigida" ? @"C:\temp\error.log\" : 
//        //          @"\\dione\DevelopmentWWW2\DeveloperApps\apigida\EPPlus\Application Files\";
//      }
//    }

//    /// <summary>
//    /// Note: 
//    /// To enable/disable tracing - use 'Define TRACE Constant' checkbox on the Build tab of the project properties.
//    /// </summary>
//    public static void SetupTracingOptions(string appName)
//    {
//      try
//      {
//#if DEBUG
//				string compileMode = "Dbg";
//#else
//        var compileMode = "Rls";
//#endif
//        if ((Environment.UserName.ToLower().Contains("igid") || Environment.UserName.ToLower().Contains("alex")))
//        {
//          var logFile = @"C:\Temp\error.log\" + appName + "." + compileMode + "." + DateTime.Now.ToString("MMMdd") + ".txt";//TODO: Path.Combine(Application.UserAppDataPath, Application.ProductName + "." + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\\", "-") + ".ExceptionLog.txt");
//          Trace.Listeners.Add(new TextWriterTraceListener(logFile));
//          Trace.AutoFlush = true;
//          Trace.WriteLine(string.Format("{0} - started by {1}.{2}", DateTime.Now.ToString("\nHH:mm:ss"), Environment.MachineName, Environment.UserName));
//        }
//      }
//      catch
//      {
//        //ignore folder access security exception and continue.
//      }
//    }

//  }


//}
