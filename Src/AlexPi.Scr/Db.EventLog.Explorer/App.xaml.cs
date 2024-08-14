namespace Db.EventLog.Explorer;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    try
    {
      base.OnStartup(e);
      ShutdownMode = ShutdownMode.OnLastWindowClose;

      //todo: use serilog:
      AAV.Sys.Helpers.Tracer.SetupTracingOptions("EvLogExplr", new TraceSwitch("OnlyUsedWhenInConfig", "This is the trace for all               messages... but who cares?") { Level = TraceLevel.Verbose });
      WriteLine($"\r\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.f} App.OnStartup() -- e.Args.Length:{e.Args.Length}, e.Args[0]:{e.Args.FirstOrDefault()}, {Environment.CommandLine}");

      if (e.Args.Length > 0 && File.Exists(e.Args.First()))
        new RODBView(e.Args.First()).ShowDialog();
      else
        new MainEvLogExplr().ShowDialog();

      //Bpr.BeepEnd3();
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.f} App.OnStartup() -- _END_"); }
  }
}
