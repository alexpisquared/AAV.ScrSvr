namespace OleksaScrSvr;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class App : System.Windows.Application
{
  bool _mustLogEORun;
  string _audit = "audit is unassigned";
  readonly DateTimeOffset _appStarted = DateTimeOffset.Now;

  public IServiceProvider ServiceProvider { get; }

  public App()
  {
    IServiceCollection services = new ServiceCollection();

    AppStartHelper.InitAppSvcs(services);

    MvvmInitHelper.InitMVVM(services);

    _ = services.AddSingleton<IAddChild, MainNavView>();
    _ = services.AddSingleton<MainNavView>(s => new MainNavView(s.GetRequiredService<ILogger>(), s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<IBpr>()) { DataContext = s.GetRequiredService<MainVM>() });

    ServiceProvider = services.BuildServiceProvider();

    ShutdownMode = ShutdownMode.OnMainWindowClose;
  }

  protected override async void OnStartup(StartupEventArgs e)
  {
    UnhandledExceptionHndlr.Logger = ServiceProvider.GetRequiredService<ILogger>();
    Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(15000));

    //if (Debugger.IsAttached) while (true) { await AppStartHelper.Testc(); Debugger.Break(); }

    ServiceProvider.GetRequiredService<INavSvc>().Navigate();

    MainWindow = ServiceProvider.GetRequiredService<MainNavView>();
    MainWindow.Show();

    _audit = VersionHelper.DevDbgAudit(ServiceProvider.GetRequiredService<IConfigurationRoot>(), "°");

    base.OnStartup(e);

    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╞══{TimeSoFar} {_audit}");

    var mainVM = (MainVM)MainWindow.DataContext;  // mainVM.DeploymntSrcExe = Settings.Default.DeplSrcExe; //todo: for future only.    
    _ = await mainVM.InitAsync();                 // blocking due to vesrion checker.
    _ = await TimedSleepAndExit(StandardLib.Consts.ScrSvrPresets.MinToPcSleep);
  }
  protected override async void OnExit(ExitEventArgs e)
  {
    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit \n██");

    LogScrSvrUptimeOncePerSession("ScrSvr - Dn - OnExit.");

    if (Current is not null) Current.DispatcherUnhandledException -= UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    //_serviceProvider.GetRequiredService<OleksaScrSvrModel>().Dispose();

    if (DateTime.Now == DateTime.Today) LogAllLevels(ServiceProvider.GetRequiredService<ILogger>());

    await ServiceProvider.GetRequiredService<IBpr>().AppFinishAsync();

    base.OnExit(e);
  }

  async Task<bool> TimedSleepAndExit(double min2sleep)
  {
    var speech = ServiceProvider.GetRequiredService<SpeechSynth>();
    var logger = ServiceProvider.GetRequiredService<ILogger>();

    Current.Resources["ExecutionDuration"] = new Duration(TimeSpan.FromMinutes(min2sleep));

    try
    {
      //if (!autoSleep) { await speech.SpeakAsync("Armed! Sleepless mode."); return false; }

      await Task.Delay(TimeSpan.FromSeconds(48)); // grace period 1
      speech.SpeakFAF($"Really?");
      await Task.Delay(TimeSpan.FromSeconds(12)); // grace period 2

      if (DevOps.IsDbg == false)
      {
        _mustLogEORun = true;
        new AsLink.EvLogHelper().LogScrSvrBgn(300);         // 300 sec of idle has passed
        speech.SpeakFAF($"Armed!");
      }

      await Task.Delay(TimeSpan.FromMinutes(min2sleep - 1));  /**/  speech.SpeakFAF($"Turning off in a minute.");
      await Task.Delay(TimeSpan.FromMinutes(1)); /**/         await speech.SpeakAsync($"Sorry...");

      LogScrSvrUptimeOncePerSession("ScrSvr - Dn - PC sleep enforced by the screen saver.");

      var sleepStart = DateTimeOffset.Now;
      logger.Log(LogLevel.Information, $"+{TimeSoFar}  SetSuspendState(); ■ never?! goes beyond this on NUC2, GRAM1, RAZER1, ..  \n█··· "); _ = SetSuspendState(hiberate: false, forceCritical: false, disableWakeEvent: false);
      logger.Log(LogLevel.Information, $"+{TimeSoFar}  Process()..Close();  !!! Wake time !!!  Slept for {VersionHelper.TimeAgo(DateTimeOffset.Now - sleepStart),8} \n██··"); Process.GetCurrentProcess().Close();
      //gger.Log(LogLevel.Information, $"+{TimeSoFar}  Process().Kill();    \n███·"); Process.GetCurrentProcess().Kill();

      // never gets here: 
      //Environment.Exit(87);
      //Environment.FailFast("Environment.FailFast");
    }
    catch (Exception ex) { logger.LogError(ex, _audit); }

    return true;
  }

  void LogScrSvrUptimeOncePerSession(string msg)
  {
    if (_mustLogEORun)
    {
      _mustLogEORun = false;

      if (DevOps.IsDbg == false)
      {
        new AsLink.EvLogHelper().LogScrSvrEnd(_appStarted.DateTime.AddSeconds(-240), msg);
      }
    }
  }
  string TimeSoFar => $"{VersionHelper.TimeAgo(DateTimeOffset.Now - _appStarted),8}";

  void LogAllLevels(ILogger lgr)
  {
    lgr.Log(LogLevel.Warning,     /**/ "│   °°° The current LogLevel is:");
    lgr.Log(LogLevel.Trace,       /**/ "│   °°° LogLevel.Trace");
    lgr.Log(LogLevel.Debug,       /**/ "│   °°° LogLevel.Debug");
    lgr.Log(LogLevel.Information, /**/ "│   °°° LogLevel.Information");
    lgr.Log(LogLevel.Warning,     /**/ "│   °°° LogLevel.Warning");
    lgr.Log(LogLevel.Error,       /**/ "│   °°° LogLevel.Error");
    lgr.Log(LogLevel.Critical,    /**/ "│   °°° LogLevel.Critical");
    lgr.Log(LogLevel.None,        /**/ "│   °°° LogLevel.None - is the highest level ... higher than Critical");
  }

  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
}