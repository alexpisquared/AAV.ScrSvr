namespace Db.EventLog.Explorer;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    try
    {
      base.OnStartup(e);
      ShutdownMode = ShutdownMode.OnLastWindowClose;

      // first, close any instances of this app that are already running
      foreach (var process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
        if (process.Id != Process.GetCurrentProcess().Id)
          process.Kill();

      //todo: use serilog:
      AAV.Sys.Helpers.Tracer.SetupTracingOptions("EvLogExplr", new TraceSwitch("OnlyUsedWhenInConfig", "This is the trace for all               messages... but who cares?") { Level = TraceLevel.Verbose });
      //tmi: WriteLine($"\r\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.f} App.OnStartup() -- e.Args.Length:{e.Args.Length}, e.Args[0]:{e.Args.FirstOrDefault()}, {Environment.CommandLine}");

      if (e.Args.Length > 0 && File.Exists(e.Args.First()))
        new RODBView(e.Args.First(), SeriLogHelper.CreateLogger<RODBView>("EventLog.Explorer", "+Info -Verb +Infi")).ShowDialog();
      else
        new MainEvLogExplr(SeriLogHelper.CreateLogger<MainEvLogExplr>("EventLog.Explorer", "+Info -Verb +Infi")).ShowDialog();

      //Bpr.BeepEnd3();
    }
    catch (Exception ex) { ex.Pop(); ; }
    //tmi: finally { WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.f} App.OnStartup() -- _END_"); }
  }
}
