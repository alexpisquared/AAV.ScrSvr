using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Db.EventLog.Explorer
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      try
      {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        //Bpr.BeepBgn3();

#if !NOT_NEEDED
        Tracer.SetupTracingOptions("EvLogExplr", new TraceSwitch("OnlyUsedWhenInConfig", "This is the trace for all               messages... but who cares?") { Level = TraceLevel.Verbose });
#endif
        Trace.WriteLine($"\r\n{DateTime.Now:yy.MM.dd HH:mm:ss.f} App.OnStartup() -- e.Args.Length:{e.Args.Length}, e.Args[0]:{e.Args.FirstOrDefault()}, {Environment.CommandLine}");

        if (e.Args.Length > 0 && File.Exists(e.Args.First()))
          new RODBView(e.Args.First()).ShowDialog();
        else
          new MainEvLogExplr().ShowDialog();

        AAV.Sys.Helpers.Bpr.BeepEnd3();
      }
      catch (Exception ex) { ex.Pop(); ; }
      finally { Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} App.OnStartup() -- _END_"); }
    }
  }
}
